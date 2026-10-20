# Windows 10 / 11 网络适配器快速切换工具 —— 需求文档

## 1. 项目概述

在 Windows 10 / 11 下，用户需要频繁在“有线网络”和“无线网络”之间切换，或单独启用/禁用某一个网络适配器。系统自带操作路径较深，本项目提供一个轻量开源小工具，常驻系统托盘，通过右键菜单快速完成网卡启停和方案切换。

典型场景：运营商的 DNS 污染、连接拦截或直接 RESET 连接时，需要临时切换到手机热点；手动启停网卡步骤过多，因此将常用操作收进托盘菜单。项目由使用者提出需求和场景，并借助 AI 完成代码实现。

- **项目名（仓库名）**：`win-adapter-switcher`
- **可执行文件名**：`WinAdapterSwitcher.exe`
- **托盘显示名**：`WinAdapterSwitcher`
- **计划任务名**：`WinAdapterSwitcher`

> 说明：Windows 的“任务栏”本身不适合放置自定义切换按钮，本工具的交互形态为**系统托盘/通知区域常驻图标 + 右键菜单**。可选支持“固定快捷方式到任务栏”作为辅助入口。

## 2. 目标平台与用户

- 必须支持：**Windows 10 / Windows 11**（Windows 10 建议测试 21H2 / 22H2）
- 目标用户定位：**能正常访问 GitHub 的用户**。不针对无法访问 GitHub 的用户做适配，这属于有意筛选，不视为缺陷。
- 界面语言：简体中文、繁體中文、English、日本語、한국어、Deutsch、Français、Español

## 3. 功能需求（必须实现）

### 3.1 常驻系统托盘
- 程序启动后常驻系统托盘/通知区域。
- 托盘图标反映当前网络状态，至少区分：
  - 有线已启用 / 无线已禁用
  - 有线已禁用 / 无线已启用
  - 两者均启用
  - 两者均禁用
  - 状态未知或权限不足
- 右键托盘图标弹出菜单。

### 3.2 一键方案切换
托盘右键菜单至少包含：
1. **启用有线 + 禁用无线**
2. **禁用有线 + 启用无线**
3. **全部启用**
4. **全部禁用**（可选，建议加二次确认）
5. **仅启用/禁用指定适配器**
6. **刷新状态**
7. **设置**
8. **退出**

### 3.3 单独控制某个网络适配器
- 能列出当前系统的网络适配器。
- 用户可在设置中选择：
  - 哪个是“有线网卡”（如“以太网”）
  - 哪个是“无线网卡”（如“WLAN”）
- 支持对任意选中的适配器执行：启用 / 禁用 / 查询状态。
- 适配器识别不能只依赖显示名称，建议同时记录：
  - 显示名称
  - 描述
  - 接口 ID / GUID（如果技术栈支持）
- 如果配置的适配器不存在，托盘菜单提示“未找到网卡，请重新设置”。

### 3.4 管理员权限
- 启用/禁用网卡通常需要管理员权限。
- 程序启动时检测是否具有管理员权限：
  - 有权限：正常操作。
  - 无权限：提示“需要管理员权限”，并提供“以管理员身份重启”入口。
- `app.manifest` 声明 `requireAdministrator`，启动即提权。

### 3.5 配置持久化
- 保存用户选择的“有线网卡”“无线网卡”和常用方案。
- 下次启动自动加载。
- 配置文件位置：**exe 同目录**（见 6.5），绿色优先。

### 3.6 开机自启
- 提供设置项“开机自启”开关：
  - 开启：创建或更新当前用户的登录计划任务 `WinAdapterSwitcher`，以最高权限启动 exe。
  - 关闭：删除该计划任务，并清理旧版 HKCU Run 项。
- Windows 10/11 对 `requireAdministrator` 程序的 HKCU Run 登录启动不可靠，因此使用计划任务。
- 设置/取消自启需要管理员权限（程序启动时已提权）。
- 程序启动时校验：若任务指向的 exe 路径已失效（如目录被移动），提示用户并自动清理该任务。
- 兼容旧版：检测到 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\WinAdapterSwitcher` 时，迁移为计划任务并删除旧项。
- 不写系统服务、不写 HKLM。

### 3.7 错误处理与反馈
- 操作成功/失败给出明确提示，例如托盘气泡或状态栏文字。
- 失败时显示原因摘要，例如：
  - 权限不足
  - 网卡不存在
  - 网卡被占用
  - 命令执行失败
- 可选记录简单日志文件，便于排查（默认关闭，见 6.5）。

## 4. 可选增强功能

以下不是必须，但建议预留或分阶段实现：
- 全局快捷键，例如 `Ctrl+Alt+1` 切换“有线优先”，`Ctrl+Alt+2` 切换“无线优先”。
- 单击托盘图标执行默认方案；双击打开设置。
- 支持命令行参数，例如：
  - `WinAdapterSwitcher.exe --wired`
  - `WinAdapterSwitcher.exe --wifi`
  - `WinAdapterSwitcher.exe --disable WLAN`
- 支持导入/导出配置。
- 支持多套方案，例如“办公室”“家庭”“断网测试”。
- 操作前确认弹窗可配置开关，防止误禁用。
- 支持静默启动、最小化启动。

## 5. 技术栈与构建约束（硬性要求）

### 5.1 总体原则
- **不得要求安装 Visual Studio、.NET SDK、Rust 工具链、Go SDK、MinGW/MSVC Build Tools 等大型开发环境。**
- **不得要求联网下载依赖包**（如 NuGet 包、npm 包）。
- 编译过程应能在**干净的 Windows 10 机器**上完成，仅依赖系统自带组件。
- 产物应为**单个轻量 exe**，目标机不依赖额外运行时安装。
- **不使用任何 CI/CD**：所有构建必须由开发者在**本地**一键完成。Release 由维护者本地构建后手动上传 GitHub。

### 5.2 技术栈
- **C# + .NET Framework 4.8 + WinForms**
- 理由：
  - Windows 10 自带 .NET Framework 4.8 运行时，目标机无需安装。
  - WinForms 的 `NotifyIcon` 原生支持系统托盘和右键菜单。
  - 产物 exe 通常只有几十 KB 到几百 KB，资源占用极低。
  - 编译命令一行即可完成，可写成 `build.bat`。

### 5.3 编译器选择策略

| 平台 | 编译器 | 获取方式 |
|---|---|---|
| Windows 10 / 11 | `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（不存在则回退 `Framework\v4.0.30319\csc.exe`） | **系统自带，零安装** |

**明确不采用**：.NET SDK（`dotnet build`）作为默认构建方式。若用户本机恰好装了 .NET SDK，可作为可选路径，但不是必需。

### 5.4 源码语法约束
- 代码必须兼容 **C# 5**（Windows 系统 `csc.exe` 上限）。
- 禁用：字符串插值 `$""`、`?.`、`nameof`、表达式体成员、元组、`using static`、`out var`、模式匹配等 C# 6+ 语法。
- README 明确写出此约束，防止后续贡献者引入高版本语法导致 Windows 构建失败。

### 5.5 一键构建脚本
- 必须提供 `build.bat`：Windows 10 / 11 一键构建。
- 脚本不得要求用户手动改路径、手动传参数。
- 构建过程不需要联网（编译器由系统自带）。

### 5.6 编译参数
```
/target:winexe
/out:WinAdapterSwitcher.exe
/win32manifest:app.manifest
/win32icon:app.ico
/reference:System.dll
/reference:System.Drawing.dll
/reference:System.Windows.Forms.dll
/reference:System.Core.dll
/langversion:5
```
### 5.7 构建失败提示
脚本检测不到 `csc.exe` 时，必须输出：
- 已搜索的编译器路径
- 安装或启用 .NET Framework 4.8 的提示
- 提示“也可直接下载 GitHub Releases 中的已编译版本”

### 5.9 明确排除的方案
以下方案因“环境庞大 / 下载麻烦 / 产物臃肿 / 依赖 CI”被排除：
- Visual Studio / VS Build Tools（数 GB）
- .NET SDK + `dotnet build`（数百 MB，自包含发布产物大）
- Rust / Cargo
- Go SDK
- Python + PyInstaller
- Electron / Node.js
- C/C++ + MinGW/MSVC
- GitHub Actions、Travis、AppVeyor 等任何 CI

## 6. 产物与分发

### 6.1 许可证
- 项目采用 **MIT License**。
- 仓库根目录包含标准 `LICENSE` 文件，README 注明许可证类型。

### 6.2 分发渠道
- **仅通过 GitHub Releases 分发**，不要求提供国内镜像站。
- Release 页面提供：
  - 绿色版 zip（解压即用，单个 `WinAdapterSwitcher.exe`）
  - SHA256 校验值
  - 版本号、更新说明、支持的系统版本

### 6.3 安装方式
- 免安装绿色版：下载 zip → 解压 → 双击 `WinAdapterSwitcher.exe`。
- 除“开机自启”可选的计划任务（及旧版 Run 项迁移清理）外，不写注册表、不写系统目录、不注册服务。
- 删除程序目录和计划任务即彻底卸载（见 6.6）。

### 6.4 单文件绿色输出
- 编译产物为**单个 `WinAdapterSwitcher.exe`**，不依赖同目录 DLL。
- 不生成安装包、不注册服务；除可选计划任务外不写系统目录。
- 目标机运行前提：Windows 10 自带 .NET Framework 4.8（系统自带，无需用户安装）。
- 产物大小目标：**≤ 1 MB**（正常 WinForms 小工具应在几十到几百 KB）。

### 6.5 配置与数据
- 配置文件：**exe 同目录 `config.ini`**（轻量 INI 格式，无需第三方库）。
- 日志（可选）：同目录 `WinAdapterSwitcher.log`，默认关闭，可在设置中开启。
- 除 `WinAdapterSwitcher.exe`、`config.ini`、`WinAdapterSwitcher.log` 和可选计划任务外，不产生其他文件。
- 移动整个程序目录后，配置仍生效；若已开启自启，程序会检测失效任务路径并提示清理。

### 6.6 开机自启的绿色规范
- 使用 Windows 计划任务 `WinAdapterSwitcher`，当前用户登录时触发（延迟约 10 秒）。
- 以最高权限运行，避免 `requireAdministrator` 程序被登录启动机制拦截。
- 任务操作指向 `WinAdapterSwitcher.exe` 的完整路径。
- 设置/取消自启需要管理员权限（程序已提权）。
- 程序启动时校验任务指向的路径是否存在：
  - 不存在：提示用户并自动清理该任务。
- 程序提供“退出时清理自启任务”选项，默认关闭（保留用户设置）。
- 兼容旧版 HKCU Run 项：启动时迁移为计划任务并删除旧项。
- README 说明：若直接删除程序目录而未关闭自启，需删除计划任务，或重新运行程序关闭自启。

### 6.7 图标与清单
- `app.manifest` 声明 `requireAdministrator`，启动即提权。
- 提供 `app.ico`，编译时嵌入。

## 7. 技术实现建议

- 网卡启停可选实现方式（任选其一，建议封装为函数便于替换）：
  1. 调用 `netsh interface set interface "网卡名称" admin=enable/disable`
  2. 使用 PowerShell `Enable-NetAdapter` / `Disable-NetAdapter`
  3. 使用 Windows 网络管理 API
- 如果使用 `netsh`，需注意网卡名称必须与系统“网络连接”中的名称一致。
- 托盘图标使用 `NotifyIcon`。
- 程序应为单实例运行，避免多个托盘图标。
- 不联网、不上传数据、无广告。
- 源码建议组织为少量 `.cs` 文件 + `build.bat` + `app.manifest` + `app.ico`，**不引入项目文件（.csproj）和包管理器**。

## 8. 非功能需求

- **轻量**：空闲时 CPU/内存占用尽量低；产物 ≤ 1 MB。
- **稳定**：连续切换多次不崩溃、不残留多个进程。
- **易用**：常用切换操作不超过 2 次点击。
- **安全**：不修改与需求无关的系统网络配置。
- **兼容**：至少覆盖 Windows 10 x64 常见版本。
- **隐私**：不联网、不上传任何数据。

## 9. 验收标准

满足以下条件视为合格：

**功能类**
1. 在 Windows 10 上可正常启动并常驻系统托盘。
2. 右键托盘菜单可执行“仅开启有线网络”。
3. 右键托盘菜单可执行“仅开启无线网络”。
4. 可在设置中单独选择并启用/禁用任意一个网络适配器。
5. 操作后托盘图标或菜单状态能刷新并反映当前状态。
6. 非管理员运行时能提示提权，提权后可正常操作。
7. 重启程序后配置仍然保留。
8. 配置的网卡不存在时有明确错误提示，不崩溃。
9. 程序无联网行为、无广告、资源占用低。
10. 开机自启开关只创建/删除计划任务 `WinAdapterSwitcher`，并清理旧版 HKCU Run 项；不触碰 HKLM。
11. exe 路径失效时，程序能检测并提示清理失效自启任务。

**构建类**
12. 仓库根目录存在标准 MIT `LICENSE` 文件。
13. Release 仅发布在 GitHub Releases，提供绿色版 zip 与 SHA256。
14. 在**未安装 Visual Studio / .NET SDK** 的 Windows 10 / 11 上，双击 `build.bat` 能成功生成 `WinAdapterSwitcher.exe`。
15. 构建出的 exe 在 Windows 10 和 Windows 11 上行为一致。
16. 仓库中**不存在任何 CI 配置文件**（如 `.github/workflows/`），README 说明构建为本地完成。
17. 产物为单个 `WinAdapterSwitcher.exe`，大小 ≤ 1 MB，无同目录依赖 DLL。
18. 源码不含 C# 6+ 语法，Windows 系统 `csc.exe` 编译通过。
19. 目标机无需安装 .NET 运行时即可运行（依赖系统自带 .NET Framework 4.8）。
20. 构建脚本在缺少编译器时，输出安装或启用 .NET Framework 4.8 的提示，不直接崩溃。
21. 配置文件 `config.ini` 位于 exe 同目录，移动整个目录后配置仍生效。

## 10. 待确认问题

以下事项需在开工前确认：

1. 配置文件格式用 **INI**（建议，轻量无需第三方库）还是 JSON？
2. 程序退出时，自启项默认**保留**还是提供“退出时询问是否保留自启”选项？
3. 日志功能是否需要？若需要，默认关闭是否可接受？
4. 全局快捷键是否为第一期必须？还是列入可选增强？
5. “禁用全部网络”是否需要二次确认弹窗？确认弹窗是否可配置？
6. 网卡识别策略：自动识别“以太网/WLAN”+ 允许用户手动改选，是否满足需求？

---

## 给其他 AI 的开场提示（可直接复制）

> 请按以下需求文档实现一个 Windows 10 网络适配器切换工具，项目名 `win-adapter-switcher`，可执行文件 `WinAdapterSwitcher.exe`。硬约束：
> 1. MIT 许可证。
> 2. C# + WinForms，目标 .NET Framework 4.8。
> 3. 仅支持 Windows 10 / 11，使用系统自带 `csc.exe` 编译。
> 4. **不使用任何 CI**，所有构建由开发者本地一键完成；Release 由维护者本地构建后手动上传 GitHub。
> 5. 代码限 C# 5 语法。
> 6. 一键构建脚本检测不到编译器时，输出安装或启用 .NET Framework 4.8 的提示。
> 7. 产物为单个 `WinAdapterSwitcher.exe`（≤1 MB），目标机零运行时安装。
> 8. 配置文件 `config.ini` 放 exe 同目录。
> 9. 开机自启使用计划任务 `WinAdapterSwitcher`（登录时最高权限启动），并兼容清理旧版 HKCU Run 项。
> 10. Release 仅发布在 GitHub Releases。
>
> 请优先完成第 3 章“必须实现”部分，给出技术方案、项目结构、关键代码和构建说明。若第 10 章“待确认问题”未明确，请先列出澄清问题再开始。

---