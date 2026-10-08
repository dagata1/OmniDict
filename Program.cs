using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OmniDictApp {
    public class Program {
        private static System.Threading.Mutex appMutex;
        [STAThread]
        public static void Main(string[] args) {
            bool createdNew;
            appMutex = new System.Threading.Mutex(true, "OmniDictAI_SingleInstance_Mutex", out createdNew);
            if (!createdNew) {
                return;
            }
            string _lp=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"OmniDict","omnidict.log");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_lp));
            try{using(var _fs2=new System.IO.FileStream(_lp,System.IO.FileMode.Append,System.IO.FileAccess.Write,System.IO.FileShare.ReadWrite))
                using(var _sw2=new System.IO.StreamWriter(_fs2,System.Text.Encoding.UTF8))
                    _sw2.Write(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+" [INFO ] App.Main start\n");}catch{}
            AppDomain.CurrentDomain.UnhandledException+=(s,e)=>{try{System.IO.File.AppendAllText(_lp,System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+" [FATAL] "+e.ExceptionObject+"\n",System.Text.Encoding.UTF8);}catch{}};
            // csc-built .NET 4.x apps without an app.config may default to legacy TLS; make sure TLS 1.2 is enabled.
            try { ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12; } catch {}
            try { new App().Run(new MainWindow()); }
            catch(Exception ex){try{System.IO.File.AppendAllText(_lp,System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+" [FATAL] "+ex.ToString()+"\n",System.Text.Encoding.UTF8);}catch{}}
        }
    }

    public class App : Application {
        protected override void OnStartup(StartupEventArgs e) { base.OnStartup(e); }
    }

    // Windows 11 Native Theme & DWM Helper
    public static class Win11Theme {
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public const int DWMWCP_ROUND = 2;

        public static bool IsDarkTheme { get; private set; }
        public static event Action ThemeChanged;

        static Win11Theme() {
            DetectTheme();
            try {
                SystemEvents.UserPreferenceChanged += (s, e) => {
                    bool old = IsDarkTheme;
                    DetectTheme();
                    if (old != IsDarkTheme && ThemeChanged != null) {
                        ThemeChanged();
                    }
                };
            } catch {}
        }

        public static void DetectTheme() {
            try {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")) {
                    if (key != null) {
                        object val = key.GetValue("AppsUseLightTheme");
                        if (val is int) {
                            IsDarkTheme = ((int)val) == 0;
                            return;
                        }
                    }
                }
            } catch {}
            IsDarkTheme = true;
        }

        public static void ApplyToWindow(Window window) {
            try {
                var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero) return;
                int dark = IsDarkTheme ? 1 : 0;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
                int corner = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
            } catch {}
        }

        public static Color BgWindow { get { return IsDarkTheme ? Color.FromRgb(20, 20, 20) : Color.FromRgb(243, 243, 243); } }
        public static Color BgSurface { get { return IsDarkTheme ? Color.FromRgb(32, 32, 32) : Color.FromRgb(255, 255, 255); } }
        public static Color BgCard { get { return IsDarkTheme ? Color.FromRgb(40, 40, 40) : Color.FromRgb(255, 255, 255); } }
        public static Color BgHover { get { return IsDarkTheme ? Color.FromRgb(50, 50, 50) : Color.FromRgb(235, 235, 235); } }
        public static Color FgPrimary { get { return IsDarkTheme ? Color.FromRgb(240, 240, 240) : Color.FromRgb(25, 25, 25); } }
        public static Color FgSecondary { get { return IsDarkTheme ? Color.FromRgb(160, 160, 160) : Color.FromRgb(95, 95, 95); } }
        public static Color FgTertiary { get { return IsDarkTheme ? Color.FromRgb(110, 110, 110) : Color.FromRgb(140, 140, 140); } }
        public static Color BorderSubtle { get { return IsDarkTheme ? Color.FromArgb(40, 255, 255, 255) : Color.FromArgb(40, 0, 0, 0); } }
        public static Color BorderStrong { get { return IsDarkTheme ? Color.FromArgb(70, 255, 255, 255) : Color.FromArgb(70, 0, 0, 0); } }
        public static Color Accent { get { return Color.FromRgb(0, 103, 192); } }
        public static Color AccentHover { get { return Color.FromRgb(24, 120, 210); } }

        public static Style CreateButtonStyle(bool isPrimary = false) {
            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(Button.FontSizeProperty, 12.0));
            style.Setters.Add(new Setter(Button.FontFamilyProperty, new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")));

            string xaml;
            if (isPrimary) {
                xaml = "<ControlTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" TargetType=\"Button\">" +
                       "  <Border Name=\"bd\" CornerRadius=\"4\" Background=\"#FF0067C0\" BorderThickness=\"0\" Padding=\"{TemplateBinding Padding}\">" +
                       "    <ContentPresenter HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>" +
                       "  </Border>" +
                       "  <ControlTemplate.Triggers>" +
                       "    <Trigger Property=\"IsMouseOver\" Value=\"True\"><Setter TargetName=\"bd\" Property=\"Background\" Value=\"#FF1878D2\"/></Trigger>" +
                       "    <Trigger Property=\"IsPressed\" Value=\"True\"><Setter TargetName=\"bd\" Property=\"Background\" Value=\"#FF005BB5\"/></Trigger>" +
                       "    <Trigger Property=\"IsEnabled\" Value=\"False\"><Setter TargetName=\"bd\" Property=\"Opacity\" Value=\"0.4\"/></Trigger>" +
                       "  </ControlTemplate.Triggers>" +
                       "</ControlTemplate>";
                style.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            } else {
                string bg = IsDarkTheme ? "#2A2A2A" : "#FAFAFA";
                string bgh = IsDarkTheme ? "#383838" : "#EBEBEB";
                string bgp = IsDarkTheme ? "#222222" : "#E0E0E0";
                string border = IsDarkTheme ? "#33FFFFFF" : "#33000000";
                xaml = "<ControlTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" TargetType=\"Button\">" +
                       "  <Border Name=\"bd\" CornerRadius=\"4\" Background=\"" + bg + "\" BorderBrush=\"" + border + "\" BorderThickness=\"1\" Padding=\"{TemplateBinding Padding}\">" +
                       "    <ContentPresenter HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>" +
                       "  </Border>" +
                       "  <ControlTemplate.Triggers>" +
                       "    <Trigger Property=\"IsMouseOver\" Value=\"True\"><Setter TargetName=\"bd\" Property=\"Background\" Value=\"" + bgh + "\"/></Trigger>" +
                       "    <Trigger Property=\"IsPressed\" Value=\"True\"><Setter TargetName=\"bd\" Property=\"Background\" Value=\"" + bgp + "\"/></Trigger>" +
                       "    <Trigger Property=\"IsEnabled\" Value=\"False\"><Setter TargetName=\"bd\" Property=\"Opacity\" Value=\"0.4\"/></Trigger>" +
                       "  </ControlTemplate.Triggers>" +
                       "</ControlTemplate>";
                style.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(FgPrimary)));
            }
            style.Setters.Add(new Setter(Button.TemplateProperty, (ControlTemplate)System.Windows.Markup.XamlReader.Parse(xaml)));
            return style;
        }
    }

    public class PromptPreset {
        public string Name { get; set; }
        public string Content { get; set; }
        public PromptPreset(string name, string content) {
            Name = name;
            Content = content;
        }
    }

    public static class OmniDictConfig {
        public static readonly string AppDataDir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OmniDict");
        public static readonly string ConfigPath = System.IO.Path.Combine(AppDataDir, "omnidict.toml");

        // Old path for migration
        private static readonly string OldConfigPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GameDict", "gamedict.toml");

        // Escape a value for a TOML basic string.
        public static string Esc(string v) {
            if (v == null) return "";
            var sb = new StringBuilder(v.Length + 8);
            foreach (char c in v) {
                switch (c) {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        // Reverse of Esc; processes escapes left to right so "\\n" stays a literal backslash + n.
        public static string Unesc(string v) {
            if (string.IsNullOrEmpty(v) || v.IndexOf('\\') < 0) return v;
            var sb = new StringBuilder(v.Length);
            for (int i = 0; i < v.Length; i++) {
                char c = v[i];
                if (c == '\\' && i + 1 < v.Length) {
                    char n = v[++i];
                    if (n == 'n') sb.Append('\n');
                    else if (n == 't') sb.Append('\t');
                    else if (n == '"') sb.Append('"');
                    else if (n == '\\') sb.Append('\\');
                    else { sb.Append('\\'); sb.Append(n); }
                } else sb.Append(c);
            }
            return sb.ToString();
        }

        // Extract the quoted value of a `key = "..."` line (handles escaped quotes).
        private static string Quoted(string t) {
            int q1 = t.IndexOf('"'); int q2 = t.LastIndexOf('"');
            if (q1 < 0 || q2 <= q1) return null;
            return Unesc(t.Substring(q1 + 1, q2 - q1 - 1));
        }

        private static bool IsKey(string t, string key) {
            if (!t.StartsWith(key)) return false;
            string rest = t.Substring(key.Length).TrimStart();
            return rest.StartsWith("=");
        }

        // API key is encrypted with Windows DPAPI (current user scope) before being written to disk.
        private static readonly byte[] KeyEntropy = Encoding.UTF8.GetBytes("OmniDict.api_key.v1");

        public static string ProtectKey(string plain) {
            if (string.IsNullOrEmpty(plain)) return "";
            byte[] enc = System.Security.Cryptography.ProtectedData.Protect(
                Encoding.UTF8.GetBytes(plain), KeyEntropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(enc);
        }

        public static string UnprotectKey(string b64) {
            if (string.IsNullOrEmpty(b64)) return "";
            try {
                byte[] dec = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(b64), KeyEntropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(dec);
            } catch (Exception ex) {
                Logger.Error("OmniDictConfig.UnprotectKey", ex);
                return null;
            }
        }

        public static List<PromptPreset> GetDefaultPresets() {
            var list = new List<PromptPreset>();
            list.Add(new PromptPreset("游戏本地化与攻略私教",
                "你是一位顶尖的多语言游戏本地化与攻略私教。请识别并精析截图中出现的内容（支持英语、日语、韩语、德语等全语种）：\n" +
                "【中文意思】：结合当前游戏具体画面与语境，给出精准自然、地道的中文翻译；\n" +
                "【核心重点】：提炼关键单词/生词/短语，标出读音/原形与在此处游戏场景下的含义及搭配；\n" +
                "【游戏大师】：结合星露谷物语等游戏情境，给出1~2条关键背景提示、任务推进要点或下步建议；\n" +
                "【顺便学一句】：提取一个最地道、最值得掌握的游戏用语或日常例句。\n" +
                "排版要求紧凑干练，层次分明，无需多余套话。"));

            list.Add(new PromptPreset("极简极速直译",
                "你是一位极高效率的即时翻译助手。请直接识别并翻译截图中的文字：\n" +
                "1. 提供最自然精准的中文译文；\n" +
                "2. 若有关键专业生词，简明列出【单词 - 中文 - 音标】。\n" +
                "输出力求极致精炼，直奔主题。"));

            list.Add(new PromptPreset("程序员技术排错与代码分析",
                "你是一位资深架构师与软件排错专家。请识别截图中出现的代码、错误日志或命令行信息：\n" +
                "【错误根因】：一针见血指明核心原因；\n" +
                "【解决方案】：给出清晰的代码修改或命令修复方案；\n" +
                "【要点解析】：拆解其中涉及的关键技术名词或语法要点。"));

            list.Add(new PromptPreset("外语学习与深度语法精读",
                "你是一位资深外语导师。请针对截图中的语句进行深度语言学精读：\n" +
                "【全文翻译】：提供通顺自然译文；\n" +
                "【词汇详解】：音标、原形、词性、派生词及易混辨析；\n" +
                "【语法长难句】：剖析句子结构、核心时态与从句逻辑；\n" +
                "【例句拓展】：提供地道场景例句。"));

            return list;
        }

        public static bool Load(out string apiBase, out string apiKey, out string model, out string useVision,
                               out double floatX, out double floatY, out string currentPresetName, out List<PromptPreset> presets) {
            apiBase = null; apiKey = null; model = null; useVision = null;
            floatX = -1; floatY = -1; currentPresetName = null;
            presets = new List<PromptPreset>();

            string loadPath = ConfigPath;
            if (!File.Exists(loadPath) && File.Exists(OldConfigPath)) {
                loadPath = OldConfigPath;
            }

            try {
                if (!File.Exists(loadPath)) return false;
                string currentPName = null;
                var currentPContent = new StringBuilder();
                bool readingPreset = false;
                bool legacyPlainKey = false;

                foreach (string line in File.ReadAllLines(loadPath, Encoding.UTF8)) {
                    string t = line.Trim();
                    if (IsKey(t, "api_base")) { apiBase = Quoted(t); }
                    else if (IsKey(t, "api_key_dpapi")) { string k = UnprotectKey(Quoted(t)); if (k != null) apiKey = k; }
                    else if (IsKey(t, "api_key")) { if (apiKey == null) { apiKey = Quoted(t); legacyPlainKey = !string.IsNullOrEmpty(apiKey); } }
                    else if (IsKey(t, "model")) { model = Quoted(t); }
                    else if (t.StartsWith("use_vision")) { useVision = t.Contains("true") ? "true" : null; }
                    else if (IsKey(t, "current_preset")) { currentPresetName = Quoted(t); }
                    else if (t.StartsWith("float_x=") || t.StartsWith("float_x ")) { double.TryParse(t.Split('=')[1].Trim(), out floatX); }
                    else if (t.StartsWith("float_y=") || t.StartsWith("float_y ")) { double.TryParse(t.Split('=')[1].Trim(), out floatY); }
                    else if (t.StartsWith("[[presets]]")) {
                        if (readingPreset && !string.IsNullOrEmpty(currentPName)) {
                            presets.Add(new PromptPreset(currentPName, currentPContent.ToString().TrimEnd()));
                        }
                        readingPreset = true;
                        currentPName = null;
                        currentPContent.Clear();
                    } else if (readingPreset) {
                        if (IsKey(t, "name")) {
                            string n = Quoted(t);
                            if (n != null) currentPName = n;
                        } else if (IsKey(t, "content")) {
                            string c = Quoted(t);
                            if (c != null) currentPContent.Append(c);
                        }
                    }
                }
                if (readingPreset && !string.IsNullOrEmpty(currentPName)) {
                    presets.Add(new PromptPreset(currentPName, currentPContent.ToString().TrimEnd()));
                }

                if (presets.Count == 0) {
                    presets = GetDefaultPresets();
                }
                if (string.IsNullOrEmpty(currentPresetName) && presets.Count > 0) {
                    currentPresetName = presets[0].Name;
                }
                if (legacyPlainKey) {
                    // Migrate an older plaintext api_key to the DPAPI-encrypted form.
                    Save(apiBase, apiKey, model, useVision == "true", floatX, floatY, currentPresetName, presets);
                    Logger.Info("Migrated plaintext api_key to api_key_dpapi");
                }
                return true;
            } catch (Exception ex) {
                Logger.Error("OmniDictConfig.Load", ex);
                return false;
            }
        }

        public static void Save(string apiBase, string apiKey, string model, bool useVision,
                                double floatX, double floatY, string currentPresetName, List<PromptPreset> presets) {
            try {
                Directory.CreateDirectory(AppDataDir);
                var sb = new StringBuilder();
                sb.AppendLine("# OmniDict AI configuration");
                sb.AppendLine("# Generated by OmniDict AI - do not edit while app is running");
                sb.AppendLine();
                sb.AppendLine("api_base       = \"" + Esc(apiBase) + "\"");
                sb.AppendLine("api_key_dpapi  = \"" + ProtectKey(apiKey) + "\"");
                sb.AppendLine("model          = \"" + Esc(model) + "\"");
                sb.AppendLine("use_vision     = " + (useVision ? "true" : "false"));
                sb.AppendLine("current_preset = \"" + Esc(currentPresetName) + "\"");
                if (floatX >= 0 && floatY >= 0) {
                    sb.AppendLine("float_x        = " + ((int)floatX));
                    sb.AppendLine("float_y        = " + ((int)floatY));
                }
                sb.AppendLine();
                if (presets != null) {
                    foreach (var p in presets) {
                        sb.AppendLine("[[presets]]");
                        sb.AppendLine("name    = \"" + Esc(p.Name) + "\"");
                        sb.AppendLine("content = \"" + Esc(p.Content) + "\"");
                        sb.AppendLine();
                    }
                }
                File.WriteAllText(ConfigPath, sb.ToString(), Encoding.UTF8);
                Logger.Info("Config saved to " + ConfigPath);
            } catch (Exception ex) {
                Logger.Error("OmniDictConfig.Save", ex);
            }
        }

        public static void SavePosition(double x, double y) {
            try {
                if (!File.Exists(ConfigPath)) return;
                var list = new List<string>(File.ReadAllLines(ConfigPath, Encoding.UTF8));
                list.RemoveAll(l => l.Trim().StartsWith("float_x") || l.Trim().StartsWith("float_y"));
                list.Add("float_x        = " + ((int)x));
                list.Add("float_y        = " + ((int)y));
                File.WriteAllLines(ConfigPath, list.ToArray(), Encoding.UTF8);
            } catch {}
        }
    }


    public class ToggleSwitch : UserControl {
        private Border track;
        private System.Windows.Shapes.Ellipse thumb;
        private TextBlock statusText;
        private bool _isChecked;

        public event Action<bool> CheckedChanged;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked != value) {
                    _isChecked = value;
                    UpdateVisual();
                    if (CheckedChanged != null) CheckedChanged(_isChecked);
                }
            }
        }

        public ToggleSwitch(bool initial = false) {
            _isChecked = initial;
            this.Cursor = Cursors.Hand;
            var sp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            statusText = new TextBlock {
                Text = _isChecked ? "启用" : "关闭",
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")
            };
            sp.Children.Add(statusText);

            track = new Border {
                Width = 40, Height = 20,
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center
            };

            var canvas = new Canvas { Width = 40, Height = 20 };
            thumb = new System.Windows.Shapes.Ellipse { Width = 12, Height = 12 };
            Canvas.SetTop(thumb, 3);
            canvas.Children.Add(thumb);
            track.Child = canvas;

            sp.Children.Add(track);
            this.Content = sp;

            this.MouseLeftButtonDown += (s, e) => {
                IsChecked = !IsChecked;
            };

            UpdateVisual();
        }

        public void UpdateVisual() {
            bool isDark = Win11Theme.IsDarkTheme;
            if (statusText != null) {
                statusText.Text = _isChecked ? "启用" : "关闭";
                statusText.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(220, 220, 220) : Color.FromRgb(40, 40, 40));
            }
            if (track != null && thumb != null) {
                if (_isChecked) {
                    track.Background = new SolidColorBrush(Color.FromRgb(227, 85, 54));
                    track.BorderBrush = new SolidColorBrush(Color.FromRgb(227, 85, 54));
                    thumb.Fill = Brushes.White;
                    Canvas.SetLeft(thumb, 22);
                } else {
                    track.Background = new SolidColorBrush(isDark ? Color.FromRgb(45, 45, 45) : Color.FromRgb(230, 230, 230));
                    track.BorderBrush = new SolidColorBrush(isDark ? Color.FromRgb(100, 100, 100) : Color.FromRgb(160, 160, 160));
                    thumb.Fill = new SolidColorBrush(isDark ? Color.FromRgb(200, 200, 200) : Color.FromRgb(90, 90, 90));
                    Canvas.SetLeft(thumb, 4);
                }
            }
        }
    }

    public enum ToastType {
        Success,
        Info,
        Warning,
        Error
    }

    public class MainWindow : Window {
        private const int  HOTKEY_ID_ALT_Q  = 9002;
        private const int  HOTKEY_ID_ALT_W  = 9003;
        private const uint MOD_ALT=0x0001,MOD_NOREPEAT=0x4000;
        private const uint VK_Q=0x51,VK_W=0x57;
        private const int  WM_HOTKEY=0x0312;
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd,int id,uint fsModifiers,uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd,int id);
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")] private static extern uint GetClipboardSequenceNumber();
        [StructLayout(LayoutKind.Sequential)] public struct POINT{public int X,Y;}

        private TextBlock  statusText;
        private ListBox    historyList;
        private List<HistoryEntry> historyItems=new List<HistoryEntry>();
        private Border     rootBorder;
        private IntPtr     windowHandle;
        private System.Windows.Forms.NotifyIcon trayIcon;
        public FloatingResultWindow floatingWin;

        public string currentModel="gemini-3.8-flash-high";
        public string apiKey="";
        public string apiBase="https://ai.kncloud.top/v1/chat/completions";
        public bool   useVision=true;
        public string currentPresetName="游戏本地化与攻略私教";
        public List<PromptPreset> promptPresets=new List<PromptPreset>();

        // Navigation elements (Win11 Twinkle Tray style)
        private Border navItem1, navItem2, navItem3, navItem4;
        private TextBlock navIcon1, navIcon2, navIcon3, navIcon4;
        private TextBlock navText1, navText2, navText3, navText4;
        private Border ind1, ind2, ind3, ind4;
        private Grid panel1, panel2, panel3, panel4;
        private TextBlock viewHeaderTitle;
        private Border sidebarBorder;
        private TextBlock brandTitleText;
        private Button winMinBtn;
        private Button winCloseBtn;

        // Toast Notification elements
        private Border toastBorder;
        private TextBlock toastIcon;
        private TextBlock toastText;
        private DispatcherTimer toastTimer;
        private TranslateTransform toastTranslate;
        private ToastType currentToastType = ToastType.Success;

        // Settings inputs
        private TextBox setApiBaseBox, setApiKeyBox, setModelBox;
        private TextBox setPresetNameText;
        private TextBox setPromptBox;
        private ToggleSwitch setVisionToggle;
        private ListBox setModelListBox;
        private System.Windows.Controls.Primitives.Popup setModelPopup;
        private ListBox setPresetListBox;
        private System.Windows.Controls.Primitives.Popup setPresetPopup;

        public MainWindow() {
            try {
                byte[] _ib = Convert.FromBase64String(EmbeddedIcon.IcoB64);
                var _ms = new System.IO.MemoryStream(_ib);
                var _dec = new System.Windows.Media.Imaging.IconBitmapDecoder(_ms, System.Windows.Media.Imaging.BitmapCreateOptions.None, System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
                BitmapFrame _best = null;
                foreach (var fr in _dec.Frames) {
                    if (fr.PixelWidth == 32 || fr.PixelWidth == 48 || fr.PixelWidth == 24) { _best = fr; break; }
                }
                if (_best == null && _dec.Frames.Count > 0) _best = _dec.Frames[_dec.Frames.Count - 1];
                if (_best != null) { _best.Freeze(); this.Icon = _best; }
            } catch(Exception _ex) { Logger.Error("WindowIcon", _ex); }
            LoadOmniConfig(); Logger.Info("Config model="+currentModel+" preset="+currentPresetName);
            InitUI(); InitTray();
            floatingWin=new FloatingResultWindow();
            string _b,_k,_m,_v,_pn; double _fx,_fy; List<PromptPreset> _ps;
            if(OmniDictConfig.Load(out _b,out _k,out _m,out _v,out _fx,out _fy,out _pn,out _ps)){
                if(_fx>=0&&_fy>=0){floatingWin.LastX=_fx;floatingWin.LastY=_fy;floatingWin.HasCustomPosition=true;}
            }
            this.Loaded+=MainWindow_Loaded; this.Hide(); this.Closing+=MainWindow_Closing;
            Win11Theme.ThemeChanged += () => this.Dispatcher.Invoke(ApplyTheme);
            
            var saved=HistoryStore.Load();
            for(int _i=saved.Count-1;_i>=0;_i--){var he=saved[_i];historyItems.Insert(0,he);RebuildHistoryItem(he);}
            if(historyItems.Count>0)this.Dispatcher.BeginInvoke(new System.Action(()=>{
                if(statusText!=null)statusText.Text="历史记录: "+historyItems.Count+" 条";
            }));
        }

        public string GetActiveSystemPrompt() {
            if (promptPresets != null) {
                foreach (var p in promptPresets) {
                    if (p.Name == currentPresetName) return p.Content;
                }
                if (promptPresets.Count > 0) return promptPresets[0].Content;
            }
            return "你是一位全能屏幕智能助手。请精准识别解析截图内容并给出专业中文回答。";
        }

        public void LoadOmniConfig() {
            string sb2,sk2,sm2,sv2,spn; double fx,fy; List<PromptPreset> ps;
            if(OmniDictConfig.Load(out sb2,out sk2,out sm2,out sv2,out fx,out fy,out spn,out ps)){
                if(!string.IsNullOrEmpty(sb2))apiBase=sb2;
                if(!string.IsNullOrEmpty(sk2))apiKey=sk2;
                if(!string.IsNullOrEmpty(sm2))currentModel=sm2;
                if(sv2!=null)useVision=sv2=="true";
                if(!string.IsNullOrEmpty(spn))currentPresetName=spn;
                if(ps!=null&&ps.Count>0)promptPresets=ps;
                else promptPresets=OmniDictConfig.GetDefaultPresets();
                if(floatingWin!=null&&fx>=0&&fy>=0){floatingWin.LastX=fx;floatingWin.LastY=fy;floatingWin.HasCustomPosition=true;}
                Logger.Info("Loaded from "+OmniDictConfig.ConfigPath);
                return;
            }
            promptPresets = OmniDictConfig.GetDefaultPresets();
            try {
                string cfgPath=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".codex","config.toml");
                if(!File.Exists(cfgPath)) return;
                foreach(string line in File.ReadAllLines(cfgPath)) {
                    string t=line.Trim(); int q1,q2;
                    if(t.StartsWith("model ")||t.StartsWith("model=")) { q1=t.IndexOf('"'); q2=t.LastIndexOf('"'); if(q1>0&&q2>q1) currentModel=t.Substring(q1+1,q2-q1-1); }
                    if(t.StartsWith("experimental_bearer_token")) { q1=t.IndexOf('"'); q2=t.LastIndexOf('"'); if(q1>0&&q2>q1) apiKey=t.Substring(q1+1,q2-q1-1); }
                    if(t.StartsWith("base_url")) { q1=t.IndexOf('"'); q2=t.LastIndexOf('"'); if(q1>0&&q2>q1) { string b=t.Substring(q1+1,q2-q1-1).TrimEnd('/'); if(!b.EndsWith("/v1")) b+="/v1"; apiBase=b+"/chat/completions"; } }
                }
                Logger.Info("Loaded from ~/.codex/config.toml");
            } catch(Exception ex) { Logger.Error("LoadOmniConfig",ex); }
        }

        private void InitUI() {
            this.Title="OmniDict AI";
            this.Width=820;
            this.Height=580;
            this.MinWidth=720;
            this.MinHeight=500;
            this.WindowStartupLocation=WindowStartupLocation.CenterScreen;
            this.WindowStyle=WindowStyle.None;
            this.AllowsTransparency=true;
            this.Background=Brushes.Transparent;
            this.Topmost=false;

            rootBorder=new Border {
                CornerRadius=new CornerRadius(12),
                BorderThickness=new Thickness(1),
                Margin=new Thickness(10) };
            rootBorder.Effect=new DropShadowEffect{BlurRadius=24,Color=Colors.Black,Opacity=0.45,ShadowDepth=4,Direction=270};

            Grid mainLayout = new Grid();
            mainLayout.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(210)}); // Left Sidebar
            mainLayout.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1, GridUnitType.Star)}); // Right Content

            // === LEFT NAVIGATION SIDEBAR (Twinkle Tray / Win11 Settings) ===
            sidebarBorder = new Border{
                CornerRadius=new CornerRadius(11,0,0,11),
                BorderThickness=new Thickness(0,0,1,0),
                Padding=new Thickness(10,14,10,14)};
            sidebarBorder.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) try { this.DragMove(); } catch {} };

            Grid sidebarGrid = new Grid();
            sidebarGrid.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto}); // App Title
            sidebarGrid.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1, GridUnitType.Star)}); // Nav items
            sidebarGrid.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto}); // Footer

            // App Brand Header
            DockPanel brandHdr = new DockPanel{LastChildFill=false, Margin=new Thickness(6,0,0,20)};
            System.Windows.Controls.Image brandIcon = new System.Windows.Controls.Image{
                Width=24, Height=24, Margin=new Thickness(0,0,10,0), VerticalAlignment=VerticalAlignment.Center,
                Source=this.Icon};
            RenderOptions.SetBitmapScalingMode(brandIcon, BitmapScalingMode.HighQuality);
            brandHdr.Children.Add(brandIcon);

            brandTitleText = new TextBlock{
                Text="OmniDict", FontWeight=FontWeights.SemiBold, FontSize=15,
                VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Segoe UI Variable Display, Segoe UI, Microsoft YaHei")};
            brandHdr.Children.Add(brandTitleText);
            Grid.SetRow(brandHdr, 0); sidebarGrid.Children.Add(brandHdr);

            // Nav Items Stack
            StackPanel navStack = new StackPanel();
            navItem1 = CreateNavItem("", "解析历史", out navIcon1, out navText1, out ind1);
            navItem2 = CreateNavItem("", "通用设置", out navIcon2, out navText2, out ind2);
            navItem3 = CreateNavItem("", "提示词预设", out navIcon3, out navText3, out ind3);
            navItem4 = CreateNavItem("", "快捷键与操作", out navIcon4, out navText4, out ind4);

            navItem1.MouseLeftButtonDown += (s, e) => SwitchNav(1);
            navItem2.MouseLeftButtonDown += (s, e) => SwitchNav(2);
            navItem3.MouseLeftButtonDown += (s, e) => SwitchNav(3);
            navItem4.MouseLeftButtonDown += (s, e) => SwitchNav(4);

            navStack.Children.Add(navItem1);
            navStack.Children.Add(navItem2);
            navStack.Children.Add(navItem3);
            navStack.Children.Add(navItem4);
            Grid.SetRow(navStack, 1); sidebarGrid.Children.Add(navStack);

            // Sidebar Footer Action
            StackPanel sideFooter = new StackPanel();
            Button sideSnipBtn = new Button{
                Content="截图解析 (Alt+Q)",
                Height=34, Margin=new Thickness(0,0,0,4)};
            sideSnipBtn.Style = Win11Theme.CreateButtonStyle(true);
            sideSnipBtn.Click += (s, e) => TriggerSnipAndAnalyze();
            sideFooter.Children.Add(sideSnipBtn);

            Grid.SetRow(sideFooter, 2); sidebarGrid.Children.Add(sideFooter);
            sidebarBorder.Child = sidebarGrid;
            Grid.SetColumn(sidebarBorder, 0); mainLayout.Children.Add(sidebarBorder);

            // === RIGHT CONTENT PANELS ===
            Grid rightPanel = new Grid{Margin=new Thickness(24,14,24,16)};
            rightPanel.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto}); // Window Caption & Controls
            rightPanel.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1, GridUnitType.Star)}); // Active View Area

            // Right Header (Caption + Controls)
            DockPanel rightHeader = new DockPanel{LastChildFill=false, Margin=new Thickness(0,0,0,16)};
            rightHeader.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) try { this.DragMove(); } catch {} };

            viewHeaderTitle = new TextBlock{
                Text="解析历史",
                FontSize=20,
                FontWeight=FontWeights.SemiBold,
                VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Segoe UI Variable Display, Segoe UI, Microsoft YaHei")};
            DockPanel.SetDock(viewHeaderTitle, Dock.Left); rightHeader.Children.Add(viewHeaderTitle);

            // Window Caption Buttons
            StackPanel winControls = new StackPanel{Orientation=Orientation.Horizontal};
            winMinBtn = new Button{
                Content="—", Width=34, Height=28, Background=Brushes.Transparent,
                BorderThickness=new Thickness(0), Cursor=Cursors.Hand, FontSize=11};
            winMinBtn.Click += (s, e) => this.WindowState = WindowState.Minimized;
            winControls.Children.Add(winMinBtn);

            winCloseBtn = new Button{
                Content="✕", Width=34, Height=28, Background=Brushes.Transparent,
                BorderThickness=new Thickness(0), Cursor=Cursors.Hand, FontSize=12};
            winCloseBtn.Click += (s, e) => this.Hide();
            winControls.Children.Add(winCloseBtn);

            DockPanel.SetDock(winControls, Dock.Right); rightHeader.Children.Add(winControls);
            Grid.SetRow(rightHeader, 0); rightPanel.Children.Add(rightHeader);

            // Panel 1: 历史记录
            panel1 = BuildHistoryPanel();
            Grid.SetRow(panel1, 1); rightPanel.Children.Add(panel1);

            // Panel 2: 常规设置 (Win11 SettingsCards + ToggleSwitch)
            panel2 = BuildGeneralSettingsPanel();
            Grid.SetRow(panel2, 1); rightPanel.Children.Add(panel2);

            // Panel 3: 提示词预设
            panel3 = BuildPromptPresetsPanel();
            Grid.SetRow(panel3, 1); rightPanel.Children.Add(panel3);

            // Panel 4: 快捷键速览
            panel4 = BuildHotkeysPanel();
            Grid.SetRow(panel4, 1); rightPanel.Children.Add(panel4);

            InitToast();
            Grid.SetRow(toastBorder, 0);
            Grid.SetRowSpan(toastBorder, 2);
            rightPanel.Children.Add(toastBorder);

            Grid.SetColumn(rightPanel, 1); mainLayout.Children.Add(rightPanel);
            rootBorder.Child = mainLayout;
            this.Content = rootBorder;

            this.KeyDown += (s, e) => { if (e.Key == Key.Escape) this.Hide(); };

            SwitchNav(1);
            ApplyTheme();
        }

        private Border CreateNavItem(string icon, string title, out TextBlock ic, out TextBlock tb, out Border indicator) {
            Border item = new Border{
                CornerRadius=new CornerRadius(6),
                Padding=new Thickness(10,8,10,8),
                Margin=new Thickness(0,2,0,2),
                Cursor=Cursors.Hand};

            Grid ig = new Grid();
            ig.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
            ig.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
            ig.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1, GridUnitType.Star)});

            Border ind = new Border{
                Width=3, Height=16, CornerRadius=new CornerRadius(1.5),
                Background=new SolidColorBrush(Color.FromRgb(227,85,54)),
                Margin=new Thickness(-6,0,8,0),
                Visibility=Visibility.Collapsed};
            indicator = ind;
            Grid.SetColumn(ind, 0); ig.Children.Add(ind);

            ic = new TextBlock{
                Text=icon, FontSize=14, Width=22, Margin=new Thickness(0,0,10,0),
                VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol")};
            Grid.SetColumn(ic, 1); ig.Children.Add(ic);

            tb = new TextBlock{
                Text=title, FontSize=13, FontWeight=FontWeights.Normal,
                VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")};
            Grid.SetColumn(tb, 2); ig.Children.Add(tb);

            item.MouseEnter += (s, e) => {
                if (ind.Visibility != Visibility.Visible) {
                    bool dark = Win11Theme.IsDarkTheme;
                    item.Background = new SolidColorBrush(dark ? Color.FromArgb(25, 255, 255, 255) : Color.FromArgb(15, 0, 0, 0));
                }
            };
            item.MouseLeave += (s, e) => {
                if (ind.Visibility != Visibility.Visible) {
                    item.Background = Brushes.Transparent;
                }
            };

            item.Child = ig;
            return item;
        }

        public void SwitchNav(int index) {
            bool isDark = Win11Theme.IsDarkTheme;
            panel1.Visibility = index == 1 ? Visibility.Visible : Visibility.Collapsed;
            panel2.Visibility = index == 2 ? Visibility.Visible : Visibility.Collapsed;
            panel3.Visibility = index == 3 ? Visibility.Visible : Visibility.Collapsed;
            panel4.Visibility = index == 4 ? Visibility.Visible : Visibility.Collapsed;

            if (index == 1) viewHeaderTitle.Text = "解析历史";
            else if (index == 2) viewHeaderTitle.Text = "通用设置";
            else if (index == 3) viewHeaderTitle.Text = "提示词预设管理";
            else if (index == 4) viewHeaderTitle.Text = "快捷键与操作指南";

            UpdateNavItemState(navItem1, navIcon1, navText1, ind1, index == 1, isDark);
            UpdateNavItemState(navItem2, navIcon2, navText2, ind2, index == 2, isDark);
            UpdateNavItemState(navItem3, navIcon3, navText3, ind3, index == 3, isDark);
            UpdateNavItemState(navItem4, navIcon4, navText4, ind4, index == 4, isDark);
        }

        private void UpdateNavItemState(Border item, TextBlock icon, TextBlock text, Border ind, bool active, bool isDark) {
            ind.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
            SolidColorBrush activeBrush = new SolidColorBrush(isDark ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0));
            SolidColorBrush inactiveBrush = new SolidColorBrush(isDark ? Color.FromRgb(210, 210, 210) : Color.FromRgb(50, 50, 50));

            if (active) {
                item.Background = new SolidColorBrush(isDark ? Color.FromArgb(45, 255, 255, 255) : Color.FromArgb(30, 0, 0, 0));
                text.FontWeight = FontWeights.SemiBold;
                text.Foreground = activeBrush;
                if (icon != null) icon.Foreground = activeBrush;
            } else {
                item.Background = Brushes.Transparent;
                text.FontWeight = FontWeights.Normal;
                text.Foreground = inactiveBrush;
                if (icon != null) icon.Foreground = inactiveBrush;
            }
        }

        private Grid BuildHistoryPanel() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
            g.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1, GridUnitType.Star)});
            g.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});

            DockPanel tb = new DockPanel{LastChildFill=false, Margin=new Thickness(0,0,0,10)};
            Button clearBtn = new Button{Content="清空记录", Height=30, Padding=new Thickness(14,0,14,0)};
            clearBtn.Style = Win11Theme.CreateButtonStyle(false);
            clearBtn.Click += (s, e) => {
                if (historyItems.Count == 0) {
                    ShowToast("暂无历史记录可清空", ToastType.Info);
                    return;
                }
                historyItems.Clear(); historyList.Items.Clear();
                statusText.Text = "历史已清空";
                HistoryStore.Save(historyItems);
                ShowToast("历史记录已清空", ToastType.Success);
            };
            DockPanel.SetDock(clearBtn, Dock.Left); tb.Children.Add(clearBtn);

            TextBlock tip = new TextBlock{
                Text="提示：点击卡片可查看完整释义与大图",
                FontSize=11.5, Foreground=new SolidColorBrush(Win11Theme.FgTertiary),
                VerticalAlignment=VerticalAlignment.Center};
            DockPanel.SetDock(tip, Dock.Right); tb.Children.Add(tip);
            Grid.SetRow(tb, 0); g.Children.Add(tb);

            historyList = new ListBox{
                Background=Brushes.Transparent, BorderThickness=new Thickness(0), Padding=new Thickness(0)};
            ScrollViewer.SetHorizontalScrollBarVisibility(historyList, ScrollBarVisibility.Disabled);
            historyList.SelectionChanged += (s, e) => {
                if (historyList.SelectedIndex < 0) return;
                int ri = historyItems.Count - 1 - historyList.SelectedIndex;
                if (ri < 0 || ri >= historyItems.Count) return;
                new HistoryDetailWindow(historyItems[ri]){Owner=this}.ShowDialog();
                historyList.SelectedIndex = -1;
            };

            ScrollViewer sv = new ScrollViewer{
                VerticalScrollBarVisibility=ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled,
                Content=historyList};

            Border container = new Border{
                CornerRadius=new CornerRadius(8),
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(1), Padding=new Thickness(4)};
            container.Child = sv;
            Grid.SetRow(container, 1); g.Children.Add(container);

            DockPanel foot = new DockPanel{LastChildFill=false, Margin=new Thickness(0,8,0,0)};
            statusText = new TextBlock{Text="历史记录: "+historyItems.Count+" 条", FontSize=11.5, Foreground=new SolidColorBrush(Win11Theme.FgTertiary)};
            DockPanel.SetDock(statusText, Dock.Left); foot.Children.Add(statusText);
            Grid.SetRow(foot, 2); g.Children.Add(foot);

            return g;
        }

        private Grid BuildGeneralSettingsPanel() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1, GridUnitType.Star)});
            g.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});

            ScrollViewer sv = new ScrollViewer{
                VerticalScrollBarVisibility=ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};

            StackPanel sp = new StackPanel();

            // Card 1: 图像模式 (Twinkle Tray 风格 ToggleSwitch)
            StackPanel cardVision = new StackPanel();
            setVisionToggle = new ToggleSwitch(useVision);
            cardVision.Children.Add(CreateSettingsCardRow(
                "原生 Vision 多模态图像识别",
                "直接上传高保真截图供视觉模型解析（推荐）。关闭后走 Windows 本地 OCR 离线提取文字",
                setVisionToggle));
            sp.Children.Add(WrapCard(cardVision));

            // Card 2: 接口设置
            StackPanel cardApi = new StackPanel();
            setApiBaseBox = CreateModernInput(apiBase, 260);
            cardApi.Children.Add(CreateSettingsCardRow("API 接口地址", "OpenAI 兼容的端点 (例如 /v1/chat/completions)", setApiBaseBox));
            cardApi.Children.Add(CreateDivider());

            setApiKeyBox = CreateModernInput(apiKey, 260);
            cardApi.Children.Add(CreateSettingsCardRow("API 密钥 (Bearer Token)", "用于请求鉴权的 API Key", setApiKeyBox));
            cardApi.Children.Add(CreateDivider());

            // Model Row with Pop-out Dropdown
            DockPanel modelRow = new DockPanel{LastChildFill=true};
            Button fetchBtn = new Button{Content="获取列表", Width=72, Height=32, Cursor=Cursors.Hand, Margin=new Thickness(8,0,0,0)};
            fetchBtn.Style = Win11Theme.CreateButtonStyle(false);
            DockPanel.SetDock(fetchBtn, Dock.Right); modelRow.Children.Add(fetchBtn);

            Grid cmbGrid = new Grid{Height=32, Width=180};
            cmbGrid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1, GridUnitType.Star)});
            cmbGrid.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});

            setModelBox = CreateModernInput(currentModel, 0);
            setModelBox.BorderThickness = new Thickness(1,1,0,1);
            setModelBox.Height = 32;
            Grid.SetColumn(setModelBox, 0); cmbGrid.Children.Add(setModelBox);

            Button arrowBtn = new Button{
                Content="▾", Width=26, Height=32,
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                Foreground=new SolidColorBrush(Win11Theme.FgSecondary),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(0,1,1,1), Cursor=Cursors.Hand};
            Grid.SetColumn(arrowBtn, 1); cmbGrid.Children.Add(arrowBtn);
            modelRow.Children.Add(cmbGrid);

            cardApi.Children.Add(CreateSettingsCardRow("当前推理模型", "可直接输入名称或从接口列表拉取选择", modelRow));
            sp.Children.Add(WrapCard(cardApi));

            // Setup Model Popup
            setModelListBox = new ListBox{
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                Foreground=new SolidColorBrush(Win11Theme.FgPrimary),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderStrong),
                BorderThickness=new Thickness(1), MaxHeight=200};
            var lbItemStyle = new Style(typeof(ListBoxItem));
            lbItemStyle.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Win11Theme.BgSurface)));
            lbItemStyle.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Win11Theme.FgPrimary)));
            lbItemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(10, 6, 10, 6)));
            var lbHover = new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true };
            lbHover.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Win11Theme.BgHover)));
            lbItemStyle.Triggers.Add(lbHover);
            var lbSel = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            lbSel.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromArgb(50, 0, 103, 192))));
            lbItemStyle.Triggers.Add(lbSel);
            setModelListBox.ItemContainerStyle = lbItemStyle;

            setModelPopup = new System.Windows.Controls.Primitives.Popup{
                PlacementTarget=cmbGrid, Placement=System.Windows.Controls.Primitives.PlacementMode.Bottom,
                StaysOpen=false, Child=setModelListBox};

            arrowBtn.Click += (s, e) => {
                if (setModelListBox.Items.Count > 0) {
                    setModelPopup.Width = cmbGrid.ActualWidth + arrowBtn.ActualWidth;
                    setModelPopup.IsOpen = !setModelPopup.IsOpen;
                } else {
                    fetchBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            };
            setModelListBox.SelectionChanged += (s, e) => {
                if (setModelListBox.SelectedItem != null) {
                    setModelBox.Text = setModelListBox.SelectedItem.ToString();
                    setModelPopup.IsOpen = false;
                }
            };

            fetchBtn.Click += (s, e) => {
                fetchBtn.IsEnabled = false; fetchBtn.Content = "...";
                string ep = setApiBaseBox.Text.Trim(); string key = setApiKeyBox.Text.Trim();
                if (ep.EndsWith("/chat/completions")) ep = ep.Substring(0, ep.Length - "/chat/completions".Length);
                if (!ep.EndsWith("/models")) ep = ep.TrimEnd('/') + "/models";
                Task.Run(() => {
                    var ids = new List<string>();
                    try {
                        var req = (HttpWebRequest)WebRequest.Create(ep);
                        req.Method = "GET"; req.Headers["Authorization"] = "Bearer " + key;
                        req.UserAgent = "Codex/1.0"; req.Timeout = 10000;
                        using (var resp = (HttpWebResponse)req.GetResponse())
                        using (var sr2 = new System.IO.StreamReader(resp.GetResponseStream())) {
                            string json = sr2.ReadToEnd();
                            int pos = 0;
                            while (true) {
                                int idx2 = json.IndexOf("\"id\"", pos);
                                if (idx2 < 0) break;
                                int q1 = json.IndexOf('"', idx2 + 4);
                                if (q1 < 0) break;
                                int q2 = json.IndexOf('"', q1 + 1);
                                if (q2 < 0) break;
                                string id = json.Substring(q1 + 1, q2 - q1 - 1);
                                if (id.Length > 0 && !id.Contains("/") && id.IndexOf("codex", StringComparison.OrdinalIgnoreCase) < 0) ids.Add(id);
                                pos = q2 + 1;
                            }
                            ids.Sort();
                        }
                    } catch (Exception ex) { Logger.Error("FetchModels", ex); }
                    this.Dispatcher.Invoke(() => {
                        fetchBtn.Content = "获取列表"; fetchBtn.IsEnabled = true;
                        if (ids.Count > 0) {
                            string cur = setModelBox.Text;
                            setModelListBox.Items.Clear();
                            foreach (var id in ids) setModelListBox.Items.Add(id);
                            setModelBox.Text = cur;
                            setModelPopup.Width = cmbGrid.ActualWidth + arrowBtn.ActualWidth;
                            setModelPopup.IsOpen = true;
                            ShowToast(string.Format("成功拉取 {0} 个可用模型", ids.Count), ToastType.Success);
                        } else {
                            ShowToast("未能获取到模型列表，请检查配置与网络", ToastType.Warning);
                        }
                    });
                });
            };

            sv.Content = sp;
            Grid.SetRow(sv, 0); g.Children.Add(sv);

            // Bottom Save bar
            DockPanel foot = new DockPanel{LastChildFill=false, Margin=new Thickness(0,10,0,0)};
            Button saveBtn = new Button{Content="保存设置", Width=96, Height=34};
            saveBtn.Style = Win11Theme.CreateButtonStyle(true);
            saveBtn.Click += (s, e) => SaveAllSettings();
            DockPanel.SetDock(saveBtn, Dock.Right); foot.Children.Add(saveBtn);
            Grid.SetRow(foot, 1); g.Children.Add(foot);

            return g;
        }

        private Grid BuildPromptPresetsPanel() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1, GridUnitType.Star)});
            g.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});

            ScrollViewer sv = new ScrollViewer{
                VerticalScrollBarVisibility=ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};

            StackPanel sp = new StackPanel();
            StackPanel card = new StackPanel();

            // Preset Selection Row
            DockPanel topRow = new DockPanel{LastChildFill=true, Margin=new Thickness(0,0,0,12)};
            StackPanel btnBar = new StackPanel{Orientation=Orientation.Horizontal};

            Button addBtn = new Button{Content="新建", Height=30, Padding=new Thickness(10,0,10,0), Margin=new Thickness(6,0,0,0)};
            addBtn.Style = Win11Theme.CreateButtonStyle(false);
            addBtn.Click += (s, e) => {
                string baseName = "自定义预设"; int c = 1; string n = baseName;
                while (promptPresets.Exists(x => x.Name == n)) { c++; n = baseName + c; }
                promptPresets.Add(new PromptPreset(n, "请精准拆解并翻译截图内容。"));
                currentPresetName = n;
                SyncPresetUi();
                ShowToast("已新建预设: " + n, ToastType.Success);
            };
            btnBar.Children.Add(addBtn);

            Button renameBtn = new Button{Content="重命名", Height=30, Padding=new Thickness(10,0,10,0), Margin=new Thickness(6,0,0,0)};
            renameBtn.Style = Win11Theme.CreateButtonStyle(false);
            renameBtn.Click += (s, e) => {
                var cur = promptPresets.Find(x => x.Name == currentPresetName);
                if (cur == null) return;
                var win = new Window{
                    Title="重命名预设", Width=360, Height=160,
                    WindowStartupLocation=WindowStartupLocation.CenterOwner, Owner=this,
                    Background=new SolidColorBrush(Win11Theme.BgWindow), ResizeMode=ResizeMode.NoResize};
                Win11Theme.ApplyToWindow(win);

                var wg = new Grid{Margin=new Thickness(16)};
                wg.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
                wg.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
                var inBox = CreateModernInput(cur.Name, 0); inBox.Height=30; inBox.SelectAll();
                Grid.SetRow(inBox, 0); wg.Children.Add(inBox);

                var wbtns = new StackPanel{Orientation=Orientation.Horizontal, HorizontalAlignment=HorizontalAlignment.Right, Margin=new Thickness(0,14,0,0)};
                var okBtn = new Button{Content="确定", Width=64, Height=28, Margin=new Thickness(0,0,8,0)};
                okBtn.Style = Win11Theme.CreateButtonStyle(true);
                okBtn.Click += (s2, e2) => {
                    string val = inBox.Text.Trim();
                    if (!string.IsNullOrEmpty(val)) {
                        cur.Name = val; currentPresetName = val; SyncPresetUi();
                        ShowToast("预设已重命名为: " + val, ToastType.Success);
                    }
                    win.Close();
                };
                wbtns.Children.Add(okBtn);
                var cBtn = new Button{Content="取消", Width=64, Height=28};
                cBtn.Style = Win11Theme.CreateButtonStyle(false);
                cBtn.Click += (s2, e2) => win.Close();
                wbtns.Children.Add(cBtn);

                Grid.SetRow(wbtns, 1); wg.Children.Add(wbtns);
                win.Content = wg; win.ShowDialog();
            };
            btnBar.Children.Add(renameBtn);

            Button delBtn = new Button{Content="删除", Height=30, Padding=new Thickness(10,0,10,0), Margin=new Thickness(6,0,0,0)};
            delBtn.Style = Win11Theme.CreateButtonStyle(false);
            delBtn.Click += (s, e) => {
                if (promptPresets.Count <= 1) { ShowToast("至少保留一个预设！", ToastType.Warning); return; }
                promptPresets.RemoveAll(x => x.Name == currentPresetName);
                currentPresetName = promptPresets[0].Name;
                SyncPresetUi();
                ShowToast("预设已删除", ToastType.Info);
            };
            btnBar.Children.Add(delBtn);

            Button defBtn = new Button{Content="恢复默认", Height=30, Padding=new Thickness(10,0,10,0), Margin=new Thickness(6,0,0,0)};
            defBtn.Style = Win11Theme.CreateButtonStyle(false);
            defBtn.Click += (s, e) => {
                if (MessageBox.Show("确认恢复所有内置默认预设？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    promptPresets = OmniDictConfig.GetDefaultPresets();
                    currentPresetName = promptPresets[0].Name;
                    SyncPresetUi();
                    ShowToast("已恢复内置默认预设", ToastType.Success);
                }
            };
            btnBar.Children.Add(defBtn);
            DockPanel.SetDock(btnBar, Dock.Right); topRow.Children.Add(btnBar);

            // Left Preset Dropdown Pill
            Grid pCmbGrid = new Grid{Height=32};
            pCmbGrid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1, GridUnitType.Star)});
            pCmbGrid.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});

            setPresetNameText = CreateModernInput(currentPresetName, 0);
            setPresetNameText.IsReadOnly = true; setPresetNameText.Cursor = Cursors.Hand;
            setPresetNameText.BorderThickness = new Thickness(1,1,0,1); setPresetNameText.Height = 32;
            Grid.SetColumn(setPresetNameText, 0); pCmbGrid.Children.Add(setPresetNameText);

            Button pArrowBtn = new Button{
                Content="▾", Width=26, Height=32,
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                Foreground=new SolidColorBrush(Win11Theme.FgSecondary),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(0,1,1,1), Cursor=Cursors.Hand};
            Grid.SetColumn(pArrowBtn, 1); pCmbGrid.Children.Add(pArrowBtn);
            topRow.Children.Add(pCmbGrid);
            card.Children.Add(topRow);

            setPresetListBox = new ListBox{
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                Foreground=new SolidColorBrush(Win11Theme.FgPrimary),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderStrong),
                BorderThickness=new Thickness(1), MaxHeight=200};
            var pItemStyle = new Style(typeof(ListBoxItem));
            pItemStyle.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Win11Theme.BgSurface)));
            pItemStyle.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Win11Theme.FgPrimary)));
            pItemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(10, 6, 10, 6)));
            var pHover = new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true };
            pHover.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Win11Theme.BgHover)));
            pItemStyle.Triggers.Add(pHover);
            var pSel = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            pSel.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromArgb(50, 0, 103, 192))));
            pItemStyle.Triggers.Add(pSel);
            setPresetListBox.ItemContainerStyle = pItemStyle;

            setPresetPopup = new System.Windows.Controls.Primitives.Popup{
                PlacementTarget=pCmbGrid, Placement=System.Windows.Controls.Primitives.PlacementMode.Bottom,
                StaysOpen=false, Child=setPresetListBox};

            Action togglePP = () => {
                if (setPresetListBox.Items.Count > 0) {
                    setPresetPopup.Width = pCmbGrid.ActualWidth + pArrowBtn.ActualWidth;
                    setPresetPopup.IsOpen = !setPresetPopup.IsOpen;
                }
            };
            pArrowBtn.Click += (s, e) => togglePP();
            setPresetNameText.PreviewMouseLeftButtonDown += (s, e) => { togglePP(); e.Handled = true; };

            setPresetListBox.SelectionChanged += (s, e) => {
                if (setPresetListBox.SelectedItem != null) {
                    currentPresetName = setPresetListBox.SelectedItem.ToString();
                    setPresetNameText.Text = currentPresetName;
                    setPresetPopup.IsOpen = false;
                    var p = promptPresets.Find(x => x.Name == currentPresetName);
                    if (p != null) setPromptBox.Text = p.Content;
                }
            };

            TextBlock editHint = new TextBlock{
                Text="系统提示词设定 (System Prompt 模板)：",
                FontSize=12, Foreground=new SolidColorBrush(Win11Theme.FgSecondary),
                Margin=new Thickness(0,6,0,6)};
            card.Children.Add(editHint);

            setPromptBox = new TextBox{
                Height=210, AcceptsReturn=true, TextWrapping=TextWrapping.Wrap,
                VerticalScrollBarVisibility=ScrollBarVisibility.Auto,
                Padding=new Thickness(10), FontSize=12.5,
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                Foreground=new SolidColorBrush(Win11Theme.FgPrimary),
                CaretBrush=new SolidColorBrush(Win11Theme.FgPrimary),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(1),
                FontFamily=new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")};
            setPromptBox.TextChanged += (s, e) => {
                var cur = promptPresets.Find(x => x.Name == currentPresetName);
                if (cur != null) cur.Content = setPromptBox.Text;
            };
            card.Children.Add(setPromptBox);

            sp.Children.Add(WrapCard(card));
            sv.Content = sp;
            Grid.SetRow(sv, 0); g.Children.Add(sv);

            DockPanel foot = new DockPanel{LastChildFill=false, Margin=new Thickness(0,10,0,0)};
            Button saveBtn = new Button{Content="保存更改", Width=96, Height=34};
            saveBtn.Style = Win11Theme.CreateButtonStyle(true);
            saveBtn.Click += (s, e) => SaveAllSettings();
            DockPanel.SetDock(saveBtn, Dock.Right); foot.Children.Add(saveBtn);
            Grid.SetRow(foot, 1); g.Children.Add(foot);

            SyncPresetUi();
            return g;
        }

        private Grid BuildHotkeysPanel() {
            Grid g = new Grid();
            ScrollViewer sv = new ScrollViewer{VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
            StackPanel sp = new StackPanel();

            StackPanel card = new StackPanel();
            TextBlock hk1 = new TextBlock{
                Text="Alt + Q", FontWeight=FontWeights.SemiBold,
                Foreground=new SolidColorBrush(Color.FromRgb(227,85,54)), VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Consolas, Segoe UI")};
            card.Children.Add(CreateSettingsCardRow("触发截图与解析", "在游戏、全屏软件、浏览器或桌面任意区域唤醒截屏并分析", hk1));
            card.Children.Add(CreateDivider());

            TextBlock hk2 = new TextBlock{
                Text="Alt + W", FontWeight=FontWeights.SemiBold,
                Foreground=new SolidColorBrush(Color.FromRgb(227,85,54)), VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Consolas, Segoe UI")};
            card.Children.Add(CreateSettingsCardRow("关闭悬浮结果窗", "不抢占游戏控制焦点，随时一键静默隐藏释义浮窗", hk2));
            card.Children.Add(CreateDivider());

            TextBlock hk3 = new TextBlock{
                Text="Esc", FontWeight=FontWeights.SemiBold,
                Foreground=new SolidColorBrush(Win11Theme.FgSecondary), VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Consolas, Segoe UI")};
            card.Children.Add(CreateSettingsCardRow("窗口置顶时关闭", "在 OmniDict 窗口或浮窗处于前台时，按 Esc 即可隐藏", hk3));
            sp.Children.Add(WrapCard(card));

            sv.Content = sp;
            g.Children.Add(sv);
            return g;
        }

        private void SyncPresetUi() {
            if (setPresetListBox == null || setPresetNameText == null || setPromptBox == null) return;
            setPresetListBox.Items.Clear();
            foreach (var p in promptPresets) setPresetListBox.Items.Add(p.Name);
            if (!promptPresets.Exists(x => x.Name == currentPresetName) && promptPresets.Count > 0) {
                currentPresetName = promptPresets[0].Name;
            }
            setPresetNameText.Text = currentPresetName;
            var cur = promptPresets.Find(x => x.Name == currentPresetName);
            if (cur != null) setPromptBox.Text = cur.Content;
        }

        private void InitToast() {
            toastBorder = new Border {
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(14, 6, 16, 6),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 8, 0, 0),
                Visibility = Visibility.Collapsed,
                Opacity = 0,
                Cursor = Cursors.Hand
            };
            Panel.SetZIndex(toastBorder, 999);
            toastBorder.Effect = new DropShadowEffect {
                BlurRadius = 16,
                Color = Colors.Black,
                Opacity = 0.28,
                ShadowDepth = 3,
                Direction = 270
            };

            toastTranslate = new TranslateTransform(0, -14);
            toastBorder.RenderTransform = toastTranslate;

            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            toastIcon = new TextBlock {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            sp.Children.Add(toastIcon);

            toastText = new TextBlock {
                FontSize = 12.5,
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")
            };
            sp.Children.Add(toastText);

            toastBorder.Child = sp;

            toastBorder.MouseLeftButtonDown += (s, e) => {
                DismissToast();
            };
            toastBorder.MouseEnter += (s, e) => {
                if (toastTimer != null) toastTimer.Stop();
            };
            toastBorder.MouseLeave += (s, e) => {
                if (toastBorder != null && toastBorder.Visibility == Visibility.Visible && toastTimer != null) {
                    toastTimer.Start();
                }
            };

            toastTimer = new DispatcherTimer();
            toastTimer.Interval = TimeSpan.FromMilliseconds(2500);
            toastTimer.Tick += (s, e) => {
                toastTimer.Stop();
                DismissToast();
            };
        }

        public void ShowToast(string message, ToastType type = ToastType.Success) {
            if (!this.Dispatcher.CheckAccess()) {
                this.Dispatcher.BeginInvoke(new Action(() => ShowToast(message, type)));
                return;
            }

            if (toastBorder == null) return;
            currentToastType = type;

            if (!this.IsVisible || this.WindowState == WindowState.Minimized) {
                if (trayIcon != null) {
                    var icon = System.Windows.Forms.ToolTipIcon.Info;
                    if (type == ToastType.Warning) icon = System.Windows.Forms.ToolTipIcon.Warning;
                    else if (type == ToastType.Error) icon = System.Windows.Forms.ToolTipIcon.Error;
                    trayIcon.ShowBalloonTip(2000, "OmniDict", message, icon);
                }
                return;
            }

            UpdateToastVisuals(message, type);

            if (toastTimer != null) toastTimer.Stop();
            toastBorder.Visibility = Visibility.Visible;

            toastBorder.BeginAnimation(UIElement.OpacityProperty, null);
            toastTranslate.BeginAnimation(TranslateTransform.YProperty, null);
            toastBorder.Opacity = 0.0;
            toastTranslate.Y = -14.0;

            var fadeIn = new DoubleAnimation {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(180)
            };
            var slideDown = new DoubleAnimation {
                From = -14.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            toastBorder.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            toastTranslate.BeginAnimation(TranslateTransform.YProperty, slideDown);

            if (toastTimer != null) toastTimer.Start();
        }

        private void UpdateToastVisuals(string message, ToastType type) {
            if (toastBorder == null || toastText == null || toastIcon == null) return;
            bool isDark = Win11Theme.IsDarkTheme;
            toastBorder.Background = new SolidColorBrush(isDark ? Color.FromArgb(245, 36, 36, 36) : Color.FromArgb(250, 255, 255, 255));
            toastBorder.BorderBrush = new SolidColorBrush(isDark ? Color.FromArgb(60, 255, 255, 255) : Color.FromArgb(35, 0, 0, 0));
            toastText.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(240, 240, 240) : Color.FromRgb(25, 25, 25));
            toastText.Text = message;

            Color iconColor;
            string iconGlyph;
            switch (type) {
                case ToastType.Warning:
                    iconGlyph = "⚠";
                    iconColor = isDark ? Color.FromRgb(252, 225, 0) : Color.FromRgb(180, 100, 0);
                    break;
                case ToastType.Error:
                    iconGlyph = "✕";
                    iconColor = isDark ? Color.FromRgb(255, 153, 164) : Color.FromRgb(196, 43, 28);
                    break;
                case ToastType.Info:
                    iconGlyph = "ℹ";
                    iconColor = isDark ? Color.FromRgb(96, 205, 255) : Color.FromRgb(0, 103, 192);
                    break;
                case ToastType.Success:
                default:
                    iconGlyph = "✔";
                    iconColor = isDark ? Color.FromRgb(108, 203, 95) : Color.FromRgb(16, 124, 65);
                    break;
            }
            toastIcon.Text = iconGlyph;
            toastIcon.Foreground = new SolidColorBrush(iconColor);
        }

        private void DismissToast() {
            if (toastBorder == null || toastBorder.Visibility != Visibility.Visible) return;
            if (toastTimer != null) toastTimer.Stop();

            var fadeOut = new DoubleAnimation {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(180)
            };
            var slideUp = new DoubleAnimation {
                To = -10.0,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (s, e) => {
                if (toastBorder.Opacity <= 0.05) {
                    toastBorder.Visibility = Visibility.Collapsed;
                }
            };
            toastBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            toastTranslate.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void SaveAllSettings() {
            if (setApiBaseBox != null) apiBase = setApiBaseBox.Text.Trim();
            if (setApiKeyBox != null) apiKey = setApiKeyBox.Text.Trim();
            if (setModelBox != null) currentModel = setModelBox.Text.Trim();
            if (setVisionToggle != null) useVision = setVisionToggle.IsChecked;

            double sx = floatingWin != null && floatingWin.HasCustomPosition ? floatingWin.LastX : -1;
            double sy = floatingWin != null && floatingWin.HasCustomPosition ? floatingWin.LastY : -1;
            OmniDictConfig.Save(apiBase, apiKey, currentModel, useVision, sx, sy, currentPresetName, promptPresets);
            ApplyTheme();
            ShowToast("设置已保存！", ToastType.Success);
        }

        private Border WrapCard(UIElement content) {
            return new Border{
                CornerRadius=new CornerRadius(8),
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(1),
                Padding=new Thickness(16,14,16,14),
                Margin=new Thickness(0,0,0,10),
                Child=content};
        }

        private Grid CreateSettingsCardRow(string title, string desc, UIElement widget) {
            Grid row = new Grid{Margin=new Thickness(0,4,0,4)};
            row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1, GridUnitType.Star)});
            row.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});

            StackPanel textSp = new StackPanel{VerticalAlignment=VerticalAlignment.Center, Margin=new Thickness(0,0,12,0)};
            TextBlock t = new TextBlock{
                Text=title, FontSize=13.5, FontWeight=FontWeights.Normal,
                Foreground=new SolidColorBrush(Win11Theme.FgPrimary),
                FontFamily=new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")};
            textSp.Children.Add(t);

            if (!string.IsNullOrEmpty(desc)) {
                TextBlock d = new TextBlock{
                    Text=desc, FontSize=11.5,
                    Foreground=new SolidColorBrush(Win11Theme.FgTertiary),
                    Margin=new Thickness(0,2,0,0), TextWrapping=TextWrapping.Wrap,
                    FontFamily=new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")};
                textSp.Children.Add(d);
            }

            Grid.SetColumn(textSp, 0); row.Children.Add(textSp);
            if (widget != null) {
                Grid.SetColumn(widget, 1); row.Children.Add(widget);
            }
            return row;
        }

        private Border CreateDivider() {
            return new Border{Height=1, Background=new SolidColorBrush(Win11Theme.BorderSubtle), Margin=new Thickness(0,8,0,8)};
        }

        private TextBox CreateModernInput(string text, double width) {
            var tb = new TextBox{
                Text=text, Height=32, VerticalContentAlignment=VerticalAlignment.Center,
                Padding=new Thickness(10,0,10,0), FontSize=12,
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                Foreground=new SolidColorBrush(Win11Theme.FgPrimary),
                CaretBrush=new SolidColorBrush(Win11Theme.FgPrimary),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(1),
                FontFamily=new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")};
            if (width > 0) tb.Width = width;
            return tb;
        }

        public void ApplyTheme() {
            bool isDark = Win11Theme.IsDarkTheme;
            rootBorder.Background = new SolidColorBrush(Win11Theme.BgWindow);
            rootBorder.BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle);
            if (sidebarBorder != null) {
                sidebarBorder.Background = new SolidColorBrush(isDark ? Color.FromRgb(26,26,26) : Color.FromRgb(238,238,238));
                sidebarBorder.BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle);
            }
            if (brandTitleText != null) {
                brandTitleText.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0));
            }
            viewHeaderTitle.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0));

            if (winMinBtn != null && winCloseBtn != null) {
                SolidColorBrush fgBrush = new SolidColorBrush(isDark ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0));
                winMinBtn.Foreground = fgBrush;
                winCloseBtn.Foreground = fgBrush;
                winCloseBtn.MouseEnter += (s, e) => { winCloseBtn.Background = new SolidColorBrush(Color.FromRgb(196, 43, 28)); winCloseBtn.Foreground = Brushes.White; };
                winCloseBtn.MouseLeave += (s, e) => { winCloseBtn.Background = Brushes.Transparent; winCloseBtn.Foreground = fgBrush; };
                winMinBtn.MouseEnter += (s, e) => { winMinBtn.Background = new SolidColorBrush(isDark ? Color.FromArgb(40, 255, 255, 255) : Color.FromArgb(30, 0, 0, 0)); };
                winMinBtn.MouseLeave += (s, e) => { winMinBtn.Background = Brushes.Transparent; };
            }
            
            if (setVisionToggle != null) setVisionToggle.UpdateVisual();
            if (toastBorder != null && toastBorder.Visibility == Visibility.Visible) {
                UpdateToastVisuals(toastText.Text, currentToastType);
            }
            SwitchNav(panel1.Visibility == Visibility.Visible ? 1 : (panel2.Visibility == Visibility.Visible ? 2 : (panel3.Visibility == Visibility.Visible ? 3 : 4)));
            RefreshAllCards();
        }

        private void RefreshAllCards() {
            historyList.Items.Clear();
            for(int _i=historyItems.Count-1;_i>=0;_i--){
                RebuildHistoryItem(historyItems[_i]);
            }
        }

        private void InitTray() {
            trayIcon=new System.Windows.Forms.NotifyIcon();
            try{byte[] _tb=Convert.FromBase64String(EmbeddedIcon.IcoB64);using(var _tms=new System.IO.MemoryStream(_tb)){trayIcon.Icon=new System.Drawing.Icon(_tms);}}
            catch{trayIcon.Icon=System.Drawing.SystemIcons.Application;}
            trayIcon.Text="OmniDict AI"; trayIcon.Visible=true;
            trayIcon.DoubleClick+=(s,e)=>{this.Show();this.Activate();SwitchNav(1);};
            var menu=new System.Windows.Forms.ContextMenuStrip();
            var m1=new System.Windows.Forms.ToolStripMenuItem("历史记录");
            m1.Click+=(s,e)=>{this.Show();this.Activate();SwitchNav(1);};
            var m2=new System.Windows.Forms.ToolStripMenuItem("截图解析 (Alt+Q)");
            m2.Click+=(s,e)=>TriggerSnipAndAnalyze();
            var m3=new System.Windows.Forms.ToolStripMenuItem("设置");
            m3.Click+=(s,e)=>{this.Show();this.Activate();SwitchNav(2);};
            var m4=new System.Windows.Forms.ToolStripMenuItem("退出");
            m4.Click+=(s,e)=>{isRealExit=true;this.Close();System.Windows.Application.Current.Shutdown();};
            menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[]{m1,m2,m3,new System.Windows.Forms.ToolStripSeparator(),m4});
            trayIcon.ContextMenuStrip=menu;
        }

        private void MainWindow_Loaded(object sender,RoutedEventArgs e) {
            this.Hide();
            windowHandle=new System.Windows.Interop.WindowInteropHelper(this).Handle;
            System.Windows.Interop.HwndSource.FromHwnd(windowHandle).AddHook(HwndHook);
            bool ok2=RegisterHotKey(windowHandle,HOTKEY_ID_ALT_Q,MOD_ALT|MOD_NOREPEAT,VK_Q);
            if(!ok2) ok2=RegisterHotKey(windowHandle,HOTKEY_ID_ALT_Q,MOD_ALT,VK_Q);
            Logger.Info("Alt+Q:"+(ok2?"OK":"FAIL"));
            bool ok3=RegisterHotKey(windowHandle,HOTKEY_ID_ALT_W,MOD_ALT|MOD_NOREPEAT,VK_W);
            if(!ok3) ok3=RegisterHotKey(windowHandle,HOTKEY_ID_ALT_W,MOD_ALT,VK_W);
            Logger.Info("Alt+W:"+(ok3?"OK":"FAIL"));
        }

        private bool isRealExit=false;
        private void MainWindow_Closing(object sender,System.ComponentModel.CancelEventArgs e) {
            if(!isRealExit){e.Cancel=true;this.Hide();return;}
            if(trayIcon!=null){trayIcon.Visible=false;trayIcon.Dispose();}
            UnregisterHotKey(windowHandle,HOTKEY_ID_ALT_Q);
            UnregisterHotKey(windowHandle,HOTKEY_ID_ALT_W);
            HistoryStore.Save(historyItems);
            Logger.Info("History saved, count="+historyItems.Count);
        }

        private IntPtr HwndHook(IntPtr hwnd,int msg,IntPtr wParam,IntPtr lParam,ref bool handled) {
            if(msg==WM_HOTKEY){int id=wParam.ToInt32();
                if(id==HOTKEY_ID_ALT_Q){
                    Logger.Info("Hotkey id="+id);
                    TriggerSnipAndAnalyze();
                    handled=true;
                } else if(id==HOTKEY_ID_ALT_W){
                    Logger.Info("Hotkey close float id="+id);
                    if(floatingWin!=null && floatingWin.IsVisible){
                        floatingWin.Hide();
                    }
                    handled=true;
                }
            }
            return IntPtr.Zero;
        }

        public void TriggerSnipAndAnalyze() {
            uint seqBefore=GetClipboardSequenceNumber();
            Logger.Info("Snip seqBefore="+seqBefore);
            try{System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("ms-screenclip:"){UseShellExecute=true,WindowStyle=System.Diagnostics.ProcessWindowStyle.Hidden,CreateNoWindow=true});Logger.Info("ms-screenclip launched");}
            catch(Exception ex){Logger.Error("Launch failed",ex);}
            Task.Run(()=>{
                for(int i=0;i<80;i++){
                    Thread.Sleep(250);
                    uint seqNow=GetClipboardSequenceNumber();
                    if(seqNow!=seqBefore){
                        Logger.Info("Clipboard changed, waiting 350ms");
                        Thread.Sleep(350);
                        byte[] imgBytes=null;bool hasImg=false;
                        this.Dispatcher.Invoke(()=>{
                            if(Clipboard.ContainsImage()){
                                try{var imgSrc=Clipboard.GetImage();var enc=new System.Windows.Media.Imaging.PngBitmapEncoder();
                                    enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(imgSrc));
                                    using(var ms=new MemoryStream()){enc.Save(ms);imgBytes=ms.ToArray();}
                                    hasImg=true;Logger.Info("Captured bytes="+imgBytes.Length);}
                                catch(Exception ex2){Logger.Error("Capture failed",ex2);}
                            }
                        });
                        if(hasImg&&imgBytes!=null){
                            byte[] _ib2=imgBytes;this.Dispatcher.Invoke(()=>{POINT pt;GetCursorPos(out pt);floatingWin.ShowLoading(pt.X,pt.Y,_ib2);});
                            AnalyzeImageForFloating(floatingWin,imgBytes);break;
                        }
                    }
                }
            });
        }

        private void AnalyzeImageForFloating(FloatingResultWindow fw,byte[] imgBytes) {
            bool vis=useVision;
            string sysPmtRaw = GetActiveSystemPrompt();
            string sysPmt = sysPmtRaw.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r\n","\\n").Replace("\n","\\n");
            Task.Run(()=>{
                try{
                    string body;
                    if(vis){
                        string b64=Convert.ToBase64String(imgBytes);
                        string up="请识别并深度解析截图中出现的文字与界面内容：";
                        body="{\"model\":\""+currentModel+"\",\"messages\":["+
                            "{\"role\":\"system\",\"content\":\""+sysPmt+"\"},"+
                            "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\""+up+"\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,"+b64+"\"}}]}"+
                            "]}";
                    } else {
                        string ocrText=OcrHelper.ExtractText(imgBytes);
                        Logger.Info("OCR len="+ocrText.Length);
                        if(string.IsNullOrWhiteSpace(ocrText))ocrText="[OCR未识别到文字]";
                        string esc=ocrText.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\n","\\n").Replace("\r","");
                        string up="截图中提取到的文字内容：\\n"+esc+"\\n\\n请分析并提供精准翻译与内容深度拆解。";
                        body="{\"model\":\""+currentModel+"\",\"messages\":["+
                            "{\"role\":\"system\",\"content\":\""+sysPmt+"\"},"+
                            "{\"role\":\"user\",\"content\":\""+up+"\"}]"+
                            "}";
                    }
                    string result=PostAI(body);
                    Logger.Info("AI chars="+result.Length);
                    byte[] _ib3=imgBytes;string _r3=result;this.Dispatcher.Invoke(()=>{fw.ShowResult(_r3);AddHistory("截图","[屏幕解析]",_r3,_ib3);});
                }catch(Exception ex){
                    Logger.Error("AnalyzeFloating",ex);
                    byte[] retryBytes = imgBytes;
                    this.Dispatcher.Invoke(()=>{
                        string msg = "解析失败: " + ex.Message;
                        if (ex is WebException && ex.Message.Contains("超时")) {
                            msg = "解析失败: 请求远程模型超时，可能是网络波动或服务繁忙。";
                        }
                        fw.ShowError(msg, ()=>{
                            fw.ShowLoading(-1, -1, retryBytes);
                            AnalyzeImageForFloating(fw, retryBytes);
                        });
                    });
                }
            });
        }

        private string PostAI(string jsonBody) {
            var req=(HttpWebRequest)WebRequest.Create(apiBase);
            req.Method="POST";req.ContentType="application/json";
            req.Headers["Authorization"]="Bearer "+apiKey;
            req.UserAgent="Codex/1.0";req.Timeout=90000;
            Logger.Info("PostAI body_len="+jsonBody.Length+" model="+currentModel+" base="+apiBase);
            byte[] data=new UTF8Encoding(false).GetBytes(jsonBody);
            req.ContentLength=data.Length;
            using(Stream st=req.GetRequestStream()) st.Write(data,0,data.Length);
            try{
                using(var resp=(HttpWebResponse)req.GetResponse())
                using(var rdr=new StreamReader(resp.GetResponseStream(),Encoding.UTF8))
                    return ExtractContent(rdr.ReadToEnd());
            }catch(WebException wex){
                string errBody="";
                if(wex.Response!=null){
                    using(var er=new StreamReader(wex.Response.GetResponseStream(),Encoding.UTF8))
                        errBody=er.ReadToEnd();
                }
                Logger.Error("PostAI HTTP error body="+errBody,wex);
                throw;
            }
        }

        private string ExtractContent(string json) {
            try{
                int ci=json.IndexOf("\"content\":");if(ci<0) return json;
                int q1=json.IndexOf('"',ci+10);if(q1<0) return json;
                var sb=new StringBuilder();
                for(int i=q1+1;i<json.Length;i++){
                    char c=json[i];
                    if(c=='\\' && i+1<json.Length){
                        char n=json[i+1];
                        if(n=='n'){sb.Append('\n');i++;}
                        else if(n=='r'){sb.Append('\r');i++;}
                        else if(n=='t'){sb.Append('\t');i++;}
                        else if(n=='"'){sb.Append('"');i++;}
                        else if(n=='\\'){sb.Append('\\');i++;}
                        else if(n=='/'){sb.Append('/');i++;}
                        else if(n=='u' && i+5<json.Length){
                            string hex=json.Substring(i+2,4);
                            int code;
                            if(int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out code)){
                                sb.Append((char)code);
                                i += 5;
                            } else {
                                sb.Append(c);
                            }
                        }
                        else sb.Append(c);
                    }else if(c=='"') break; else sb.Append(c);
                }
                return sb.ToString();
            }catch{return json;}
        }

        private void RebuildHistoryItem(HistoryEntry entry){
            bool hdrExists=false;
            foreach(System.Windows.Controls.ListBoxItem it in historyList.Items){
                var hb=it.Content as Border;
                if(hb!=null){var tb=hb.Child as TextBlock;
                    if(tb!=null&&tb.Foreground is SolidColorBrush){
                        string label=entry.Date==DateTime.Now.ToString("yyyy-MM-dd")?"今天":
                            entry.Date==DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")?"昨天":entry.Date;
                        if(tb.Text==label){hdrExists=true;break;}
                    }
                }
            }
            if(!hdrExists)historyList.Items.Add(MakeDateHeader(entry.Date??""));
            historyList.Items.Add(MakeCard(entry));
        }

        private void AddHistory(string source,string query,string result,byte[] imgBytes=null) {
            string today=DateTime.Now.ToString("yyyy-MM-dd");
            string imgPath=null;
            if(imgBytes!=null&&imgBytes.Length>0)imgPath=HistoryStore.SaveImage(imgBytes);
            var entry=new HistoryEntry{
                Date=today,Time=DateTime.Now.ToString("HH:mm:ss"),Source=source,
                Query=query.Length>40?query.Substring(0,40)+"...":query,
                Result=result,ImagePath=imgPath};
            historyItems.Add(entry);
            bool needHeader=true;
            foreach(var it in historyItems){if(it!=entry&&it.Date==today){needHeader=false;break;}}
            if(needHeader)historyList.Items.Insert(0,MakeDateHeader(today));
            historyList.Items.Insert(needHeader?1:0,MakeCard(entry));
            statusText.Text="历史记录: "+historyItems.Count+" 条";
        }

        private ListBoxItem MakeDateHeader(string date){
            string label=date==DateTime.Now.ToString("yyyy-MM-dd")?"今天":
                date==DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")?"昨天":date;
            Border hb=new Border{
                Padding=new Thickness(8,6,8,4),Margin=new Thickness(0,4,0,2),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(0,0,0,1)};
            hb.Child=new TextBlock{Text=label,FontSize=11,FontWeight=FontWeights.SemiBold,
                Foreground=new SolidColorBrush(Color.FromRgb(227,85,54))};
            return new ListBoxItem{Content=hb,Background=Brushes.Transparent,
                Padding=new Thickness(0),IsEnabled=false,
                HorizontalContentAlignment=HorizontalAlignment.Stretch};
        }

        private ListBoxItem MakeCard(HistoryEntry entry){
            Border card=new Border{CornerRadius=new CornerRadius(6),
                Background=new SolidColorBrush(Win11Theme.BgCard),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(1),Padding=new Thickness(10,8,10,8),Margin=new Thickness(2,2,2,4)};
            Grid cg=new Grid();
            cg.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
            cg.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});
            cg.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
            cg.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
            cg.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
            
            if(!string.IsNullOrEmpty(entry.ImagePath)&&File.Exists(entry.ImagePath)){
                try{
                    var bmp=new System.Windows.Media.Imaging.BitmapImage();
                    bmp.BeginInit();bmp.UriSource=new Uri(entry.ImagePath);
                    bmp.DecodePixelHeight=54;
                    bmp.CacheOption=System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bmp.EndInit();bmp.Freeze();
                    var img=new System.Windows.Controls.Image{Source=bmp,Width=80,Height=54,
                        Stretch=System.Windows.Media.Stretch.UniformToFill,
                        VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,0,10,0)};
                    Border imgBrd=new Border{CornerRadius=new CornerRadius(4),ClipToBounds=true,
                        Width=80,Height=54,Margin=new Thickness(0,0,10,0),VerticalAlignment=VerticalAlignment.Top};
                    imgBrd.Child=img;
                    Grid.SetRow(imgBrd,0);Grid.SetColumn(imgBrd,0);Grid.SetRowSpan(imgBrd,3);
                    cg.Children.Add(imgBrd);
                }catch{}
            }

            StackPanel topRow=new StackPanel{Orientation=Orientation.Horizontal};
            Border srcB=new Border{CornerRadius=new CornerRadius(3),
                Background=new SolidColorBrush(Win11Theme.IsDarkTheme ? Color.FromArgb(40,0,103,192) : Color.FromArgb(25,0,103,192)),
                BorderBrush=new SolidColorBrush(Win11Theme.IsDarkTheme ? Color.FromArgb(80,0,103,192) : Color.FromArgb(60,0,103,192)),
                BorderThickness=new Thickness(1),Padding=new Thickness(5,1,5,1),Margin=new Thickness(0,1,8,0)};
            srcB.Child=new TextBlock{Text=entry.Source,FontSize=10,Foreground=new SolidColorBrush(Win11Theme.IsDarkTheme ? Color.FromRgb(140,180,240) : Color.FromRgb(0,90,180))};
            topRow.Children.Add(srcB);
            topRow.Children.Add(new TextBlock{Text=entry.Query,FontSize=12,FontWeight=FontWeights.SemiBold,
                Foreground=new SolidColorBrush(Win11Theme.FgPrimary),
                TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center});
            Grid.SetRow(topRow,0);Grid.SetColumn(topRow,1);cg.Children.Add(topRow);

            TextBlock timeTb=new TextBlock{Text=entry.Time,FontSize=10,
                Foreground=new SolidColorBrush(Win11Theme.FgTertiary),Margin=new Thickness(0,3,0,2)};
            Grid.SetRow(timeTb,1);Grid.SetColumn(timeTb,1);cg.Children.Add(timeTb);

            string preview=(entry.Result??"").Replace("\n"," ").Replace("\r","");
            if(preview.Length>60)preview=preview.Substring(0,60)+"...";
            TextBlock pTb=new TextBlock{Text=preview,FontSize=11,
                Foreground=new SolidColorBrush(Win11Theme.FgSecondary),
                TextWrapping=TextWrapping.NoWrap,TextTrimming=TextTrimming.CharacterEllipsis};
            Grid.SetRow(pTb,2);Grid.SetColumn(pTb,1);cg.Children.Add(pTb);
            card.Child=cg;
            return new ListBoxItem{Content=card,Background=Brushes.Transparent,
                Padding=new Thickness(0),HorizontalContentAlignment=HorizontalAlignment.Stretch};
        }
    }

    public class HistoryDetailWindow : Window {
        public HistoryDetailWindow(HistoryEntry entry) {
            this.Title="OmniDict - 详情"; this.Width=520; this.Height=520;
            this.WindowStartupLocation=WindowStartupLocation.CenterOwner;
            this.Background=new SolidColorBrush(Win11Theme.BgWindow);
            this.SourceInitialized+=(s,e)=>Win11Theme.ApplyToWindow(this);

            Grid g=new Grid{Margin=new Thickness(16)};
            g.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
            g.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)});
            DockPanel h=new DockPanel{LastChildFill=false,Margin=new Thickness(0,0,0,10)};
            StackPanel hi=new StackPanel{Orientation=Orientation.Horizontal};
            Border sb2=new Border{CornerRadius=new CornerRadius(3),
                Background=new SolidColorBrush(Win11Theme.IsDarkTheme ? Color.FromArgb(40,0,103,192) : Color.FromArgb(25,0,103,192)),
                BorderBrush=new SolidColorBrush(Win11Theme.IsDarkTheme ? Color.FromArgb(80,0,103,192) : Color.FromArgb(60,0,103,192)),
                BorderThickness=new Thickness(1),Padding=new Thickness(6,2,6,2),Margin=new Thickness(0,0,8,0)};
            sb2.Child=new TextBlock{Text=entry.Source,FontSize=11,Foreground=new SolidColorBrush(Win11Theme.IsDarkTheme ? Color.FromRgb(140,180,240) : Color.FromRgb(0,90,180))};
            hi.Children.Add(sb2);
            hi.Children.Add(new TextBlock{Text=entry.Time+"  "+entry.Query,FontSize=13,
                FontWeight=FontWeights.SemiBold,Foreground=new SolidColorBrush(Win11Theme.FgPrimary),
                VerticalAlignment=VerticalAlignment.Center});
            DockPanel.SetDock(hi,Dock.Left); h.Children.Add(hi);
            Grid.SetRow(h,0); g.Children.Add(h);

            if(!string.IsNullOrEmpty(entry.ImagePath)&&File.Exists(entry.ImagePath)){
                try{
                    g.RowDefinitions.Insert(1,new RowDefinition{Height=GridLength.Auto});
                    var bmp=new System.Windows.Media.Imaging.BitmapImage(new Uri(entry.ImagePath));
                    var img=new System.Windows.Controls.Image{Source=bmp,MaxHeight=140,
                        Stretch=System.Windows.Media.Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Left};
                    Border imgBrd=new Border{CornerRadius=new CornerRadius(6),ClipToBounds=true,
                        BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                        BorderThickness=new Thickness(1),Margin=new Thickness(0,0,0,10)};
                    imgBrd.Child=img;
                    Grid.SetRow(imgBrd,1); g.Children.Add(imgBrd);
                    Grid.SetRow(g.Children[1],0);
                }catch{}
            }

            ScrollViewer sv=new ScrollViewer{VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
            sv.Content=new TextBlock{Text=entry.Result,FontSize=13,LineHeight=22,TextWrapping=TextWrapping.Wrap,
                Foreground=new SolidColorBrush(Win11Theme.FgPrimary),
                FontFamily=new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")};
            Border cb3=new Border{CornerRadius=new CornerRadius(8),
                Background=new SolidColorBrush(Win11Theme.BgSurface),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(1),Padding=new Thickness(14)};
            cb3.Child=sv; Grid.SetRow(cb3,g.RowDefinitions.Count-1); g.Children.Add(cb3);
            this.Content=g;
            this.KeyDown+=(s,e)=>{if(e.Key==Key.Escape)this.Close();};
        }
    }

    public class FloatingResultWindow : Window {
        private RichTextBox contentBox;
        private System.Windows.Controls.Image previewImg;
        private Border previewBorder;
        private Border root;
        private Border cc;
        private Button close;
        private Border altWTag;
        private TextBlock altWText;
        public double LastX = -1;
        public double LastY = -1;
        public bool HasCustomPosition = false;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_SHOWNOACTIVATE = 4;

        public FloatingResultWindow() {
            this.Title = "OmniDict Float";
            this.WindowStyle = WindowStyle.None; this.AllowsTransparency = true;
            this.Background = Brushes.Transparent; this.Topmost = true;
            this.ShowInTaskbar = false; this.ResizeMode = ResizeMode.NoResize;
            this.Focusable = false;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.SourceInitialized += (s, e) => {
                var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
                SetWindowLong(handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);
            };

            root = new Border {
                CornerRadius = new CornerRadius(12),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(8) };
            root.Effect = new DropShadowEffect { BlurRadius = 22, Color = Colors.Black, Opacity = 0.35, ShadowDepth = 4, Direction = 270 };
            root.MouseLeftButtonDown += (s, e) => {
                if (e.LeftButton == MouseButtonState.Pressed) {
                    try {
                        this.DragMove();
                        LastX = this.Left; LastY = this.Top; HasCustomPosition = true;
                        OmniDictConfig.SavePosition(LastX, LastY);
                    } catch {}
                }
            };
            Grid g = new Grid { Margin = new Thickness(10, 8, 10, 8) };
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header
            DockPanel h = new DockPanel { LastChildFill = false, Margin = new Thickness(2, 2, 2, 8) };
            System.Windows.Controls.Image floatIcon = new System.Windows.Controls.Image {
                Width = 20, Height = 20, Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center };
            try {
                byte[] _fib = Convert.FromBase64String(EmbeddedIcon.IcoB64);
                var _fms = new System.IO.MemoryStream(_fib);
                var _fdec = new System.Windows.Media.Imaging.IconBitmapDecoder(_fms, System.Windows.Media.Imaging.BitmapCreateOptions.None, System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
                BitmapFrame _fbest = null;
                foreach (var fr in _fdec.Frames) { if (fr.PixelWidth == 32 || fr.PixelWidth == 24) { _fbest = fr; break; } }
                if (_fbest == null && _fdec.Frames.Count > 0) _fbest = _fdec.Frames[_fdec.Frames.Count - 1];
                if (_fbest != null) { _fbest.Freeze(); floatIcon.Source = _fbest; this.Icon = _fbest; }
            } catch {}
            RenderOptions.SetBitmapScalingMode(floatIcon, BitmapScalingMode.HighQuality);

            StackPanel leftControls = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            leftControls.Children.Add(floatIcon);

            // Header compact thumbnail pill (hover to view full image in tooltip)
            previewImg = new System.Windows.Controls.Image {
                Height = 20, MaxWidth = 72, Stretch = System.Windows.Media.Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center };
            previewBorder = new Border {
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1), Padding = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand, Visibility = Visibility.Collapsed };
            previewBorder.Child = previewImg;
            leftControls.Children.Add(previewBorder);

            DockPanel.SetDock(leftControls, Dock.Left); h.Children.Add(leftControls);

            StackPanel rightControls = new StackPanel { Orientation = Orientation.Horizontal };

            altWTag = new Border {
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1), Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center, Cursor = Cursors.Hand };
            altWTag.MouseLeftButtonDown += (s, e) => this.Hide();
            altWText = new TextBlock {
                Text = "Alt+W", FontSize = 10.5, FontWeight = FontWeights.Medium,
                FontFamily = new FontFamily("Consolas, Segoe UI Variable Text, Segoe UI") };
            altWTag.Child = altWText;
            rightControls.Children.Add(altWTag);

            close = new Button {
                Content = "✕", Width = 26, Height = 26,
                BorderThickness = new Thickness(0), Cursor = Cursors.Hand, FontWeight = FontWeights.Bold };
            close.Click += (s, e) => this.Hide();
            rightControls.Children.Add(close);

            DockPanel.SetDock(rightControls, Dock.Right); h.Children.Add(rightControls);
            Grid.SetRow(h, 0); g.Children.Add(h);


            // Content
            contentBox = new RichTextBox {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0), IsReadOnly = true,
                FontSize = 13.5, FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei"),
                Padding = new Thickness(4, 2, 4, 4) };
            ScrollViewer.SetVerticalScrollBarVisibility(contentBox, ScrollBarVisibility.Auto);
            ScrollViewer.SetHorizontalScrollBarVisibility(contentBox, ScrollBarVisibility.Disabled);
            cc = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(8, 6, 8, 6) };
            cc.Child = contentBox; Grid.SetRow(cc, 1); g.Children.Add(cc);

            root.Child = g; this.Content = root;
            this.KeyDown += (s, e) => { if (e.Key == Key.Escape) this.Hide(); };

            Win11Theme.ThemeChanged += () => this.Dispatcher.Invoke(ApplyFloatTheme);
            ApplyFloatTheme();
        }

        private void ApplyFloatTheme() {
            bool isDark = Win11Theme.IsDarkTheme;
            root.Background = new SolidColorBrush(isDark ? Color.FromArgb(180, 16, 16, 20) : Color.FromArgb(210, 250, 250, 252));
            root.BorderBrush = new SolidColorBrush(isDark ? Color.FromArgb(50, 255, 255, 255) : Color.FromArgb(50, 0, 0, 0));
            
            close.Background = new SolidColorBrush(isDark ? Color.FromArgb(40, 255, 255, 255) : Color.FromArgb(30, 0, 0, 0));
            close.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(190, 190, 205) : Color.FromRgb(80, 80, 95));
            close.MouseEnter += (s, e) => close.Background = new SolidColorBrush(Color.FromRgb(196, 43, 28));
            close.MouseLeave += (s, e) => close.Background = new SolidColorBrush(isDark ? Color.FromArgb(40, 255, 255, 255) : Color.FromArgb(30, 0, 0, 0));

            previewBorder.BorderBrush = new SolidColorBrush(isDark ? Color.FromArgb(50, 255, 255, 255) : Color.FromArgb(50, 0, 0, 0));
            contentBox.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(220, 220, 235) : Color.FromRgb(25, 25, 30));

            if (altWTag != null) {
                altWTag.Background = new SolidColorBrush(isDark ? Color.FromArgb(40, 0, 103, 192) : Color.FromArgb(25, 0, 103, 192));
                altWTag.BorderBrush = new SolidColorBrush(isDark ? Color.FromArgb(90, 0, 103, 192) : Color.FromArgb(70, 0, 103, 192));
                altWText.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(140, 180, 240) : Color.FromRgb(0, 90, 180));
            }
        }

        private double GetPreferredHeight() {
            double workHeight = SystemParameters.WorkArea.Height;
            double preferred = workHeight * 0.78;
            if (preferred < 560) preferred = 560;
            if (preferred > 760) preferred = 760;
            return preferred;
        }

        private double lastOriginX = -1;
        private double lastOriginY = -1;

        public void ShowLoading(double cursorX, double cursorY, byte[] imgBytes) {
            this.WindowState = WindowState.Normal;
            if (cursorX >= 0 && cursorY >= 0) {
                lastOriginX = cursorX;
                lastOriginY = cursorY;
            }
            // Compact pill size for loading
            this.ClearValue(Window.WidthProperty);
            this.ClearValue(Window.HeightProperty);
            contentBox.ClearValue(FrameworkElement.HeightProperty);
            contentBox.ClearValue(FrameworkElement.MaxHeightProperty);
            root.Width = 260;
            root.ClearValue(FrameworkElement.HeightProperty);
            root.ClearValue(FrameworkElement.MaxHeightProperty);
            this.SizeToContent = SizeToContent.WidthAndHeight;

            if (HasCustomPosition && LastX >= 0 && LastY >= 0) {
                EnsureWithinScreen(LastX, LastY);
            } else {
                PositionAt(lastOriginX, lastOriginY);
            }
            if (imgBytes != null && imgBytes.Length > 0) {
                try {
                    var bmp = new System.Windows.Media.Imaging.BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = new MemoryStream(imgBytes);
                    bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bmp.EndInit(); bmp.Freeze();
                    previewImg.Source = bmp;
                    // Hover tooltip to preview larger image
                    var tipImg = new System.Windows.Controls.Image { Source = bmp, MaxWidth = 320, MaxHeight = 220, Stretch = System.Windows.Media.Stretch.Uniform };
                    var tipBorder = new Border {
                        Background = new SolidColorBrush(Color.FromArgb(230, 16, 16, 20)),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                        BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(4), Child = tipImg };
                    previewBorder.ToolTip = tipBorder;
                    previewBorder.Visibility = Visibility.Visible;
                } catch { previewBorder.Visibility = Visibility.Collapsed; }
            } else { previewBorder.Visibility = Visibility.Collapsed; }
            SetRichText("⌛ 正在识别中...");
            this.Show();
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (handle != IntPtr.Zero) {
                ShowWindow(handle, SW_SHOWNOACTIVATE);
            }
        }

        public void ShowResult(string text) {
            AdaptSizeToContent(text);
            SetRichText(text);
        }

        private void AdaptSizeToContent(string text) {
            int len = string.IsNullOrEmpty(text) ? 0 : text.Length;
            double targetWidth = len < 80 ? 360 : (len < 250 ? 460 : 540);
            double workHeight = SystemParameters.WorkArea.Height;
            double maxContentH = workHeight * 0.76;
            if (maxContentH < 380) maxContentH = 380;

            this.ClearValue(Window.WidthProperty);
            this.ClearValue(Window.HeightProperty);
            root.Width = targetWidth;
            root.ClearValue(FrameworkElement.HeightProperty);
            root.ClearValue(FrameworkElement.MaxHeightProperty);
            contentBox.ClearValue(FrameworkElement.HeightProperty);
            contentBox.MaxHeight = maxContentH;
            this.SizeToContent = SizeToContent.WidthAndHeight;

            // Re-check bounds with new size
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => {
                if (HasCustomPosition && LastX >= 0 && LastY >= 0) {
                    EnsureWithinScreen(LastX, LastY);
                } else if (lastOriginX >= 0 && lastOriginY >= 0) {
                    EnsureWithinScreen(lastOriginX + 20, lastOriginY + 20);
                }
            }));
        }

        public void ShowError(string errorMsg, Action onRetry) {
            bool isDark = Win11Theme.IsDarkTheme;
            var doc = new System.Windows.Documents.FlowDocument();
            doc.PagePadding = new Thickness(0);

            var p = new System.Windows.Documents.Paragraph { Margin = new Thickness(4, 8, 4, 12), LineHeight = 22 };
            var errRun = new System.Windows.Documents.Run(errorMsg);
            errRun.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(255, 140, 140) : Color.FromRgb(210, 40, 40));
            errRun.FontSize = 13.5;
            p.Inlines.Add(errRun);
            doc.Blocks.Add(p);

            if (onRetry != null) {
                var btnPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(4, 0, 4, 8) };
                var retryBtn = new Button {
                    Content = "重试解析",
                    Width = 96,
                    Height = 32,
                    Cursor = Cursors.Hand };
                retryBtn.Style = Win11Theme.CreateButtonStyle(true);
                retryBtn.Click += (s, e) => {
                    retryBtn.IsEnabled = false;
                    retryBtn.Content = "正在重试...";
                    onRetry();
                };
                btnPara.Inlines.Add(new System.Windows.Documents.InlineUIContainer(retryBtn));
                doc.Blocks.Add(btnPara);
            }

            contentBox.Document = doc;
        }

        private void SetRichText(string raw) {
            bool isDark = Win11Theme.IsDarkTheme;
            var doc = new System.Windows.Documents.FlowDocument();
            doc.PagePadding = new Thickness(0);
            string[] lns = raw.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            var curPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 0, 0, 4), LineHeight = 22 };
            bool firstBlock = true;
            System.Action flushPara = () => {
                if (curPara.Inlines.Count > 0) { doc.Blocks.Add(curPara); firstBlock = false; }
                curPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 0, 0, 4), LineHeight = 22 };
            };
            foreach (string line in lns) {
                string t = line.TrimEnd();
                if (t.TrimStart('-').Replace("-", "").Trim().Length == 0 && t.Length >= 2 && t.Length <= 4) { flushPara(); continue; }
                // Section brackets like 【中文意思】, 【核心重点】, 【游戏大师】
                if (t.StartsWith("【") && t.Contains("】")) {
                    flushPara();
                    Color sectionColor = isDark ? Color.FromRgb(140, 185, 255) : Color.FromRgb(0, 103, 192);
                    AddHeading(doc, t, 14.0, sectionColor, new Thickness(0, firstBlock ? 2 : 12, 0, 4));
                    firstBlock = false; continue;
                }
                if (t.StartsWith("#### ")) {
                    flushPara();
                    Color hc = isDark ? Color.FromRgb(160, 180, 240) : Color.FromRgb(0, 90, 180);
                    AddHeading(doc, t.Substring(5), 12.5, hc, new Thickness(0, firstBlock ? 0 : 6, 0, 2));
                    firstBlock = false; continue;
                }
                if (t.StartsWith("### ")) {
                    flushPara();
                    Color hc = isDark ? Color.FromRgb(140, 165, 255) : Color.FromRgb(0, 80, 195);
                    AddHeading(doc, t.Substring(4), 13.5, hc, new Thickness(0, firstBlock ? 0 : 8, 0, 2));
                    firstBlock = false; continue;
                }
                if (t.StartsWith("## ")) {
                    flushPara();
                    Color hc = isDark ? Color.FromRgb(180, 200, 255) : Color.FromRgb(0, 103, 192);
                    AddHeading(doc, t.Substring(3), 14.5, hc, new Thickness(0, firstBlock ? 0 : 10, 0, 2));
                    firstBlock = false; continue;
                }
                if (t.Length == 0) { flushPara(); continue; }
                if (t.StartsWith("- ") || t.StartsWith("* ")) {
                    string content = t.Substring(2);
                    flushPara();
                    var bp = new System.Windows.Documents.Paragraph { Margin = new Thickness(12, 1, 0, 2), LineHeight = 22 };
                    var bdot = new System.Windows.Documents.Run("• ");
                    bdot.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(99, 140, 255) : Color.FromRgb(0, 103, 192));
                    bp.Inlines.Add(bdot);
                    AddInlineText(bp, content, isDark);
                    doc.Blocks.Add(bp); firstBlock = false;
                    curPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 0, 0, 4), LineHeight = 22 };
                    continue;
                }
                AddInlineText(curPara, t, isDark);
                curPara.Inlines.Add(new System.Windows.Documents.LineBreak());
            }
            if (curPara.Inlines.Count > 0) doc.Blocks.Add(curPara);
            contentBox.Document = doc;
        }

        private void AddHeading(System.Windows.Documents.FlowDocument doc, string text, double fs, Color c, Thickness margin) {
            var p = new System.Windows.Documents.Paragraph { Margin = margin, LineHeight = double.NaN };
            var r = new System.Windows.Documents.Run(text);
            r.FontWeight = FontWeights.Bold; r.FontSize = fs; r.Foreground = new SolidColorBrush(c);
            p.Inlines.Add(r); doc.Blocks.Add(p);
        }

        private void AddInlineText(System.Windows.Documents.Paragraph para, string text, bool isDark) {
            int i = 0;
            while (i < text.Length) {
                int si = text.IndexOf("**", i);
                if (si < 0) { AppendRun(para, text.Substring(i), false, isDark); break; }
                if (si > i) AppendRun(para, text.Substring(i, si - i), false, isDark);
                int ei = text.IndexOf("**", si + 2);
                if (ei < 0) { AppendRun(para, text.Substring(si), false, isDark); break; }
                AppendRun(para, text.Substring(si + 2, ei - si - 2), true, isDark);
                i = ei + 2;
            }
        }

        private void AppendRun(System.Windows.Documents.Paragraph para, string text, bool bold, bool isDark) {
            if (text.Length == 0) return;
            var r = new System.Windows.Documents.Run(text);
            if (bold) {
                r.FontWeight = FontWeights.Bold;
                r.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(245, 248, 255) : Color.FromRgb(10, 10, 20));
            } else {
                r.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(215, 215, 225) : Color.FromRgb(30, 30, 40));
            }
            para.Inlines.Add(r);
        }

        private void PositionAt(double x, double y) {
            double tx = x + 20, ty = y + 20;
            EnsureWithinScreen(tx, ty);
        }

        private void EnsureWithinScreen(double targetX, double targetY) {
            double maxX = SystemParameters.PrimaryScreenWidth - Width;
            double maxY = SystemParameters.PrimaryScreenHeight - Height;
            if (targetX > maxX) targetX = maxX;
            if (targetY > maxY) targetY = maxY;
            if (targetX < 0) targetX = 0;
            if (targetY < 0) targetY = 0;
            this.Left = targetX; this.Top = targetY;
        }
    }

    public static class HistoryStore {
        public  static readonly string AppDataDir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OmniDict");
        public  static readonly string HistoryPath = System.IO.Path.Combine(AppDataDir, "history.dat");
        public  static readonly string ImagesDir = System.IO.Path.Combine(AppDataDir, "images");

        private static readonly string OldAppDataDir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GameDict");
        private static readonly string OldHistoryPath = System.IO.Path.Combine(OldAppDataDir, "history.dat");

        private const string SEP = "---ENTRY---";

        public static string SaveImage(byte[] bytes) {
            try {
                Directory.CreateDirectory(ImagesDir);
                string path = System.IO.Path.Combine(ImagesDir, DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png");
                File.WriteAllBytes(path, bytes); return path;
            } catch (Exception ex) { Logger.Error("SaveImage", ex); return null; }
        }

        public static void Save(List<HistoryEntry> items) {
            try {
                Directory.CreateDirectory(AppDataDir);
                var sb = new StringBuilder();
                foreach (var e in items) {
                    sb.AppendLine(SEP);
                    sb.AppendLine(e.Date ?? "");
                    sb.AppendLine(e.Time ?? "");
                    sb.AppendLine(e.Source ?? "");
                    sb.AppendLine((e.Query ?? "").Replace("\n", "\\n").Replace("\r", ""));
                    sb.AppendLine((e.Result ?? "").Replace("\n", "\\n").Replace("\r", ""));
                    sb.AppendLine(e.ImagePath ?? "");
                }
                File.WriteAllText(HistoryPath, sb.ToString(), Encoding.UTF8);
            } catch (Exception ex) { Logger.Error("HistoryStore.Save", ex); }
        }

        public static List<HistoryEntry> Load() {
            var list = new List<HistoryEntry>();
            string readPath = HistoryPath;
            if (!File.Exists(readPath) && File.Exists(OldHistoryPath)) {
                readPath = OldHistoryPath;
            }

            try {
                if (!File.Exists(readPath)) return list;
                string[] lines = File.ReadAllLines(readPath, Encoding.UTF8);
                int i = 0;
                while (i < lines.Length) {
                    if (lines[i].Trim() == SEP) {
                        bool newFmt = i + 6 < lines.Length && lines[i + 1].Length == 10 && lines[i + 1].Contains("-");
                        if (newFmt && i + 6 < lines.Length) {
                            var entry = new HistoryEntry {
                                Date = lines[i + 1], Time = lines[i + 2], Source = lines[i + 3],
                                Query = lines[i + 4].Replace("\\n", "\n"),
                                Result = lines[i + 5].Replace("\\n", "\n"),
                                ImagePath = lines[i + 6].Trim() };
                            if (!File.Exists(entry.ImagePath)) entry.ImagePath = null;
                            list.Add(entry); i += 7; continue;
                        } else if (i + 4 < lines.Length) {
                            var entry = new HistoryEntry {
                                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                                Time = lines[i + 1], Source = lines[i + 2],
                                Query = lines[i + 3].Replace("\\n", "\n"),
                                Result = lines[i + 4].Replace("\\n", "\n"),
                                ImagePath = null };
                            list.Add(entry); i += 5; continue;
                        }
                    }
                    i++;
                }
            } catch (Exception ex) { Logger.Error("HistoryStore.Load", ex); }
            return list;
        }
    }

    public class HistoryEntry {
        public string Date { get; set; }
        public string Time { get; set; }
        public string Source { get; set; }
        public string Query { get; set; }
        public string Result { get; set; }
        public string ImagePath { get; set; }
    }

    public static class OcrHelper {
        public static string ExtractText(byte[] pngBytes) {
            try {
                string tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "omnidict_ocr_in.png");
                System.IO.File.WriteAllBytes(tmp, pngBytes);
                string ps =
                    "Add-Type -AssemblyName System.Runtime.WindowsRuntime; " +
                    "[void][Windows.Storage.StorageFile,Windows.Storage,ContentType=WindowsRuntime]; " +
                    "[void][Windows.Media.Ocr.OcrEngine,Windows.Foundation,ContentType=WindowsRuntime]; " +
                    "[void][Windows.Globalization.Language,Windows.Foundation,ContentType=WindowsRuntime]; " +
                    "function Aw($t){[System.WindowsRuntimeSystemExtensions]::GetAwaiter($t).GetResult()} " +
                    "$f=Aw([Windows.Storage.StorageFile]::GetFileFromPathAsync('" + tmp.Replace("'", "''") + "')); " +
                    "$s=Aw($f.OpenAsync([Windows.Storage.FileAccessMode]::Read)); " +
                    "$bmp=Aw([Windows.Graphics.Imaging.BitmapDecoder]::CreateAsync($s)); " +
                    "$sb=Aw($bmp.GetSoftwareBitmapAsync()); " +
                    "$eng=[Windows.Media.Ocr.OcrEngine]::TryCreateFromUserProfileLanguages(); " +
                    "if(-not $eng -and [Windows.Media.Ocr.OcrEngine]::AvailableRecognizerLanguages.Count -gt 0){$eng=[Windows.Media.Ocr.OcrEngine]::TryCreateFromLanguage([Windows.Media.Ocr.OcrEngine]::AvailableRecognizerLanguages[0])} " +
                    "$r=Aw($eng.RecognizeAsync($sb)); $r.Text";
                var psi = new System.Diagnostics.ProcessStartInfo("powershell.exe", "-NoProfile -NonInteractive -Command \"" + ps + "\"") {
                    RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using (var proc = System.Diagnostics.Process.Start(psi)) {
                    string text = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    return text.Trim();
                }
            } catch (Exception ex) { Logger.Error("OcrHelper", ex); return ""; }
        }
    }

    public static class Logger {
        private static readonly string LogPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OmniDict", "omnidict.log");
        private static readonly object _lock = new object();
        static Logger() { try { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(LogPath)); } catch {} }
        public static void Info(string m) { Write("INFO ", m); }
        public static void Warn(string m) { Write("WARN ", m); }
        public static void Error(string m, Exception ex = null) {
            Write("ERROR", ex == null ? m : m + " | " + ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace); }
        private static void Write(string lv, string m) {
            try {
                lock (_lock) {
                    using (var fs = new FileStream(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var sw = new StreamWriter(fs, Encoding.UTF8))
                        sw.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + lv + "] " + m + "\n");
                }
            } catch {}
        }
    }

internal static class EmbeddedIcon {
        public static readonly string IcoB64 = "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/wD///8A/+f/AOAABwDAAAMAwAADAMAAAwDAAAMAwAADAMAAAwDAAAMAwAADAMAAAwDAAAMAwAADAOAYBwDwPA8A+P8fAP///wD///8A////AP///wAoAAAAIAAAAEAAAAABACAAAAAAAIAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACqVQAD/wAAAf8AAAGqVQADAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/AAABqlUAAwAAAAAAAAIAAAACAAAAAACqVQAD//8AAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/wAAAb9/AAT/fwAC/38AAv9/AAL/fwAC/38AAv8AAAL/AAAC/wAAAQAAAAAAAAAA0GUhU9ZZDcTYWgzGzmAfWgAAAQAAAAAA/wAAAf8AAAL/AAAC/wAAAv9/AAL/fwAC/38AAv9/AAK/PwAE/wAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAAEAAAABAAAAAQAAAAEAAAABAAAAAQAAAAIA2nlIFcxXGH3LTQj/vEEE/7o/A//NTQf/z1gXhdR0ShgAAAIAAAABAAAAAQAAAAEAAAABAAAAAQAAAAEAAAABAAAAAAAAAAAAAAAAAAAAAAD/AAAB/wAAAdR/fwbkcyB+52oSuNthD7nVWw260VcMus1TDLrKUAy6xk4MusFLDMe7RQntu0EG/5YtA/6sPAX8rTsF/JUsA/28QQb/vEcK78RMDMfESwq6yE8MuspQDLrMUAy6zVINutFVDLncXw253WgVgZ9/Xwj/AAAB/wAAAf9/AAQAAAAA638ngv93Dv/mXwT/008C/8REA/++QAP/uT4D/7Y8BP+xOgT/rDgE/6EzBP+RKQP8sEEH/81RBf/OUgf/skMJ/48oA/ygMwP/qjcE/7A5BP+0OwT/tzwE/7k9BP+8PwT/w0ME/9NOA//3aAf/32wXiwAAAAC/fwAE/38AAgAAAQDrdxrE62QH/MhOBvnPWw/8yVYN/L5LCfy3RAb8skAG/K08BfymNgP8oDIE/r5NDP/pawv/xkoE/8dMBv/schT/wFAP/58xA/6iMwP8qDkF/K49BvyxPwX8tkMF/L9KBvzFTwj8uEEG+ddQA/zjahHMAAADAP8AAAH/fwACAAABAOt2F8DmYgf/53gd/PaBHf/naw3/1FcD/8RJAf+2PwL/sT4F/7pMDf/bahf/8nkV/+ZnCP/FSQT/xUsF/+huE//0gSL/3G4b/7hLDP+sOwX/rzsD/7dAA//DSAP/1VgE/+lsB//eZgz80E0F/+FoEMgAAAIA/wAAAf9/AAIAAAEA6XYXwetoC//yhSb86nES/9dfDP/UaBj/0mga/9lxIP/qfyP/+Ikj//eAHP/schL/5GcJ/8ZLBf/GTAb/5mwU/+15I//3hSb/+Ikm/+h7Hv/WaBX/yVoN/8lWC//DSwb/1FgD/+ZuDfzVUgb/32QPyQAAAgD/AAAB/38AAgAAAQDpeBrB7WoN//CCJfzncBb/8pc5//2kQf/8mjb//ZQv//qMKP/zgSD/8Hka/+10Ff/kaAv/yEwE/8hNBv/mbBX/7Xgj/+9+Jv/xgCT/9YMi//mEH//6gxn/+4YW/+19Ff/TWQb/4mkM/NdTBv/fZRDJAAADAP8AAAH/fwACAAABAOt4GsHtaw3/738j/Ot8If/4pUf/95c5//aTNP/1jy//9Yko//SEIv/yehv/624M/+RmCP/JTgX/x0wF/+ZuGP/sdyH/73cd//GAJ//xfyX/8Xod//F4Gf/yeBP/94QW/9tkCv/gZwz82FYH/+BpEckAAAMA/wAAAf9/AAIAAAEA7Hodwe5tDv/vgSX8634i//ekRv/4nD7/+JIu//eMJ//2jCz/9Ycn//F3Ff/zrHL/6og//8hIAf/ITAT/53Uk//bIof/xmVT/73UY//B3GP/xeRn/8XkY//J5FP/0fxb/22MK/+BnDfzYVgf/4WoTyQAAAwD/AAAB/38AAgAAAQDtfR3B728P//CEJ/ztgSX/+KdK//iZOP/3rmT/9qxn//aDG//0fBH/8os0//ncxP/qfy7/yksA/8tQCf/nbRT/75dS//fVt//zpmf/86dp//jWuP/0qGf/8nMM//SAGf/cZAr/4GgN/NpYB//jaxTJAAADAP8AAAH/fwACAAABAO19HcHwcBD/8IYo/O2DJ//3qU7/+Jgz//jJm//55dX/9qtk//a8h//417n/9cKX/+doBf/NUgb/zFAH/+p0Hv/tcxf/75dN//vt3//417n/86Je//KHLv/zeRb/9YEZ/9xlC//iahD83VsH/+RtFckAAAMA/wAAAf9/AAIAAAEA74AfwfJzEf/wiCr87oUp//eqT//5nj3/96ZT//nn1v/54cz/9sGP//njzv/yoF3/6WgE/89VCP/OUwn/6nAY/+55HP/30K3/9sym//bQrv/xhy3/8nsX//N9Gv/1hBr/3mYM/+JrEfzdXQj/5G4UyQAAAwD/AAAB/38AAgAAAQDvgiHB9HYT//KNL/zwiiz/+KxQ//mjR//3lzP/99Gp//fYuv/0nUj/993G//GFKv/schH/0lcH/89SBf/shTf/87R///bKo//wgiP/9LmD//bDlP/ydgr/9IEd//eGHP/faA3/420S/OBgCf/mcxjJAAADAP8AAAH/fwACAAABAPGHIsD1eRT/8o8x/PCNL//4r1P/+KdJ//idO//3q13/+erd//jl1P/3xZj/830V/+56Gv/VWwj/0lUG/+6FNP/1xpv/9tKx//fRr//20K7/+ebX//WdTf/1hR7/94kc/+BqD//lcBX84WIJ/+h0GMgAAAMA/wAAAf9/AAIAAAEA84slw/Z8FfzzkTT88ZEy//ezV//4qUv/+KRH//eaNf/31rP/+fr7//WiUv/0hB7/8n4d/9dcB/7VXA3+8H8k//KEJ//zjjH/+ODI//fTr//2wo7/99e4//aUNf/4ihn/4m4S/+Z0GPzkZQr86ncaywAAAgD/AAAB/6oAAwAAAAD0lS6u/4wa//SWN/vylTb/+LVZ//itT//3qEn/+KA+//eqVv/2smr/9pEt//aNKf/ygR7+4GMI/+BlEf/xiC/+9ZA6//aRNf/1olP/9Zg8//aJG//2jCL/944i//iRIP/lcxT/6nkb+/t2D//rgSS3AAABAP9VAAP/fwAC//8AAfGlSyX4lTC49589/POaOvv4uFv/+LFU//isTf/4p0j/+KA9//eYMv/3mDb/9ZEt+/uMIv/pdRnp53wn5/yVNv/0lTz79ppB//eWNf/3ljP/95g1//eUK//3kiX/+JUj/+h5GfvyiCP98YQmu+ybSin//wAB/38AAgAAAAD//wABAAAAAAAABQD2rlDd/7RK//e9X/z4tln/+LBT//isTf/3qEn/9qNE/fadO/v/oDj/+5ky9vKUPzzwoV00+6FG8f+nRv/2nkL79p9C/feePP/4mzX/+Jov//iYKf/2mST9/5gm//SZM+QAAwYAAAAAAP//AAEAAAAAAAAAAAAAAAD/qlUD/38AAvW6ZjT6wGOk+sNl/Pe6XPz3tlj8+LFU+/euTv7/skv//7RK//uoR8nspVU2AAAAAAAAAADur2Yt+a5VxP+zUP//rkj/+KM///egOPv3nzP8950s+/yjKfv7qjyo8KpLNv9/AAL/qlUDAAAAAAAAAAAAAAAAAAAAAAAAAAD//38CAAAAAObmthX/zm7//8xn///IZP//ymH//L5b//m6Wsn5uFxhqv//AwAAAACqVQADqlVVAwAAAAAA//8B+btkW/y3VMT5skj//7lD//+zOf//rzH//7Iz/++3XyAAAAAA/6pVAwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/v38E/6pVA/XPenD60HC/+MdrqfjJbXf6zng1AAACAAAAAAAAAAAA/39/AgAAAAAAAAAA/39/AgAAAAAAAAAAAAABAPTAYjH6vU9097ZDqfu5P8L0tEJ3/79/BP+/PwQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/6pVA/8AAAEAAAAAAAAAAAAAAAAAAAAAAAAAAP+qVQP/AAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/qlUD//9/Av+qVQP/v38E//9/AgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//38C/78/BP+qAAP/fwAC/6pVAwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP////////////////////////////5////8H//wAAAHwAAAA8AAAAPAAAADwAAAA8AAAAPAAAADwAAAA8AAAAPAAAADwAAAA8AAAAPAAAADwAAAA8AAAAPgAAAH8AGAD/gDwB/8D/A//n/+f///////////////////////////KAAAADAAAABgAAAAAQAgAAAAAACAJQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA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4//zkjz/83oP//SEH//1gxr/+Iod/95pD//gZQz/6ncZ/9BXCPztagv/6n4jsgAAAQD/fwACAAAAAAAAAAD/qgADAAAAAO+PLaT/hRn/42sO+/SXOP/0iyz/7osu//m1Wf/4qUz/+KZH//ejRv/4mzn/9qlX//nt4v/46Nf/+Ord//jXuv/0hyL/84Mi/+56G//pbxD/0FYG/8xRB//mbRf/7X8o//GlY//1yJ7/+ODJ//nr3//55NH/997I//rv6P/2xpr/9JU+//WGIP/2hRz/+Y0e/99qD//hZw7/63ka/9FYCPztagr/6nwisgAAAgD/fwACAAAAAAAAAAD/qgADAAAAAPCRLqP/ihr/5W4Q+/WXOf/1ji//7o4w//i3XP/4q07/96hL//ikRf/4okT/95gz//fUrv/68u3/+vby//a3fP/0hRz/84gn//F+Hv/qcBH/0lcG/89UB//pchv/74Ip//GFKP/yiCn/8ooq//bRrv/58On/9s6m//fYu//46t3/+PHq//WdR//3gxT/+ZEh/+BtEP/hahH/63wd/9RbCPzwbg3/7YAksQAAAQD/fwACAAAAAAAAAAD/qgADAAAAAPKXNKX/jBr853ER+vWaPf/1kTH/8JAz//i4Xf/4rlH/96lM//imR//4pEX/+Jw5//auYP/47+T/9tu+//aWOf/1jCn/9Ikm//OBH//scxL/1FkG/tFXCv7seB//8Ycu//SNNv/0kTv/9Y0t//W4e//24cr/9JAu//WOKP/1mT3/9aRS//aQKv/4ihz/+ZMh/+JwEf/iaxL/7X8f/9heCvvzcg787oMptAAAAQD/fwACAAAAAAAAAAD/fz8EAAAAAPGjRXL/mSP/63QR+/WePv/1lDX/8ZM1//m6X//4sFP/+K1Q//epSv/3pkX/+KNF//ifPf/2pEr/9pk5//eTL//2kS//9Ysm//SFIf/veRf/2l0F/9hcC//tfib/84sx//SON//1kjr/9pU8//WTNf/0lTj/9pQy//eTLv/3jiX/9ooc//ePIf/4jiH/+ZQi/+VzE//nchf/8IUh/9thCPv/gxj/7JA2fwAAAAD/fz8EAAAAAAAAAAD//wAB//8AAee5ogv0mziw+owh//agP/71mDn/8pg5//m8X//4s1X/97BS//isTv/4qUn/+KZF//ijQ//4nTr/+Js5//aYN//2kzD/9o8q//SJJP7yfRj94mwT/ONyHvvzhyr98o83/vWUO//1lz7/9pk+//eZPP/3ljX/9pY0//eWMf/3lC7/95Qq//aSJv/3kSL/+pgk/+d5GP/peRz/84wl/e94F//uizK37syZD///AAH//wABAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAMA9rJmPPmtT+77nzz7854+/Pi+Yv/4tVj/+bNW//ivUf/4rE3/96lJ//elRf/4o0P/96A///abOP/3mTb/9ZMx/vWMJvr/kyX/64c1c+2US2X/nTr/9ZQ5+/SYQP72nEH/951C//edQP/4nDz/95o3//eaNf/3mDH/+JYs//eVKP/3lCT/+Zol/+qAHf3wgh/79ZYw9/OnV0MAAQQAAAAAAP8AAAEAAAAAAAAAAAAAAAAAAAAAAAAAAP//AAH/fwACAAAAAPS1XL//uUz/+qdF//fAY//4ulz/+LZZ//iyVf/4r1H/+K5N//iqSf/3pkb/+KNC//ihQP/2nDz8+Zk1//+hNv/xlkGVAAACAAAAAQDvoFWH/6lK//uhRP/2n0T796FD//ihQv/3n0D/+J46//icN//3mzL/+Jsu//iYKv/3mCf/+J0m//aQJP//oS3/9KA7ygAAAQD/AAAC//8AAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//38C/6pVA/HJfyb6u1+t+LdY3fjDZv74vV7++Lpb//i3V//5tFX/+LBS//iuT//2qkr+96hG+/ilQv7/rUT/+qVC8/OqVW8AAAMA/38AAv9VAAMAAAAA8a5mX/qtU+3/sk7/+KVF//ikRvv3o0L++KE+//igOf/4oDX/+Z8x//idLf/3nSn++aEo/viiNd/6qkWt8rRdKf9/AAL/f38CAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA9NqVRv/Nbf74v2H8+L1g//e6XP73uFn9+LVW+/e0Uvz7s0///7pR//uyTv/5slSq8rhyKAAAAAD/fwACAAAAAAAAAAD/qgADAAAAAPDDjiL7uGKg+bBT+/+4T//+rkX/96Y//PelPfv4pjr9+KM0/viiMP/3oSr7/6wt/fOzUVgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAL+/fwT//wAC8c6CJfzOb//9x2T/+L9h/vzAYP//xF7//8pg///IYP/5vV3t+L5invXCczcAAAAAAAAAAP+qVQMAAAAAAAAAAAAAAAAAAAAA/6pVAwAAAAAAAAAA78B8Mfm7YJf5tlDn/7pJ//+5Q///sTr//Ks0//inMP77qCz//7Q0/+y0WDf/fwACzJkzBQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//38CAAABAPLQgn/803Pz9sZn/vfHaPL3x2nV9cdsovnNd1zz3aYXAAAAAAAAAAD/fwAC/39/AgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP9/fwL/qlUDAAAAAAAAAADw1JsS88BjVfi8UZ34uUbU+LQ/8vizOf/7uDv487ZIiv///wH/f38CAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAADs2aAb89CFLPXhnBr///8BAAAAAAAAAAAAAAAA/6pVA///fwIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA//9/Av+qVQMAAAAAAAAAAAAAAAD///8B9NaEGfTIbC/nyHMhAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP//fwIAAAAAAAAAAAAAAAAAAAAA/6pVA///qgP///8BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD///8B/6pVA/+qVQMAAAAAAAAAAAAAAAAAAAAA/38AAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB/f38C//9/An9/fwL/AAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/AAAB////Af//fwL/f38CAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP///////wAA////////AAD///////8AAP///////wAA////////AAD///////8AAP///////wAA////////AAD///w///8AAP//+B///wAA///gB///AAD4AAAAAB8AAPAAAAAADwAA4AAAAAAHAADgAAAAAAcAAOAAAAAABwAA4AAAAAAHAADgAAAAAAcAAOAAAAAABwAA4AAAAAAHAADgAAAAAAcAAOAAAAAABwAA4AAAAAAHAADgAAAAAAcAAOAAAAAABwAA4AAAAAAHAADgAAAAAAcAAOAAAAAABwAA4AAAAAAHAADgAAAAAAcAAOAAAAAABwAA4AAAAAAHAADwAAAAAA8AAPAAAAAADwAA/AABgAA/AAD8AAGAAD8AAP4AB+AAfwAA/4AP8AH/AAD/gD/8Af8AAP/B//+B/wAA////////AAD///////8AAP///////wAA////////AAD///////8AAP///////wAA////////AAD///////8AACgAAABAAAAAgAAAAAEAIAAAAAAAAEIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA//8AAb8/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GP/maw773WIL//OOMv/1jSz/63ca/+qFKv/6sVb/+KNG//mhQ//5nT7/959D//jexP/75dX/+eTQ//bEkv/2x5v/99m9//nk0v/65tT/+t7G//nizf/vlUz/6WsI/+NqD//aXgb/xUoF/8FHBf/aXQv/5WwW/+t1IP/teiX/7oEr/+18If/xr3X/+uLM//ncw//54sz/8qxv//B8HP/xeRf/8XkV//N8G//zexn/8nsY//N8F//5iRz/3WoP/9ZaBv/lbA//6nMW/8pPBv/UVwb78WwM/+yBK5oAAAAA/1UAAwAAAAAAAAAAAAAAAAAAAAD/fz8EAAAAAOuLNYX/hhr/6G0Q+91jCv/zjzP/9Y0t/+x5G//rhSz/+LJV//ikR//5oUP/+KFE//iWM//1voX/+evf//njz//56Nv/++zj//ro2v/32Lz/99e5//rizv/207L/7Xsf/+txEv/jaA3/218H/8ZLBP/DRwX/214K/+ZtGP/sdyL/734q/+98IP/woVz/+ObU//nizf/22r7/+uTQ//bHnf/wgCH/8YEk//F/Iv/zfx3/830Z//N9GP/0fhj/+Yoc/91qD//XWwb/5WwO/+p1GP/JUAb/1FgG+/BsDP/pfSmaAAAAAP9VAAMAAAAAAAAAAAAAAAAAAAAA/38/BAAAAADrjzWF/4ca/+luEPveZAv/85Ez//aPLv/teh3/64ct//m0V//4pUj/+aJE//igQv/4mz3/9p9G//jjzP/65tf/+eXU//XFlP/1pln/85M5//bRr//66t3/87N6/+5xDf/sdBf/5GoN/9xgBv/GTAX/xUoG/91gC//kbRn/63gj/+57I//viDT/99m9//vp3P/3zqn/7ow2//fTsf/67eL/86tr//J8F//zhCX/84Ae//N9Gv/0fRn/9H8Y//mKHP/faw//11wG/+RrD//qdRj/zFIH/9VZBvvxbgz/64IrmgAAAAD/VQADAAAAAAAAAAAAAAAAAAAAAP9/PwQAAAAA6o41hv+IG//pcBH74GYM//SSNf/3kjP/7n4f/+yKLf/5tFj/+KZJ//mkRf/5oUP/96BD//eWMP/2xY7/+Ozh//ns4P/1u4L/9X0M//WmV//559n/+ePR//GTQv/vdxT/63YX/+ZtD//eYgj/x00F/8VLBf/eYg3/5W8Z/+t5I//tfyf/7oQq//fV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AAD//wABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAP+/PwSqqlUDAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//38C/78/BP8AAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/AAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP+qqgP/v38E/79/BP+/fwT/qlUD/39/AgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAf39/Av+qVQP/vz8E/78/BP+/PwT/qlUDAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA///////////////////////////////////////////////////////////////////////////////////////////////////////////////+f/////////gf////////8A/////////AA////////AAAP////AAAAAAAAD/4AAAAAAAAH/gAAAAAAAAf+AAAAAAAAA/wAAAAAAAAD/AAAAAAAAAP8AAAAAAAAA/wAAAAAAAAD/AAAAAAAAAP8AAAAAAAAA/wAAAAAAAAD/AAAAAAAAAP8AAAAAAAAA/wAAAAAAAAD/AAAAAAAAAP8AAAAAAAAA/wAAAAAAAAD/AAAAAAAAAP8AAAAAAAAA/wAAAAAAAAD/AAAAAAAAAP8AAAAAAAAA/wAAAAAAAAD/AAAAAAAAAP8AAAAAAAAA/wAAAAAAAAD/AAAAAAAAAP+AAAAAAAAA/4AAAAAAAAH/gAAAAAAAAf/gAAAAAAAH//AAABgAAA//8AAAPAAAD//4AAB+AAAf//8AAf+AAH///wAD/+AA////AB//+AD///8Af///AP///8/////z////////////////////////////////////////////////////////////////////////////////////////////////////////////+JUE5HDQoaCgAAAA1JSERSAAAAgAAAAIAIBgAAAMM+YcsAAEKXSURBVHic7b0JvF5XdR/633ufc77hTrqaLFmyJE8yCAx4ADuAsUwwvJCEAEGGhNCUklfShDYtJWkfaSoplNI2QwvkpYU0xAEySW0IqQMOZpBs4+BBGMuDLMmyJVnj1Z3v/aYz7P1+a629z3euH7FlI5Pg3C1f3+/e+w1n772G//qvtdcBFsfiWByLY3EsjsWxOBbH4lgci2NxLI7FsTgWx+JYHItjcSyOxbE4FsfiWByLY3EsjsWxOBbH4lgci+M5D+eceuoX/qEOJ/PfunWr3uqcpp/xQh07nDOgSX634Zza4pzZ+g0Xbdmxw7wQhcLRRjunZY7OAH/bHBVoLb5f1xV9Pz5k61anb1KqoMdPPPHEknsPHRrozWR23foVeN367nyk1NxOgP9Og1eGFmwXDDbDbldwgHL4ARtbt27VuzZv07t3wSqlLM0KAH1HAqB3/L7m/af1wOH6yOhQVBsYPrLv6GtvvHFip18rEhqlnt95f9807U8OnvqJMbX0PVOd/DVznXywKJzVSul6TU/UI3UwtfnBhi72X1BLH1mRTz70o5s2neSV8kO0Zid23nRTWMi/l2PrVqd3bYbefYPKw+/I7D2x98jol2ujlx+aKq6ZTrG+MLXL5rtufTvNV6bKJAaItbUzS0ajBzY0Zv74P7x89WeUIhl4foXgeRMAuvBt26C2bd6mb177od855gbfP5UDna5Fnlk4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fs2qA2/Z8Rvv+vZVV30q3rPn/az/L7TxghWAKire+9W/Oe9f3qo/88DJgTfPFgOwNgXxB87fIkD5agzK6y8xXVw8OHHvD2/s/Mx/+tCNB7bs2GF23vR3C3Sfz/GCFgAeW7dqbN9u6VjCu3/19rfee7L+z05PtH4oNaNDOeXz2ZP3ULet7rJGev8Va/Qf/Ml/XPlZpTb2nktY9YM2XvgCQEOOLvMjKjL6yG//77X3H195+WwvXcsnFlQxccWltYf/8y9s3h9YXAaVLzB//w9+kEZXzvM85+e8kMY/mIkuGM6pLTft1GObVvD8Vz5yxu3c9LDD9u0viNBucSyOxbE4FsfiWByLY3EsjsWxOBbH4lgci2NxLI7FsTgWx+JYHItjcSyOxbE4FsfiWByLY3EsjsXxD3X8f68IvlfqCiDuAAAAAElFTkSuQmCCiVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAADLr0lEQVR4nOz9B5hl13UeiP4n3nurqnNAaOQcmAlm0gRFkaKobAnQ2LIte2SJtmXJ47Gfx+PxDIB5lm155GfLsi1LHidZtmRAOVBMIkAxkyBBkASI2Gg0Old35brhxPettPc+t6obkNgkG1RtslDV9564w9pr/etfawFbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22r/alrEb7pWxu1rfx19913d9737rvv5m+iKNIjttqfpta2bWc+3AfE+4DoAaC5G/hTMTe+WQVARIN79wMPxPe87W3V8x18b9sm9PuRBx7g/rj79tvrPw2D/6dpkbPwv/3uGHgAj87f3p7a90D0kRcwN+64t01u2fdAhAceaO65554G32Ttm04A0GK+M4p4AVNLASyttRd/6EO/HS8vL6fx3FzT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";
        public static string ExtractToTemp(){
            try{
                byte[] bytes=Convert.FromBase64String(IcoB64);
                string path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"omnidict_icon.ico");
                File.WriteAllBytes(path,bytes);return path;
            }catch{return null;}
        }
    }
}