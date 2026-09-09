# GameDict AI 🎮📖

GameDict AI 是一款面向全语种游戏玩家、外语学习者的智能桌面悬浮翻译助手。针对游戏内生词难查、频繁切屏打断体验的问题而设计，提供无缝的「全语种截图识别 -> AI 视觉语境深度拆解 -> 半透明悬浮窗展示」完整工作流。

![Icon](gamedict_preview.png)

## ✨ 核心特性

- **跨语言智能识别与翻译**：
  - 支持英语、日语（假名/汉字）、韩语、法德西俄等全球主流语种自动识别。
  - 角色设定为资深游戏本地化专家，结合游戏语境输出：**地道中文翻译**、**核心词汇/语法解析（音标、词性、原形及游戏梗/背景）**、**实用例句拓展**。
- **极速快捷键工作流**：
  - 全局快捷键 Alt+Q 唤起 Windows 原生 Snipping 截图。
  - 截图松开后自动提取剪贴板，智能并发请求 AI 分析。
- **纯黑半透明悬浮窗**：
  - 结果直接以纯黑毛玻璃风格半透明悬浮窗展示，不抢占当前游戏操作焦点。
  - **位置智能记忆**：任意拖动后自动记住停靠屏幕坐标，下次查词不再遮挡游戏核心对话框。
  - **富文本排版**：支持 Markdown 标题、加粗高亮与列表圆点排版，压缩行距，清晰易读。
- **双模输入机制**：
  - **视觉模式（默认）**：将截图 Base64 直接提交给多模态大模型（如 Gemini、GPT-4o、Claude 等）。
  - **纯文本/OCR 模式**：内置基于 Windows WinRT 的多语种 OCR 引擎，自动提取文字后再请求纯文本大模型。
- **历史记录与持久化**：
  - 历史主窗口支持**按日期折叠分组**（今天、昨天、指定日期）。
  - 支持**同时保存截图缩略图**与文本解析，支持复查、放大预览与一键复制。
- **免安装单文件绿色运行**：
  - 纯原生 C# 实现，内置 Windows 原生 PE 矢量图标资源，常驻系统托盘，轻量低占用。

## 🛠️ 构建与编译

Windows 原生编译（无需安装大型开发环境，使用系统内置的 .NET Framework 4.8 编译器即可）：

`powershell
 = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
 = "\WPF"

& "\csc.exe" /target:winexe 
  "/win32icon:gamedict.ico" 
  "/r:\PresentationFramework.dll" 
  "/r:\PresentationCore.dll" 
  "/r:\WindowsBase.dll" 
  "/r:\System.Xaml.dll" 
  "/r:\System.dll" 
  "/r:\System.Windows.Forms.dll" 
  "/r:\System.Drawing.dll" 
  /out:"GameDict.exe" "Program.cs"
`

## ⚙️ 配置说明

首次启动后，在主界面点击 **设置 (Settings)** 即可填入你的 OpenAI 兼容 API 信息：
- **API 地址**：例如 https://api.openai.com/v1/chat/completions
- **API 密钥**：sk-...
- **模型**：可手动输入或点击「获取」按钮自动拉取服务端可用模型列表。
- **图像模式**：开启时直接上传截图；关闭时先走本地 OCR 再请求纯文本模型。

配置文件将自动保存在 %APPDATA%\GameDict\gamedict.toml。

## 📜 许可证

MIT License
