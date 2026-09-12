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
                if(statusText!=null)statusText.Text="历史: "+historyItems.Count+" 条";
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

        private TextBlock titleText;
        private Border listContainer;
        private Button mainSetBtn;
        private Button mainClearBtn;
        private Button mainCloseBtn;

        private void InitUI() {
            this.Title="OmniDict AI"; this.Width=480; this.Height=560;
            this.WindowStartupLocation=WindowStartupLocation.CenterScreen;
            this.WindowStyle=WindowStyle.None; this.AllowsTransparency=true;
            this.Background=Brushes.Transparent; this.Topmost=false;

            rootBorder=new Border { CornerRadius=new CornerRadius(12),
                BorderThickness=new Thickness(1), Margin=new Thickness(10) };
            rootBorder.Effect=new DropShadowEffect{BlurRadius=20,Color=Colors.Black,Opacity=0.35,ShadowDepth=4,Direction=270};
            rootBorder.MouseLeftButtonDown+=(s,e)=>{ if(e.LeftButton==MouseButtonState.Pressed) try{this.DragMove();}catch{} };

            Grid g=new Grid{Margin=new Thickness(16,12,16,12)};
            g.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
            g.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
            g.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)});
            g.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});

            // Header
            DockPanel header=new DockPanel{LastChildFill=false,Margin=new Thickness(0,0,0,12)};
            StackPanel left=new StackPanel{Orientation=Orientation.Horizontal};
            Border badge=new Border{Width=24,Height=24,CornerRadius=new CornerRadius(6),
                Background=new SolidColorBrush(Color.FromRgb(0,103,192)),Margin=new Thickness(0,0,8,0)};
            badge.Child=new TextBlock{Text="O",Foreground=Brushes.White,FontWeight=FontWeights.Bold,
                HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,FontSize=12};
            left.Children.Add(badge);
            titleText=new TextBlock{Text="OmniDict AI",FontWeight=FontWeights.SemiBold,FontSize=14,
                VerticalAlignment=VerticalAlignment.Center};
            left.Children.Add(titleText);
            DockPanel.SetDock(left,Dock.Left); header.Children.Add(left);

            StackPanel right=new StackPanel{Orientation=Orientation.Horizontal};
            mainSetBtn=new Button{Content="设置",Height=26,Padding=new Thickness(10,0,10,0),
                Cursor=Cursors.Hand,Margin=new Thickness(0,0,8,0)};
            mainSetBtn.Click+=(s,e)=>{var d=new SettingsDialog(this);d.Owner=this;d.ShowDialog();};
            right.Children.Add(mainSetBtn);

            mainCloseBtn=new Button{Content="✕",Width=28,Height=26,Background=Brushes.Transparent,
                BorderThickness=new Thickness(0),Cursor=Cursors.Hand,FontWeight=FontWeights.Normal,FontSize=12};
            mainCloseBtn.Click+=(s,e)=>this.Hide();
            right.Children.Add(mainCloseBtn);
            DockPanel.SetDock(right,Dock.Right); header.Children.Add(right);
            Grid.SetRow(header,0); g.Children.Add(header);

            // Toolbar
            DockPanel toolbar=new DockPanel{LastChildFill=false,Margin=new Thickness(0,0,0,10)};
            Button snipBtn=new Button{Content="截图解析 (Alt+Q)",Height=32,Padding=new Thickness(16,0,16,0),
                Cursor=Cursors.Hand,Margin=new Thickness(0,0,8,0)};
            snipBtn.Style=Win11Theme.CreateButtonStyle(true);
            snipBtn.Click+=(s,e)=>TriggerSnipAndAnalyze();
            DockPanel.SetDock(snipBtn,Dock.Left); toolbar.Children.Add(snipBtn);

            mainClearBtn=new Button{Content="清空历史",Height=32,Padding=new Thickness(14,0,14,0),Cursor=Cursors.Hand};
            mainClearBtn.Click+=(s,e)=>{historyItems.Clear();historyList.Items.Clear();statusText.Text="历史已清空";HistoryStore.Save(historyItems);};
            DockPanel.SetDock(mainClearBtn,Dock.Left); toolbar.Children.Add(mainClearBtn);
            Grid.SetRow(toolbar,1); g.Children.Add(toolbar);

            // History ListBox
            historyList=new ListBox{Background=Brushes.Transparent,BorderThickness=new Thickness(0),
                Padding=new Thickness(0)};
            ScrollViewer.SetHorizontalScrollBarVisibility(historyList,ScrollBarVisibility.Disabled);
            historyList.SelectionChanged+=(s,e)=>{
                if(historyList.SelectedIndex<0) return;
                int ri=historyItems.Count-1-historyList.SelectedIndex;
                if(ri<0||ri>=historyItems.Count) return;
                new HistoryDetailWindow(historyItems[ri]){Owner=this}.ShowDialog();
                historyList.SelectedIndex=-1;
            };
            ScrollViewer sv=new ScrollViewer{VerticalScrollBarVisibility=ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
            sv.Content=historyList;
            listContainer=new Border{CornerRadius=new CornerRadius(8),
                BorderThickness=new Thickness(1),Padding=new Thickness(4)};
            listContainer.Child=sv; Grid.SetRow(listContainer,2); g.Children.Add(listContainer);

            // Footer
            DockPanel footer=new DockPanel{LastChildFill=false,Margin=new Thickness(0,8,0,0)};
            statusText=new TextBlock{Text="预设: "+currentPresetName,FontSize=11};
            DockPanel.SetDock(statusText,Dock.Left); footer.Children.Add(statusText);
            Grid.SetRow(footer,3); g.Children.Add(footer);

            rootBorder.Child=g; this.Content=rootBorder;
            this.KeyDown+=(s,e)=>{ if(e.Key==Key.Escape) this.Hide(); };

            ApplyTheme();
        }

        public void ApplyTheme() {
            rootBorder.Background = new SolidColorBrush(Win11Theme.BgWindow);
            rootBorder.BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle);
            titleText.Foreground = new SolidColorBrush(Win11Theme.FgPrimary);
            
            mainSetBtn.Style = Win11Theme.CreateButtonStyle(false);
            mainClearBtn.Style = Win11Theme.CreateButtonStyle(false);
            
            mainCloseBtn.Foreground = new SolidColorBrush(Win11Theme.FgSecondary);
            listContainer.Background = new SolidColorBrush(Win11Theme.BgSurface);
            listContainer.BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle);
            
            statusText.Foreground = new SolidColorBrush(Win11Theme.FgTertiary);

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
            trayIcon.DoubleClick+=(s,e)=>{this.Show();this.Activate();};
            var menu=new System.Windows.Forms.ContextMenuStrip();
            var m1=new System.Windows.Forms.ToolStripMenuItem("历史记录");
            m1.Click+=(s,e)=>{this.Show();this.Activate();};
            var m2=new System.Windows.Forms.ToolStripMenuItem("截图解析 (Alt+Q)");
            m2.Click+=(s,e)=>TriggerSnipAndAnalyze();
            var m3=new System.Windows.Forms.ToolStripMenuItem("设置");
            m3.Click+=(s,e)=>{this.Show();this.Activate();new SettingsDialog(this){Owner=this}.ShowDialog();};
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
                }catch(Exception ex){Logger.Error("AnalyzeFloating",ex);this.Dispatcher.Invoke(()=>fw.ShowResult("解析失败: "+ex.Message));}
            });
        }

        private string PostAI(string jsonBody) {
            var req=(HttpWebRequest)WebRequest.Create(apiBase);
            req.Method="POST";req.ContentType="application/json";
            req.Headers["Authorization"]="Bearer "+apiKey;
            req.UserAgent="Codex/1.0";req.Timeout=60000;
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
            statusText.Text="历史: "+historyItems.Count+" 条";
        }

        private ListBoxItem MakeDateHeader(string date){
            string label=date==DateTime.Now.ToString("yyyy-MM-dd")?"今天":
                date==DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")?"昨天":date;
            Border hb=new Border{
                Padding=new Thickness(8,6,8,4),Margin=new Thickness(0,4,0,2),
                BorderBrush=new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness=new Thickness(0,0,0,1)};
            hb.Child=new TextBlock{Text=label,FontSize=11,FontWeight=FontWeights.SemiBold,
                Foreground=new SolidColorBrush(Win11Theme.Accent)};
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

    public class SettingsDialog : Window {
        private TextBox TB, TK, CMBTEXT;
        private System.Windows.Controls.Primitives.Popup CMBPOPUP;
        private ListBox CMBLB;
        private TextBox PRESET_TEXT;
        private System.Windows.Controls.Primitives.Popup PRESET_POPUP;
        private ListBox PRESET_LB;
        private TextBox promptBox;
        private MainWindow M;
        private List<PromptPreset> localPresets = new List<PromptPreset>();

        public SettingsDialog(MainWindow main) {
            M = main;
            this.Title = "设置";
            this.Width = 620;
            this.Height = 600;
            this.MinWidth = 520;
            this.MinHeight = 500;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.Background = new SolidColorBrush(Win11Theme.BgWindow);
            this.SourceInitialized += (s, e) => Win11Theme.ApplyToWindow(this);

            if (M.promptPresets != null) {
                foreach (var p in M.promptPresets) localPresets.Add(new PromptPreset(p.Name, p.Content));
            }
            if (localPresets.Count == 0) localPresets = OmniDictConfig.GetDefaultPresets();

            Grid rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            ScrollViewer sv = new ScrollViewer {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };

            StackPanel sp = new StackPanel { Margin = new Thickness(24, 20, 24, 16) };

            TextBlock pageTitle = new TextBlock {
                Text = "系统设置",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Win11Theme.FgPrimary),
                FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI, Microsoft YaHei"),
                Margin = new Thickness(0, 0, 0, 4) };
            sp.Children.Add(pageTitle);

            TextBlock cfgPath = new TextBlock {
                Text = "配置文件路径: " + OmniDictConfig.ConfigPath,
                FontSize = 11,
                Foreground = new SolidColorBrush(Win11Theme.FgTertiary),
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei"),
                Margin = new Thickness(0, 0, 0, 16),
                TextWrapping = TextWrapping.Wrap };
            sp.Children.Add(cfgPath);

            // Group 1: 模型与接口
            sp.Children.Add(CreateSectionHeader("模型与接口"));

            StackPanel card1 = new StackPanel();
            TB = CreateWin11Input(M.apiBase, 280);
            card1.Children.Add(CreateSettingsRow("API 端点地址", "OpenAI 兼容推理接口 (Chat Completions)", TB));
            card1.Children.Add(CreateRowDivider());

            TK = CreateWin11Input(M.apiKey, 280);
            card1.Children.Add(CreateSettingsRow("API 密钥", "用于鉴权的 Bearer Token / API Key", TK));
            card1.Children.Add(CreateRowDivider());

            DockPanel modelRowWidget = new DockPanel { LastChildFill = true };
            Button fetchBtn = new Button { Content = "拉取列表", Width = 72, Height = 30, Cursor = Cursors.Hand, Margin = new Thickness(6, 0, 0, 0) };
            fetchBtn.Style = Win11Theme.CreateButtonStyle(false);
            DockPanel.SetDock(fetchBtn, Dock.Right);
            modelRowWidget.Children.Add(fetchBtn);

            Grid cmbGrid = new Grid { Height = 30, Width = 200 };
            cmbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cmbGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            CMBTEXT = CreateWin11Input(M.currentModel, 0);
            CMBTEXT.BorderThickness = new Thickness(1, 1, 0, 1);
            CMBTEXT.Height = 30;
            Grid.SetColumn(CMBTEXT, 0);
            cmbGrid.Children.Add(CMBTEXT);

            Button arrowBtn = new Button {
                Content = "▾", Width = 26, Height = 30,
                Background = new SolidColorBrush(Win11Theme.BgSurface),
                Foreground = new SolidColorBrush(Win11Theme.FgSecondary),
                BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness = new Thickness(0, 1, 1, 1),
                Cursor = Cursors.Hand };
            Grid.SetColumn(arrowBtn, 1);
            cmbGrid.Children.Add(arrowBtn);

            modelRowWidget.Children.Add(cmbGrid);
            card1.Children.Add(CreateSettingsRow("当前推理模型", "可直接输入名称或从接口列表拉取选择", modelRowWidget));
            sp.Children.Add(WrapInCard(card1));

            CMBLB = new ListBox {
                Background = new SolidColorBrush(Win11Theme.BgSurface),
                Foreground = new SolidColorBrush(Win11Theme.FgPrimary),
                BorderBrush = new SolidColorBrush(Win11Theme.BorderStrong),
                BorderThickness = new Thickness(1), MaxHeight = 220 };
            ScrollViewer.SetVerticalScrollBarVisibility(CMBLB, ScrollBarVisibility.Auto);
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
            CMBLB.ItemContainerStyle = lbItemStyle;
            CMBLB.SelectionChanged += (s, e) => {
                if (CMBLB.SelectedItem != null) {
                    CMBTEXT.Text = CMBLB.SelectedItem.ToString();
                    CMBPOPUP.IsOpen = false;
                }
            };
            CMBPOPUP = new System.Windows.Controls.Primitives.Popup {
                PlacementTarget = cmbGrid, Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                StaysOpen = false, Child = CMBLB };
            arrowBtn.Click += (s, e) => {
                if (CMBLB.Items.Count > 0) {
                    CMBPOPUP.Width = cmbGrid.ActualWidth + arrowBtn.ActualWidth;
                    CMBPOPUP.IsOpen = !CMBPOPUP.IsOpen;
                } else {
                    fetchBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            };

            fetchBtn.Click += (s, e) => {
                fetchBtn.IsEnabled = false; fetchBtn.Content = "...";
                string ep = TB.Text.Trim(); string key = TK.Text.Trim();
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
                                if (id.Length > 0 && !id.Contains("/") && id.IndexOf("codex", System.StringComparison.OrdinalIgnoreCase) < 0) ids.Add(id);
                                pos = q2 + 1;
                            }
                            ids.Sort();
                        }
                    } catch (Exception ex) { Logger.Error("FetchModels", ex); }
                    this.Dispatcher.Invoke(() => {
                        fetchBtn.Content = "拉取列表"; fetchBtn.IsEnabled = true;
                        if (ids.Count > 0) {
                            string cur = CMBTEXT.Text;
                            CMBLB.Items.Clear();
                            foreach (var id in ids) CMBLB.Items.Add(id);
                            CMBTEXT.Text = cur;
                            CMBPOPUP.Width = cmbGrid.ActualWidth + arrowBtn.ActualWidth;
                            CMBPOPUP.IsOpen = true;
                        }
                    });
                });
            };

            // Group 2: 解析预设与 System Prompt 自定义
            sp.Children.Add(CreateSectionHeader("AI 提示词与场景预设 (Prompt Presets)"));

            StackPanel cardPreset = new StackPanel();

            // Row 1: Preset selector and management buttons
            DockPanel presetBar = new DockPanel { LastChildFill = false, Margin = new Thickness(0, 4, 0, 8) };

            Grid presetGrid = new Grid { Height = 30, Width = 210 };
            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            PRESET_TEXT = CreateWin11Input("", 0);
            PRESET_TEXT.IsReadOnly = true;
            PRESET_TEXT.Cursor = Cursors.Hand;
            PRESET_TEXT.BorderThickness = new Thickness(1, 1, 0, 1);
            PRESET_TEXT.Height = 30;
            Grid.SetColumn(PRESET_TEXT, 0);
            presetGrid.Children.Add(PRESET_TEXT);

            Button presetArrowBtn = new Button {
                Content = "▾", Width = 26, Height = 30,
                Background = new SolidColorBrush(Win11Theme.BgSurface),
                Foreground = new SolidColorBrush(Win11Theme.FgSecondary),
                BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness = new Thickness(0, 1, 1, 1),
                Cursor = Cursors.Hand };
            Grid.SetColumn(presetArrowBtn, 1);
            presetGrid.Children.Add(presetArrowBtn);

            PRESET_LB = new ListBox {
                Background = new SolidColorBrush(Win11Theme.BgSurface),
                Foreground = new SolidColorBrush(Win11Theme.FgPrimary),
                BorderBrush = new SolidColorBrush(Win11Theme.BorderStrong),
                BorderThickness = new Thickness(1), MaxHeight = 220 };
            ScrollViewer.SetVerticalScrollBarVisibility(PRESET_LB, ScrollBarVisibility.Auto);
            PRESET_LB.ItemContainerStyle = lbItemStyle;

            PRESET_POPUP = new System.Windows.Controls.Primitives.Popup {
                PlacementTarget = presetGrid, Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                StaysOpen = false, Child = PRESET_LB };

            Action togglePresetPopup = () => {
                if (PRESET_LB.Items.Count > 0) {
                    PRESET_POPUP.Width = presetGrid.ActualWidth + presetArrowBtn.ActualWidth;
                    PRESET_POPUP.IsOpen = !PRESET_POPUP.IsOpen;
                }
            };
            presetArrowBtn.Click += (s, e) => togglePresetPopup();
            PRESET_TEXT.PreviewMouseLeftButtonDown += (s, e) => { togglePresetPopup(); e.Handled = true; };

            Action<string> SyncComboItems = null;
            SyncComboItems = (selectName) => {
                PRESET_LB.Items.Clear();
                foreach (var p in localPresets) PRESET_LB.Items.Add(p.Name);
                if (selectName != null && PRESET_LB.Items.Contains(selectName)) {
                    PRESET_LB.SelectedItem = selectName;
                    PRESET_TEXT.Text = selectName;
                } else if (PRESET_LB.Items.Count > 0) {
                    PRESET_LB.SelectedIndex = 0;
                    PRESET_TEXT.Text = PRESET_LB.Items[0].ToString();
                } else {
                    PRESET_TEXT.Text = "";
                }
            };

            PRESET_LB.SelectionChanged += (s, e) => {
                if (PRESET_LB.SelectedItem != null) {
                    string selName = PRESET_LB.SelectedItem.ToString();
                    PRESET_TEXT.Text = selName;
                    PRESET_POPUP.IsOpen = false;
                    var p = localPresets.Find(x => x.Name == selName);
                    if (p != null) promptBox.Text = p.Content;
                }
            };

            DockPanel.SetDock(presetGrid, Dock.Left);
            presetBar.Children.Add(presetGrid);

            StackPanel presetBtnBar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(8, 0, 0, 0) };
            Button addPresetBtn = new Button { Content = "新建预设", Height = 30, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 0) };
            addPresetBtn.Style = Win11Theme.CreateButtonStyle(false);
            addPresetBtn.Click += (s, e) => {
                string baseName = "新预设"; int count = 1;
                string newName = baseName;
                while (localPresets.Exists(x => x.Name == newName)) { count++; newName = baseName + count; }
                localPresets.Add(new PromptPreset(newName, "你是一位屏幕助手，请清晰准确解析截图中内容。"));
                SyncComboItems(newName);
            };
            presetBtnBar.Children.Add(addPresetBtn);

            Button renamePresetBtn = new Button { Content = "重命名", Height = 30, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 0) };
            renamePresetBtn.Style = Win11Theme.CreateButtonStyle(false);
            renamePresetBtn.Click += (s, e) => {
                if (string.IsNullOrEmpty(PRESET_TEXT.Text)) return;
                string curName = PRESET_TEXT.Text;
                var p = localPresets.Find(x => x.Name == curName);
                if (p == null) return;

                var promptWin = new Window {
                    Title = "重命名预设", Width = 360, Height = 170,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this, Background = new SolidColorBrush(Win11Theme.BgWindow),
                    ResizeMode = ResizeMode.NoResize };
                Win11Theme.ApplyToWindow(promptWin);

                var pg = new Grid { Margin = new Thickness(16) };
                pg.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                pg.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                pg.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var pLbl = new TextBlock { Text = "预设名称:", FontSize = 12, Foreground = new SolidColorBrush(Win11Theme.FgPrimary), Margin = new Thickness(0, 0, 0, 6) };
                Grid.SetRow(pLbl, 0); pg.Children.Add(pLbl);

                var pBox = CreateWin11Input(curName, 0);
                pBox.SelectAll();
                Grid.SetRow(pBox, 1); pg.Children.Add(pBox);

                var pBtns = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
                var pOk = new Button { Content = "确定", Width = 64, Height = 28, Margin = new Thickness(0, 0, 8, 0) };
                pOk.Style = Win11Theme.CreateButtonStyle(true);
                pOk.Click += (s2, e2) => {
                    string nn = pBox.Text.Trim();
                    if (!string.IsNullOrEmpty(nn) && nn != curName) {
                        p.Name = nn;
                        SyncComboItems(nn);
                    }
                    promptWin.Close();
                };
                pBtns.Children.Add(pOk);
                var pCancel = new Button { Content = "取消", Width = 64, Height = 28 };
                pCancel.Style = Win11Theme.CreateButtonStyle(false);
                pCancel.Click += (s2, e2) => promptWin.Close();
                pBtns.Children.Add(pCancel);

                Grid.SetRow(pBtns, 2); pg.Children.Add(pBtns);
                promptWin.Content = pg;
                promptWin.ShowDialog();
            };
            presetBtnBar.Children.Add(renamePresetBtn);

            Button delPresetBtn = new Button { Content = "删除预设", Height = 30, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 0) };
            delPresetBtn.Style = Win11Theme.CreateButtonStyle(false);
            delPresetBtn.Click += (s, e) => {
                if (string.IsNullOrEmpty(PRESET_TEXT.Text)) return;
                if (localPresets.Count <= 1) {
                    MessageBox.Show("至少需要保留一个预设！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string curName = PRESET_TEXT.Text;
                localPresets.RemoveAll(x => x.Name == curName);
                SyncComboItems(null);
            };
            presetBtnBar.Children.Add(delPresetBtn);

            Button resetPresetBtn = new Button { Content = "恢复默认预设", Height = 30, Padding = new Thickness(10, 0, 10, 0) };
            resetPresetBtn.Style = Win11Theme.CreateButtonStyle(false);
            resetPresetBtn.Click += (s, e) => {
                if (MessageBox.Show("确定将所有预设恢复为系统默认吗？当前修改将丢失。", "确认恢复", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    localPresets = OmniDictConfig.GetDefaultPresets();
                    SyncComboItems(null);
                }
            };
            presetBtnBar.Children.Add(resetPresetBtn);

            DockPanel.SetDock(presetBtnBar, Dock.Right);
            presetBar.Children.Add(presetBtnBar);
            cardPreset.Children.Add(presetBar);

            // Row 2: Prompt content editor
            TextBlock editLbl = new TextBlock {
                Text = "提示词模板 (System Prompt，支持实时自由修改)：",
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Win11Theme.FgSecondary),
                Margin = new Thickness(0, 6, 0, 4) };
            cardPreset.Children.Add(editLbl);

            promptBox = new TextBox {
                Height = 110,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(8),
                Background = new SolidColorBrush(Win11Theme.BgSurface),
                Foreground = new SolidColorBrush(Win11Theme.FgPrimary),
                CaretBrush = new SolidColorBrush(Win11Theme.FgPrimary),
                BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness = new Thickness(1),
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei"),
                FontSize = 12 };
            cardPreset.Children.Add(promptBox);

            promptBox.TextChanged += (s, e) => {
                if (!string.IsNullOrEmpty(PRESET_TEXT.Text)) {
                    string selName = PRESET_TEXT.Text;
                    var p = localPresets.Find(x => x.Name == selName);
                    if (p != null) p.Content = promptBox.Text;
                }
            };

            SyncComboItems(M.currentPresetName);
            sp.Children.Add(WrapInCard(cardPreset));

            // Group 3: 图像识别与输入模式
            sp.Children.Add(CreateSectionHeader("输入与识别模式"));

            StackPanel card2 = new StackPanel();
            CheckBox visionChk = new CheckBox {
                Content = "开启原生 Vision 多模态图像输入",
                IsChecked = M.useVision,
                Foreground = new SolidColorBrush(Win11Theme.FgPrimary),
                VerticalContentAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand };
            card2.Children.Add(CreateSettingsRow(
                "图像识别模式 (Multimodal Vision)",
                "直接上传截图供多模态模型端到端识别。取消勾选则优先调用 Windows 本地 OCR 引擎离线提取文本后再传入（适合纯文本模型）",
                visionChk));
            sp.Children.Add(WrapInCard(card2));

            // Group 4: 全局快捷键指南
            sp.Children.Add(CreateSectionHeader("全局快捷键操作指南"));

            StackPanel card3 = new StackPanel();
            TextBlock hk1 = new TextBlock {
                Text = "Alt + Q",
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Win11Theme.Accent),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Consolas, Segoe UI") };
            card3.Children.Add(CreateSettingsRow("截图查词 / 全场景屏幕解析", "在任意全屏游戏、专业软件或网页中呼出区域框选", hk1));
            card3.Children.Add(CreateRowDivider());

            TextBlock hk2 = new TextBlock {
                Text = "Alt + W",
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Win11Theme.Accent),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Consolas, Segoe UI") };
            card3.Children.Add(CreateSettingsRow("静默关闭悬浮窗", "完全不抢占游戏输入焦点，随时关闭释义浮窗", hk2));
            sp.Children.Add(WrapInCard(card3));

            sv.Content = sp;
            Grid.SetRow(sv, 0);
            rootGrid.Children.Add(sv);

            // Bottom Action Bar
            Border bottomBar = new Border {
                Background = new SolidColorBrush(Win11Theme.IsDarkTheme ? Color.FromRgb(28, 28, 28) : Color.FromRgb(240, 240, 240)),
                BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(24, 12, 24, 12) };

            StackPanel btns = new StackPanel {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right };

            Button save = new Button { Content = "保存设置", Width = 88, Height = 32, Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 8, 0) };
            save.Style = Win11Theme.CreateButtonStyle(true);
            save.Click += (s, e) => {
                M.apiBase = TB.Text.Trim();
                M.apiKey = TK.Text.Trim();
                M.currentModel = CMBTEXT.Text.Trim();
                M.useVision = visionChk.IsChecked == true;
                if (!string.IsNullOrEmpty(PRESET_TEXT.Text)) {
                    M.currentPresetName = PRESET_TEXT.Text;
                }
                M.promptPresets = localPresets;

                double sx = M.floatingWin != null && M.floatingWin.HasCustomPosition ? M.floatingWin.LastX : -1;
                double sy = M.floatingWin != null && M.floatingWin.HasCustomPosition ? M.floatingWin.LastY : -1;
                OmniDictConfig.Save(M.apiBase, M.apiKey, M.currentModel, M.useVision, sx, sy, M.currentPresetName, M.promptPresets);
                M.ApplyTheme();
                this.Close();
            };

            Button cancel = new Button { Content = "取消", Width = 72, Height = 32, Cursor = Cursors.Hand };
            cancel.Style = Win11Theme.CreateButtonStyle(false);
            cancel.Click += (s, e) => this.Close();

            btns.Children.Add(save);
            btns.Children.Add(cancel);
            bottomBar.Child = btns;

            Grid.SetRow(bottomBar, 1);
            rootGrid.Children.Add(bottomBar);

            this.Content = rootGrid;
        }

        private TextBlock CreateSectionHeader(string text) {
            return new TextBlock {
                Text = text,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Win11Theme.FgSecondary),
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei"),
                Margin = new Thickness(2, 12, 0, 6) };
        }

        private Border WrapInCard(UIElement content) {
            return new Border {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Win11Theme.BgSurface),
                BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 6),
                Child = content };
        }

        private Grid CreateSettingsRow(string header, string description, UIElement actionWidget) {
            Grid row = new Grid { Margin = new Thickness(0, 6, 0, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
            TextBlock h = new TextBlock {
                Text = header,
                FontSize = 13,
                FontWeight = FontWeights.Normal,
                Foreground = new SolidColorBrush(Win11Theme.FgPrimary),
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei") };
            textPanel.Children.Add(h);

            if (!string.IsNullOrEmpty(description)) {
                TextBlock desc = new TextBlock {
                    Text = description,
                    FontSize = 11.5,
                    Foreground = new SolidColorBrush(Win11Theme.FgTertiary),
                    FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei"),
                    Margin = new Thickness(0, 2, 0, 0),
                    TextWrapping = TextWrapping.Wrap };
                textPanel.Children.Add(desc);
            }

            Grid.SetColumn(textPanel, 0);
            row.Children.Add(textPanel);

            if (actionWidget != null) {
                Grid.SetColumn(actionWidget, 1);
                row.Children.Add(actionWidget);
            }

            return row;
        }

        private Border CreateRowDivider() {
            return new Border {
                Height = 1,
                Background = new SolidColorBrush(Win11Theme.BorderSubtle),
                Margin = new Thickness(0, 4, 0, 4) };
        }

        private TextBox CreateWin11Input(string text, double width = 0) {
            var tb = new TextBox {
                Text = text,
                Height = 30,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(Win11Theme.BgSurface),
                Foreground = new SolidColorBrush(Win11Theme.FgPrimary),
                CaretBrush = new SolidColorBrush(Win11Theme.FgPrimary),
                BorderBrush = new SolidColorBrush(Win11Theme.BorderSubtle),
                BorderThickness = new Thickness(1),
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Microsoft YaHei"),
                FontSize = 12 };
            if (width > 0) tb.Width = width;
            return tb;
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
            this.Title = "OmniDict Float"; this.Width = 560; this.Height = 700;
            this.WindowStyle = WindowStyle.None; this.AllowsTransparency = true;
            this.Background = Brushes.Transparent; this.Topmost = true;
            this.ShowInTaskbar = false; this.ResizeMode = ResizeMode.NoResize;
            this.Focusable = false;
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

        public void ShowLoading(double cursorX, double cursorY, byte[] imgBytes) {
            this.WindowState = WindowState.Normal; this.Width = 560; this.Height = GetPreferredHeight();
            if (HasCustomPosition && LastX >= 0 && LastY >= 0) {
                EnsureWithinScreen(LastX, LastY);
            } else {
                PositionAt(cursorX, cursorY);
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

        public void ShowResult(string text) { SetRichText(text); }

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
                string path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"gamedict_icon.ico");
                File.WriteAllBytes(path,bytes);return path;
            }catch{return null;}
        }
    }
}
