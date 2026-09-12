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
        public static readonly string IcoB64 = "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xsb09u3bDYWr7/3orp+891j08xMz2Y2JXjiaEdHDnryPmmnlS+PeQ5tW4dN/cUfvj5R6a/+lhFU/bONHXgB4FD1qFTUKw6/9xp+uundy5ZU57DqlImvyZPKaS+v7f+vf3vxkL50vOH/PVz4YF3KQRgNj+sWfY/9hKMY/GAvwPJHC1q079cSmZTz/5fs2202bYH/UEP5gDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZgDMZg/EMb/z8xcJ0agwgA+QAAAABJRU5ErkJggolQTkcNChoKAAAADUlIRFIAAAEAAAABAAgGAAAAXHKoZgAAy69JREFUeJzs/QeYZdd1Hoj+J957q6pzQGjkHJgJZtIERZGiqGwJ0NiyLXtkibZlyeOxn8fj8QyAeZZteeRny7ItSx4nWbZkQDlQTCJAMZMgQZAEiNhoNDpXd+W64cT3rbT3PreqG5DYJBtUbbJQ1feeuMPaa/3rX2sBW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq221rbbVttpW22pbbattta221bbaVttqW22rbbWtttW22lbbalttq/2paxG+6Vsbta38dffdd3fe9+677+ZvoijSI7ban6bWtm1nPtwHxPuA6AGguRv4UzE3vlkFQESDe/cDD8T3vO1t1fMdfG/bJvT7kQce4P64+/bb6z8Ng/+naZGz8L/97hh4AI/O396e2vdA9JEXMDfuuLdNbtn3QIQHHmjuueeeBt9k7ZtOANBivjOKeAFTSwEsrbUXf+hDvx0vLy+n8dxc08vzFiPgJS/Z3tx8874iim45s+nF2ja66wEkt94uu8GdERqSLV/H19lqf4LWtm18HxA98gCie94WnXWRt23bA7Dr4x//YPzksaPNgZe/5VUH9u664nOf/ewnb9u799TNuzCMrr1t2Y5/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";
        public static string ExtractToTemp(){
            try{
                byte[] bytes=Convert.FromBase64String(IcoB64);
                string path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"omnidict_icon.ico");
                File.WriteAllBytes(path,bytes);return path;
            }catch{return null;}
        }
    }
}