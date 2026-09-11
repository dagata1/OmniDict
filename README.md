# OmniDict AI 🌐📖

OmniDict AI 是一款专为游戏玩家、开发者、外语学习者以及多屏工作者打造的**现代化全场景屏幕智能悬浮解析助手**。

无论是全屏游戏对话、专业技术文档、IDE/终端代码报错，还是外网社区长图，按下快捷键即可享受**「无感截屏 -> AI 多模态视觉解析 -> 静默磨砂悬浮窗」**的流畅工作流。

![Icon](omnidict_preview.png)

## ✨ 核心特性

- **Windows 11 原生 Fluent 质感**：
  - 全面支持**系统深色/浅色模式自动检测与秒级动态跟随**。
  - 标准 Win11 SettingsCard 设置面板设计，圆角毛玻璃窗口搭配沉浸式 DWM 标题栏。
- **全场景 AI 提示词与预设系统 (Prompt Presets)**：
  - 内置多款针对性预设：
    1. **游戏本地化与攻略私教**（中文翻译 + 核心重点词汇/梗 + 游戏机制大师操作建议 + 顺便学一句）；
    2. **极简极速直译**（极致精炼、无冗余的即时查词与对照）；
    3. **程序员技术排错与代码分析**（根因诊断 + 修复方案 + 关键词解析）；
    4. **外语学习与深度语法精读**（长难句拆解 + 词性时态分析 + 地道例句）。
  - 支持用户自由新建、实时编辑、重命名、删除或恢复系统默认预设。
- **沉浸式不抢焦点工作流**：
  - **Alt + Q**：唤起 Windows 原生 Snipping 截图，松开后自动并发请求大模型解析。
  - **Alt + W**：全局静默隐藏悬浮窗（亦可直接点击悬浮窗右上角的快捷标签关闭），不依赖容易被全屏游戏抢断的 Esc 键。
  - 悬浮窗底层采用 `WS_EX_NOACTIVATE` 属性，**完全不掠夺游戏/IDE 操作焦点**，打游戏、写代码毫不中断。
  - **位置智能记忆**：拖动一次后自动记住屏幕坐标，后续弹出绝不遮挡原内容。
- **全字符集与转义符兼容**：
  - 深度解析 JSON 控制字符，完美支持 `\u003e` 等各种 Unicode 十六进制特殊符号反转义呈现。
- **双模智能输入**：
  - **原生 Vision 模式（推荐）**：端到端提交给 Vision 多模态模型（GPT-4o、Gemini、Claude 等）。
  - **离线 OCR 模式**：调用 Windows 本地内置 WinRT 多语种 OCR 引擎提取文字后请求纯文本模型。
- **历史记录与截图归档**：
  - 支持按日期层级分组查看，自动归档本地截图与完整解析富文本。
- **单文件绿色无外部依赖**：
  - 基于 C# 5 / .NET Framework 4.8 原生静态编译，零第三方 DLL 依赖，开箱即用。

## 🛠️ 构建与编译

使用 Windows 系统内置的 .NET Framework 编译器即可单行构建：

```powershell
$netPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
$wpfPath = "$netPath\WPF"

& "$netPath\csc.exe" /target:winexe `
  "/win32icon:omnidict.ico" `
  "/r:$wpfPath\PresentationFramework.dll" `
  "/r:$wpfPath\PresentationCore.dll" `
  "/r:$wpfPath\WindowsBase.dll" `
  "/r:$netPath\System.Xaml.dll" `
  "/r:$netPath\System.dll" `
  "/r:$netPath\System.Windows.Forms.dll" `
  "/r:$netPath\System.Drawing.dll" `
  /out:"OmniDict.exe" "Program.cs"
```

## ⚙️ 快速上手

1. 启动 `OmniDict.exe`；
2. 打开「设置」填入 OpenAI 兼容的 API 基础地址（如 `https://api.openai.com/v1/chat/completions`）及 API 密钥；
3. 点击「拉取列表」选取当前模型；
4. 在任何游戏或软件中按下 **Alt + Q**，圈选屏幕任意区域即时查词与解析；按 **Alt + W** 瞬间关闭悬浮窗。

## 📜 许可证

MIT License
