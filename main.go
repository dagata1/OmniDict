package main

import (
	"bytes"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"image"
	"image/png"
	"io"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"syscall"
	"time"
	"unsafe"

	"github.com/pelletier/go-toml/v2"
)

const (
	MOD_ALT      = 0x0001
	MOD_CONTROL  = 0x0002
	MOD_NOREPEAT = 0x4000
	VK_T         = 0x54
	VK_Q         = 0x51
	WM_HOTKEY    = 0x0312

	HOTKEY_CTRL_T = 1001
	HOTKEY_ALT_Q  = 1002

	CF_DIB         = 8
	CF_UNICODETEXT = 13
)

var (
	user32 = syscall.NewLazyDLL("user32.dll")
	regHK  = user32.NewProc("RegisterHotKey")
	unregHK = user32.NewProc("UnregisterHotKey")
	getMsg = user32.NewProc("GetMessageW")
	msgBox = user32.NewProc("MessageBoxW")

	isClipboardFormatAvailable = user32.NewProc("IsClipboardFormatAvailable")
	openClipboard              = user32.NewProc("OpenClipboard")
	closeClipboard             = user32.NewProc("CloseClipboard")
	getClipboardData           = user32.NewProc("GetClipboardData")

	kernel32     = syscall.NewLazyDLL("kernel32.dll")
	globalLock   = kernel32.NewProc("GlobalLock")
	globalUnlock = kernel32.NewProc("GlobalUnlock")
)

type BITMAPINFOHEADER struct {
	BiSize          uint32
	BiWidth         int32
	BiHeight        int32
	BiPlanes        uint16
	BiBitCount      uint16
	BiCompression   uint32
	BiSizeImage     uint32
	BiXPelsPerMeter int32
	BiYPelsPerMeter int32
	BiClrUsed       uint32
	BiClrImportant  uint32
}

type MSG struct {
	Hwnd    uintptr
	Message uint32
	WParam  uintptr
	LParam  uintptr
	Time    uint32
	Pt      struct{ X, Y int32 }
}

type AppConfig struct {
	BaseURL string
	APIKey  string
	Model   string
}

func LoadCodexConfig() (*AppConfig, error) {
	home, err := os.UserHomeDir()
	if err != nil {
		return nil, err
	}
	cfgFile := filepath.Join(home, ".codex", "config.toml")
	data, err := os.ReadFile(cfgFile)
	if err != nil {
		return nil, err
	}

	var raw struct {
		Model          string `toml:"model"`
		ModelProvider  string `toml:"model_provider"`
		ModelProviders map[string]struct {
			BaseURL                 string `toml:"base_url"`
			ExperimentalBearerToken string `toml:"experimental_bearer_token"`
		} `toml:"model_providers"`
	}

	if err := toml.Unmarshal(data, &raw); err != nil {
		return nil, err
	}

	cfg := &AppConfig{
		Model:   raw.Model,
		BaseURL: "https://ai.kncloud.top/v1",
	}
	if cfg.Model == "" {
		cfg.Model = "gemini-3.8-flash-high"
	}

	pKey := raw.ModelProvider
	if pKey == "" {
		pKey = "KNcloud"
	}
	if p, ok := raw.ModelProviders[pKey]; ok {
		cfg.BaseURL = strings.TrimRight(p.BaseURL, "/")
		if !strings.HasSuffix(cfg.BaseURL, "/v1") {
			cfg.BaseURL += "/v1"
		}
		cfg.APIKey = p.ExperimentalBearerToken
	}
	return cfg, nil
}

func HasClipboardImage() bool {
	avail, _, _ := isClipboardFormatAvailable.Call(CF_DIB)
	return avail != 0
}

func GetClipboardImagePNG() ([]byte, error) {
	runtime.LockOSThread()
	defer runtime.UnlockOSThread()

	avail, _, _ := isClipboardFormatAvailable.Call(CF_DIB)
	if avail == 0 {
		return nil, fmt.Errorf("no image in clipboard")
	}

	r, _, _ := openClipboard.Call(0)
	if r == 0 {
		time.Sleep(20 * time.Millisecond)
		r, _, _ = openClipboard.Call(0)
		if r == 0 {
			return nil, fmt.Errorf("cannot open clipboard")
		}
	}
	defer closeClipboard.Call()

	h, _, _ := getClipboardData.Call(CF_DIB)
	if h == 0 {
		return nil, fmt.Errorf("no DIB handle")
	}

	ptr, _, _ := globalLock.Call(h)
	if ptr == 0 {
		return nil, fmt.Errorf("lock failed")
	}
	defer globalUnlock.Call(h)

	header := (*BITMAPINFOHEADER)(unsafe.Pointer(ptr))
	width := int(header.BiWidth)
	height := int(header.BiHeight)
	topDown := false
	if height < 0 {
		topDown = true
		height = -height
	}

	if width <= 0 || height <= 0 || (header.BiBitCount != 24 && header.BiBitCount != 32) {
		return nil, fmt.Errorf("unsupported image format")
	}

	offset := uintptr(header.BiSize)
	rowSize := ((width*int(header.BiBitCount) + 31) / 32) * 4
	pixelData := unsafe.Pointer(ptr + offset)

	img := image.NewRGBA(image.Rect(0, 0, width, height))
	bytesPerPixel := int(header.BiBitCount) / 8

	for y := 0; y < height; y++ {
		srcY := y
		if !topDown {
			srcY = height - 1 - y
		}
		rowPtr := unsafe.Pointer(uintptr(pixelData) + uintptr(srcY*rowSize))
		for x := 0; x < width; x++ {
			pxPtr := unsafe.Pointer(uintptr(rowPtr) + uintptr(x*bytesPerPixel))
			b := *(*byte)(pxPtr)
			g := *(*byte)(unsafe.Pointer(uintptr(pxPtr) + 1))
			r := *(*byte)(unsafe.Pointer(uintptr(pxPtr) + 2))
			a := uint8(255)
			if bytesPerPixel == 4 {
				a = *(*byte)(unsafe.Pointer(uintptr(pxPtr) + 3))
				if a == 0 {
					a = 255
				}
			}
			outIdx := (y*width + x) * 4
			img.Pix[outIdx] = r
			img.Pix[outIdx+1] = g
			img.Pix[outIdx+2] = b
			img.Pix[outIdx+3] = a
		}
	}

	var buf bytes.Buffer
	if err := png.Encode(&buf, img); err != nil {
		return nil, err
	}
	return buf.Bytes(), nil
}

func CallAI(cfg *AppConfig, imageBytes []byte) (string, error) {
	b64 := base64.StdEncoding.EncodeToString(imageBytes)
	endpoint := cfg.BaseURL + "/chat/completions"

	prompt := `你是一位全能的英语私教助手。用户在玩游戏（如星露谷物语、RPG、Steam游戏）或看屏幕时截取了此画面。
请提取截图中出现的英文单词、短语或对话，按以下格式输出精炼解析：
【中文含义】：整句或核心词的中文意思
【核心词汇与语法】：音标、词性、重点用法解析
【例句】：贴近生活或游戏情景的自然例句`

	reqMap := map[string]interface{}{
		"model": cfg.Model,
		"messages": []map[string]interface{}{
			{
				"role": "user",
				"content": []map[string]interface{}{
					{"type": "text", "text": prompt},
					{"type": "image_url", "image_url": map[string]string{"url": "data:image/png;base64," + b64}},
				},
			},
		},
	}

	jb, _ := json.Marshal(reqMap)
	httpReq, err := http.NewRequest("POST", endpoint, bytes.NewBuffer(jb))
	if err != nil {
		return "", err
	}
	httpReq.Header.Set("Content-Type", "application/json")
	httpReq.Header.Set("Authorization", "Bearer "+cfg.APIKey)
	httpReq.Header.Set("User-Agent", "Codex/1.0")

	client := &http.Client{Timeout: 35 * time.Second}
	resp, err := client.Do(httpReq)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}

	var resObj struct {
		Choices []struct {
			Message struct {
				Content string `json:"content"`
			} `json:"message"`
		} `json:"choices"`
		Error *struct {
			Message string `json:"message"`
		} `json:"error"`
	}

	if err := json.Unmarshal(body, &resObj); err != nil {
		return "", err
	}
	if resObj.Error != nil {
		return "", fmt.Errorf("%s", resObj.Error.Message)
	}
	if len(resObj.Choices) == 0 {
		return "", fmt.Errorf("无响应")
	}
	return resObj.Choices[0].Message.Content, nil
}

// ShowResultPopup shows a native modern WPF popup window with HTML/rich text formatting and ESC close
func ShowResultPopup(resultText string) {
	tempFile := filepath.Join(os.TempDir(), "gamedict_last_result.txt")
	_ = os.WriteFile(tempFile, []byte(resultText), 0644)

	psScript := fmt.Sprintf(`
Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

$win = New-Object System.Windows.Window
$win.Title = "GameDict AI - 查词解析"
$win.Width = 440
$win.Height = 560
$win.WindowStyle = [System.Windows.WindowStyle]::None
$win.AllowsTransparency = $true
$win.Background = [System.Windows.Media.Brushes]::Transparent
$win.Topmost = $true
$win.WindowStartupLocation = [System.Windows.WindowStartupLocation]::CenterScreen

$border = New-Object System.Windows.Controls.Border
$border.CornerRadius = New-Object System.Windows.CornerRadius 12
$border.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(245, 250, 250, 252))
$border.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(60, 0, 0, 0))
$border.BorderThickness = New-Object System.Windows.Thickness 1
$border.Margin = New-Object System.Windows.Thickness 8

$shadow = New-Object System.Windows.Media.Effects.DropShadowEffect
$shadow.BlurRadius = 16
$shadow.Color = [System.Windows.Media.Colors]::Black
$shadow.Opacity = 0.25
$shadow.ShadowDepth = 3
$border.Effect = $shadow

$grid = New-Object System.Windows.Controls.Grid
$grid.Margin = New-Object System.Windows.Thickness 16

$row0 = New-Object System.Windows.Controls.RowDefinition
$row0.Height = [System.Windows.GridLength]::Auto
$row1 = New-Object System.Windows.Controls.RowDefinition
$row1.Height = New-Object System.Windows.GridLength 1, [System.Windows.GridUnitType]::Star
$row2 = New-Object System.Windows.Controls.RowDefinition
$row2.Height = [System.Windows.GridLength]::Auto
$grid.RowDefinitions.Add($row0)
$grid.RowDefinitions.Add($row1)
$grid.RowDefinitions.Add($row2)

# Header
$header = New-Object System.Windows.Controls.DockPanel
$header.LastChildFill = $false
$header.Margin = New-Object System.Windows.Thickness 0, 0, 0, 10
$header.Add_MouseLeftButtonDown({ $win.DragMove() })

$title = New-Object System.Windows.Controls.TextBlock
$title.Text = "GameDict AI 词典解析"
$title.FontWeight = [System.Windows.FontWeights]::SemiBold
$title.FontSize = 14
$title.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(30, 30, 30))
[System.Windows.Controls.DockPanel]::SetDock($title, [System.Windows.Controls.Dock]::Left)
$header.Children.Add($title)

$closeBtn = New-Object System.Windows.Controls.Button
$closeBtn.Content = "✕"
$closeBtn.Width = 24
$closeBtn.Height = 24
$closeBtn.Background = [System.Windows.Media.Brushes]::Transparent
$closeBtn.BorderThickness = New-Object System.Windows.Thickness 0
$closeBtn.Cursor = [System.Windows.Input.Cursors]::Hand
$closeBtn.Add_Click({ $win.Close() })
[System.Windows.Controls.DockPanel]::SetDock($closeBtn, [System.Windows.Controls.Dock]::Right)
$header.Children.Add($closeBtn)

[System.Windows.Controls.Grid]::SetRow($header, 0)
$grid.Children.Add($header)

# Scrollable Content
$scroll = New-Object System.Windows.Controls.ScrollViewer
$scroll.VerticalScrollBarVisibility = [System.Windows.Controls.ScrollBarVisibility]::Auto

$textBlock = New-Object System.Windows.Controls.TextBlock
$content = [System.IO.File]::ReadAllText('%s')
$textBlock.Text = $content
$textBlock.TextWrapping = [System.Windows.TextWrapping]::Wrap
$textBlock.FontSize = 13.5
$textBlock.LineHeight = 22
$textBlock.FontFamily = New-Object System.Windows.Media.FontFamily("Segoe UI, Microsoft YaHei")
$scroll.Content = $textBlock

$card = New-Object System.Windows.Controls.Border
$card.CornerRadius = New-Object System.Windows.CornerRadius 8
$card.Background = [System.Windows.Media.Brushes]::White
$card.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(35, 0, 0, 0))
$card.BorderThickness = New-Object System.Windows.Thickness 1
$card.Padding = New-Object System.Windows.Thickness 12
$card.Child = $scroll

[System.Windows.Controls.Grid]::SetRow($card, 1)
$grid.Children.Add($card)

# Footer
$footer = New-Object System.Windows.Controls.TextBlock
$footer.Text = "按 Esc 或右上角 ✕ 键即可关闭"
$footer.FontSize = 11
$footer.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(120, 120, 120))
$footer.Margin = New-Object System.Windows.Thickness 0, 8, 0, 0
[System.Windows.Controls.Grid]::SetRow($footer, 2)
$grid.Children.Add($footer)

$win.Add_KeyDown({
    if ($_.Key -eq [System.Windows.Input.Key]::Escape) {
        $win.Close()
    }
})

$border.Child = $grid
$win.Content = $border
$win.ShowDialog() | Out-Null
`, strings.ReplaceAll(tempFile, "\\", "\\\\"))

	cmd := exec.Command("powershell", "-ExecutionPolicy", "Bypass", "-NoProfile", "-NonInteractive", "-Command", psScript)
	cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	_ = cmd.Run()
}

func showToast(msg string) {
	cmd := exec.Command("powershell", "-NoProfile", "-NonInteractive", "-Command", fmt.Sprintf(`[System.Windows.Forms.MessageBox]::Show('%s', 'GameDict AI')`, msg))
	cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
	_ = cmd.Start()
}

func main() {
	cfg, err := LoadCodexConfig()
	if err != nil {
		t, _ := syscall.UTF16PtrFromString("无法读取 ~/.codex/config.toml: " + err.Error())
		title, _ := syscall.UTF16PtrFromString("GameDict AI 错误")
		msgBox.Call(0, uintptr(unsafe.Pointer(t)), uintptr(unsafe.Pointer(title)), 0)
		return
	}

	runtime.LockOSThread()
	defer runtime.UnlockOSThread()

	// 注册全局热键
	r1, _, _ := regHK.Call(0, uintptr(HOTKEY_CTRL_T), uintptr(MOD_CONTROL|MOD_NOREPEAT), uintptr(VK_T))
	if r1 == 0 {
		regHK.Call(0, uintptr(HOTKEY_CTRL_T), uintptr(MOD_CONTROL), uintptr(VK_T))
	}
	defer unregHK.Call(0, uintptr(HOTKEY_CTRL_T))

	r2, _, _ := regHK.Call(0, uintptr(HOTKEY_ALT_Q), uintptr(MOD_ALT|MOD_NOREPEAT), uintptr(VK_Q))
	if r2 == 0 {
		regHK.Call(0, uintptr(HOTKEY_ALT_Q), uintptr(MOD_ALT), uintptr(VK_Q))
	}
	defer unregHK.Call(0, uintptr(HOTKEY_ALT_Q))

	// 启动提示弹窗 (非阻塞)
	go func() {
		time.Sleep(300 * time.Millisecond)
		tip, _ := syscall.UTF16PtrFromString(fmt.Sprintf("GameDict AI 已启动并在后台守护！\n\n- 在任何游戏/屏幕任意位置按 [Ctrl + T] 或 [Alt + Q] 截屏\n- 框选后即可自动弹出 AI 深度词典浮窗\n- 浮窗按 [Esc] 或右上角直接关闭\n\n模型：%s (Codex配置已复用)", cfg.Model))
		title, _ := syscall.UTF16PtrFromString("GameDict AI")
		msgBox.Call(0, uintptr(unsafe.Pointer(tip)), uintptr(unsafe.Pointer(title)), 0x00000040)
	}()

	var msg MSG
	for {
		r, _, _ := getMsg.Call(uintptr(unsafe.Pointer(&msg)), 0, 0, 0)
		if int32(r) <= 0 {
			break
		}
		if msg.Message == WM_HOTKEY {
			go func() {
				// 1. 启动 Windows 截图工具
				cmd := exec.Command("cmd", "/c", "start", "ms-screenclip:")
				cmd.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
				_ = cmd.Run()

				// 2. 监听剪贴板
				for i := 0; i < 80; i++ {
					time.Sleep(350 * time.Millisecond)
					if HasClipboardImage() {
						pngBytes, err := GetClipboardImagePNG()
						if err == nil && len(pngBytes) > 0 {
							// 查词
							res, err := CallAI(cfg, pngBytes)
							if err != nil {
								ShowResultPopup("【错误提示】:\n" + err.Error())
							} else {
								ShowResultPopup(res)
							}
							break
						}
					}
				}
			}()
		}
	}
}
