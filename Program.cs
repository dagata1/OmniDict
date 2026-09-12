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

                foreach (string line in File.ReadAllLines(loadPath, Encoding.UTF8)) {
                    string t = line.Trim(); int q1, q2;
                    if (t.StartsWith("api_base")) { q1 = t.IndexOf('"'); q2 = t.LastIndexOf('"'); if (q1 >= 0 && q2 > q1) apiBase = t.Substring(q1 + 1, q2 - q1 - 1); }
                    else if (t.StartsWith("api_key")) { q1 = t.IndexOf('"'); q2 = t.LastIndexOf('"'); if (q1 >= 0 && q2 > q1) apiKey = t.Substring(q1 + 1, q2 - q1 - 1); }
                    else if (t.StartsWith("model")) { q1 = t.IndexOf('"'); q2 = t.LastIndexOf('"'); if (q1 >= 0 && q2 > q1) model = t.Substring(q1 + 1, q2 - q1 - 1); }
                    else if (t.StartsWith("use_vision")) { useVision = t.Contains("true") ? "true" : null; }
                    else if (t.StartsWith("current_preset")) { q1 = t.IndexOf('"'); q2 = t.LastIndexOf('"'); if (q1 >= 0 && q2 > q1) currentPresetName = t.Substring(q1 + 1, q2 - q1 - 1); }
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
                        if (t.StartsWith("name")) {
                            q1 = t.IndexOf('"'); q2 = t.LastIndexOf('"');
                            if (q1 >= 0 && q2 > q1) currentPName = t.Substring(q1 + 1, q2 - q1 - 1);
                        } else if (t.StartsWith("content")) {
                            q1 = t.IndexOf('"'); q2 = t.LastIndexOf('"');
                            if (q1 >= 0 && q2 > q1) {
                                string c = t.Substring(q1 + 1, q2 - q1 - 1).Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                                currentPContent.Append(c);
                            }
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
                sb.AppendLine("api_base       = \"" + (apiBase ?? "") + "\"");
                sb.AppendLine("api_key        = \"" + (apiKey ?? "") + "\"");
                sb.AppendLine("model          = \"" + (model ?? "") + "\"");
                sb.AppendLine("use_vision     = " + (useVision ? "true" : "false"));
                sb.AppendLine("current_preset = \"" + (currentPresetName ?? "") + "\"");
                if (floatX >= 0 && floatY >= 0) {
                    sb.AppendLine("float_x        = " + ((int)floatX));
                    sb.AppendLine("float_y        = " + ((int)floatY));
                }
                sb.AppendLine();
                if (presets != null) {
                    foreach (var p in presets) {
                        sb.AppendLine("[[presets]]");
                        sb.AppendLine("name    = \"" + (p.Name ?? "").Replace("\"", "\\\"") + "\"");
                        string esc = (p.Content ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");
                        sb.AppendLine("content = \"" + esc + "\"");
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
        private TextBlock navText1, navText2, navText3, navText4;
        private Border ind1, ind2, ind3, ind4;
        private Grid panel1, panel2, panel3, panel4;
        private TextBlock viewHeaderTitle;
        private Border sidebarBorder;

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
            try{byte[] _ib=Convert.FromBase64String(EmbeddedIcon.IcoB64);var _ms=new System.IO.MemoryStream(_ib);var _dec=new System.Windows.Media.Imaging.IconBitmapDecoder(_ms,System.Windows.Media.Imaging.BitmapCreateOptions.None,System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);if(_dec.Frames.Count>0){var _fr=_dec.Frames[0];_fr.Freeze();this.Icon=_fr;}}catch(Exception _ex){Logger.Error("WindowIcon",_ex);}
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
            Border brandBadge = new Border{
                Width=24, Height=24, CornerRadius=new CornerRadius(6),
                Background=new SolidColorBrush(Color.FromRgb(0,103,192)),
                Margin=new Thickness(0,0,10,0), VerticalAlignment=VerticalAlignment.Center};
            brandBadge.Child = new TextBlock{Text="O", Foreground=Brushes.White, FontWeight=FontWeights.Bold,
                FontSize=12, HorizontalAlignment=HorizontalAlignment.Center, VerticalAlignment=VerticalAlignment.Center};
            brandHdr.Children.Add(brandBadge);

            TextBlock brandTitle = new TextBlock{
                Text="OmniDict", FontWeight=FontWeights.SemiBold, FontSize=15,
                VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Segoe UI Variable Display, Segoe UI, Microsoft YaHei")};
            brandHdr.Children.Add(brandTitle);
            Grid.SetRow(brandHdr, 0); sidebarGrid.Children.Add(brandHdr);

            // Nav Items Stack
            StackPanel navStack = new StackPanel();
            navItem1 = CreateNavItem("⏱", "解析历史", out navText1, out ind1);
            navItem2 = CreateNavItem("⚙", "通用设置", out navText2, out ind2);
            navItem3 = CreateNavItem("📝", "提示词预设", out navText3, out ind3);
            navItem4 = CreateNavItem("⌨", "快捷键与操作", out navText4, out ind4);

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
            Button minBtn = new Button{
                Content="—", Width=32, Height=28, Background=Brushes.Transparent,
                BorderThickness=new Thickness(0), Cursor=Cursors.Hand, FontSize=11};
            minBtn.Click += (s, e) => this.WindowState = WindowState.Minimized;
            winControls.Children.Add(minBtn);

            Button closeBtn = new Button{
                Content="✕", Width=32, Height=28, Background=Brushes.Transparent,
                BorderThickness=new Thickness(0), Cursor=Cursors.Hand, FontSize=12};
            closeBtn.Click += (s, e) => this.Hide();
            winControls.Children.Add(closeBtn);

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

            Grid.SetColumn(rightPanel, 1); mainLayout.Children.Add(rightPanel);
            rootBorder.Child = mainLayout;
            this.Content = rootBorder;

            this.KeyDown += (s, e) => { if (e.Key == Key.Escape) this.Hide(); };

            SwitchNav(1);
            ApplyTheme();
        }

        private Border CreateNavItem(string icon, string title, out TextBlock tb, out Border indicator) {
            Border item = new Border{
                CornerRadius=new CornerRadius(6),
                Padding=new Thickness(10,8,10,8),
                Margin=new Thickness(0,2,0,2),
                Cursor=Cursors.Hand};

            Grid ig = new Grid();
            ig.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
            ig.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
            ig.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1, GridUnitType.Star)});

            indicator = new Border{
                Width=3, Height=16, CornerRadius=new CornerRadius(1.5),
                Background=new SolidColorBrush(Color.FromRgb(227,85,54)),
                Margin=new Thickness(-6,0,8,0),
                Visibility=Visibility.Collapsed};
            Grid.SetColumn(indicator, 0); ig.Children.Add(indicator);

            TextBlock ic = new TextBlock{
                Text=icon, FontSize=14, Width=20, Margin=new Thickness(0,0,10,0),
                VerticalAlignment=VerticalAlignment.Center};
            Grid.SetColumn(ic, 1); ig.Children.Add(ic);

            tb = new TextBlock{
                Text=title, FontSize=13, FontWeight=FontWeights.Normal,
                VerticalAlignment=VerticalAlignment.Center,
                FontFamily=new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei")};
            Grid.SetColumn(tb, 2); ig.Children.Add(tb);

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

            UpdateNavItemState(navItem1, navText1, ind1, index == 1, isDark);
            UpdateNavItemState(navItem2, navText2, ind2, index == 2, isDark);
            UpdateNavItemState(navItem3, navText3, ind3, index == 3, isDark);
            UpdateNavItemState(navItem4, navText4, ind4, index == 4, isDark);
        }

        private void UpdateNavItemState(Border item, TextBlock text, Border ind, bool active, bool isDark) {
            ind.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
            if (active) {
                item.Background = new SolidColorBrush(isDark ? Color.FromArgb(45, 255, 255, 255) : Color.FromArgb(30, 0, 0, 0));
                text.FontWeight = FontWeights.SemiBold;
                text.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(255, 255, 255) : Color.FromRgb(10, 10, 10));
            } else {
                item.Background = Brushes.Transparent;
                text.FontWeight = FontWeights.Normal;
                text.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(185, 185, 195) : Color.FromRgb(80, 80, 90));
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
                historyItems.Clear(); historyList.Items.Clear();
                statusText.Text = "历史已清空";
                HistoryStore.Save(historyItems);
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
                    if (!string.IsNullOrEmpty(val)) { cur.Name = val; currentPresetName = val; SyncPresetUi(); }
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
                if (promptPresets.Count <= 1) { MessageBox.Show("至少保留一个预设！", "提示"); return; }
                promptPresets.RemoveAll(x => x.Name == currentPresetName);
                currentPresetName = promptPresets[0].Name;
                SyncPresetUi();
            };
            btnBar.Children.Add(delBtn);

            Button defBtn = new Button{Content="恢复默认", Height=30, Padding=new Thickness(10,0,10,0), Margin=new Thickness(6,0,0,0)};
            defBtn.Style = Win11Theme.CreateButtonStyle(false);
            defBtn.Click += (s, e) => {
                if (MessageBox.Show("确认恢复所有内置默认预设？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    promptPresets = OmniDictConfig.GetDefaultPresets();
                    currentPresetName = promptPresets[0].Name;
                    SyncPresetUi();
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

        private void SaveAllSettings() {
            if (setApiBaseBox != null) apiBase = setApiBaseBox.Text.Trim();
            if (setApiKeyBox != null) apiKey = setApiKeyBox.Text.Trim();
            if (setModelBox != null) currentModel = setModelBox.Text.Trim();
            if (setVisionToggle != null) useVision = setVisionToggle.IsChecked;

            double sx = floatingWin != null && floatingWin.HasCustomPosition ? floatingWin.LastX : -1;
            double sy = floatingWin != null && floatingWin.HasCustomPosition ? floatingWin.LastY : -1;
            OmniDictConfig.Save(apiBase, apiKey, currentModel, useVision, sx, sy, currentPresetName, promptPresets);
            ApplyTheme();
            MessageBox.Show("设置已保存！", "OmniDict", MessageBoxButton.OK, MessageBoxImage.Information);
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
            viewHeaderTitle.Foreground = new SolidColorBrush(Win11Theme.FgPrimary);
            
            if (setVisionToggle != null) setVisionToggle.UpdateVisual();
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
            Border bg = new Border {
                Width = 20, Height = 20, CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromRgb(0, 103, 192)), VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0) };
            bg.Child = new TextBlock {
                Text = "O", Foreground = Brushes.White, FontSize = 11, FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            StackPanel leftControls = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            leftControls.Children.Add(bg);

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
        public static readonly string IcoB64 = "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";
        public static string ExtractToTemp(){
            try{
                byte[] bytes=Convert.FromBase64String(IcoB64);
                string path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"omnidict_icon.ico");
                File.WriteAllBytes(path,bytes);return path;
            }catch{return null;}
        }
    }
}