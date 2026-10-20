# WinAdapterSwitcher

[English](../README.md) | **简体中文**

Windows 10 / 11 网络适配器快速切换工具。常驻系统托盘，通过右键菜单在有线网络和无线网络之间一键切换，也可以单独启用或禁用任意网卡。

- 单文件绿色版：`WinAdapterSwitcher.exe`，无需安装
- 目标平台：Windows 10 / Windows 11
- 运行时依赖：系统自带的 .NET Framework 4.8，无需额外安装
- 不联网、不上传数据、无广告

## 开发初衷

日常使用中，运营商的 DNS 污染、连接拦截，甚至直接 RESET 连接，经常需要临时切换到手机热点。但 Windows 每次手动禁用/启用网卡都要经过“设置 → 网络和 Internet → 高级网络设置 → 更多网络适配器选项 → 右键网卡 → 禁用/启用”等多层菜单，紧急切换时很麻烦。

这个项目由我提出需求和场景，并借助 AI 完成代码实现：把有线/无线切换和网卡启停收进托盘右键菜单，尽量一键完成。

## 功能

- 托盘图标显示有线/无线网卡状态（已启用、已禁用、状态未知）
- 一键切换：
  - 仅开启有线网络
  - 仅开启无线网络
  - 启用全部网络
  - 禁用全部网络（默认二次确认）
- 单独启用/禁用任意网卡
- 设置中手动选择“有线网卡”和“无线网卡”，首次运行会自动识别并保存
- 界面支持简体中文、繁體中文、English、日本語、한국어、Deutsch、Français、Español，首次运行自动跟随系统语言，也可在设置中手动切换
- 开机自启开关（登录时通过计划任务以最高权限启动）
- 可选日志、退出时清理自启任务
- 命令行参数，支持从快捷方式或脚本调用
- 单实例运行，重复启动会唤醒已有实例

## 使用

1. 下载并解压 Release 中的 zip。
2. 双击 `WinAdapterSwitcher.exe`。程序声明了 `requireAdministrator`，Windows 会弹出 UAC 提权提示。
3. 程序启动后出现在系统托盘。右键托盘图标打开菜单。

> 启用/禁用网卡需要管理员权限，这是 Windows 的限制。程序通过 `app.manifest` 在启动时请求提权。

### 托盘菜单

| 菜单项 | 说明 |
| --- | --- |
| 状态行 | 显示已配置的有线/无线网卡当前状态 |
| 仅开启有线网络 | 开启有线，关闭无线 |
| 仅开启无线网络 | 开启无线，关闭有线 |
| 启用全部网络 | 启用已配置的所有网络 |
| 禁用全部网络 | 禁用已配置的所有网络（默认弹窗确认） |
| 单独启用/禁用 | 列出当前所有网卡，点击即可切换启用状态 |
| 刷新状态 | 立即重新读取网卡状态 |
| 以管理员身份重启 | 仅在未提权运行时出现 |
| 设置... | 选择网卡、开机自启、日志等 |
| 退出 | 退出程序 |

双击托盘图标可直接打开设置。

### 设置项

- **有线网卡 / 无线网卡**：从当前系统网卡中选择，保存名称和描述
- **界面语言**：自动（跟随系统）或手动选择 8 种语言之一
- **执行「禁用全部网络」前弹出确认**：默认开启
- **启用日志文件**：默认关闭，开启后在 exe 同目录生成 `WinAdapterSwitcher.log`
- **退出程序时清除开机自启任务**：默认关闭
- **开机自启**：创建当前用户的登录计划任务，以最高权限启动

### 配置文件

配置保存在 **exe 同目录的 `config.ini`**，移动整个程序目录后配置仍然生效。格式如下：

```ini
; WinAdapterSwitcher configuration
; This file is stored next to WinAdapterSwitcher.exe.

[Adapters]
Wired=以太网
WiredDescription=Realtek PCIe GbE Family Controller
Wireless=WLAN
WirelessDescription=Intel(R) WiFi Link 5100 AGN

[Options]
Language=auto
ConfirmDisableAll=1
EnableLog=0
CleanAutoStartOnExit=0
```

`Language` 可选值为 `auto`、`zh-CN`、`zh-TW`、`en-US`、`ja-JP`、`ko-KR`、`de-DE`、`fr-FR`、`es-ES`。`auto` 会根据系统界面语言自动选择对应语言；无法识别时回退到 English。

若配置的网卡名称在当前系统中不存在，托盘状态会显示“未找到”，操作时会提示重新设置，程序不会崩溃。

## 命令行

命令行参数主要用于快捷方式、计划任务或脚本调用。程序已在运行时，新实例会把命令转发给已有实例。由于程序是 GUI 应用，独立执行命令时结果会以弹窗形式显示。

```text
WinAdapterSwitcher.exe                 启动托盘程序
WinAdapterSwitcher.exe --wired         仅开启有线网络
WinAdapterSwitcher.exe --wifi          仅开启无线网络
WinAdapterSwitcher.exe --enable 网卡名 启用指定网卡
WinAdapterSwitcher.exe --disable 网卡名 禁用指定网卡
WinAdapterSwitcher.exe --all-enable    启用全部网络
WinAdapterSwitcher.exe --all-disable   禁用全部网络
WinAdapterSwitcher.exe --settings      打开设置
WinAdapterSwitcher.exe --menu          打开托盘菜单
WinAdapterSwitcher.exe --help          显示帮助
```

## 构建

构建**不需要 Visual Studio、.NET SDK、Rust、Go、Node.js 或任何包管理器**，也不需要联网。所有构建都在本地一键完成，本项目**不使用任何 CI/CD**。

### 源码语法约束

代码必须兼容 **C# 5**，因为 Windows 自带的 `csc.exe` 只支持到 C# 5。禁止引入以下语法：

- 字符串插值 `$""`
- 空条件运算符 `?.`
- `nameof`
- 表达式体成员（`=>` 方法/属性）
- 元组、`using static`、`out var`、模式匹配
- 其他 C# 6 及以上语法

构建脚本使用 `/langversion:5` 强制检查。源文件为 UTF-8，脚本通过 `/codepage:65001` 指定编码。

### Windows 10 / 11

要求：系统自带 .NET Framework 4.x 的 `csc.exe`。

```bat
build.bat
```

脚本会自动查找：

```text
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
```

找不到编译器时会输出安装或启用 .NET Framework 4.8 的提示，不会直接崩溃。

### 编译参数

`build.bat` 使用以下编译参数：

```text
/target:winexe
/out:WinAdapterSwitcher.exe
/win32manifest:app.manifest
/win32icon:app.ico
/reference:System.dll
/reference:System.Drawing.dll
/reference:System.Windows.Forms.dll
/reference:System.Core.dll
/reference:System.Management.dll
/reference:System.Xml.dll
/langversion:5
```

> 网卡枚举和状态读取通过 `System.Management` 查询 `MSFT_NetAdapter`；启用/禁用通过系统自带的 `netsh interface set interface` 完成。如果 WMI 不可用，会自动回退到解析 `netsh interface show interface`。

Windows 构建成功后，仓库根目录生成单个 `WinAdapterSwitcher.exe`，正常大小约 150 KB，不含同目录 DLL。

## 发布

Release 仅发布在 GitHub Releases，由维护者在本地构建后手动上传：

1. 在 Windows 10 / 11 上执行 `build.bat`。
2. 在 Windows 10 / 11 上做基本功能验证。
3. 将 `WinAdapterSwitcher.exe` 打包为 zip。
4. 计算 SHA256 校验值：

   ```powershell
   Get-FileHash .\WinAdapterSwitcher.exe -Algorithm SHA256
   ```

5. 创建 GitHub Release，上传 zip，并在说明中附上 SHA256、版本号、更新内容和支持的系统版本。

仓库中不包含任何 CI 配置文件。

## 开机自启与卸载

开机自启使用 Windows 计划任务，任务名为 `WinAdapterSwitcher`：

- 触发：当前用户登录时（延迟约 10 秒）
- 权限：最高权限，避免 `requireAdministrator` 程序被登录启动机制拦截
- 操作：启动 `WinAdapterSwitcher.exe` 的完整路径
- 不写 `HKCU\...\Run`、不写 HKLM、不注册服务

设置或取消自启需要管理员权限（程序启动时已提权）。旧版本写入的 `HKCU\...\Run\WinAdapterSwitcher` 会在程序启动时自动迁移并删除。

如果直接删除程序目录而没有关闭自启，下次登录时任务会失效。程序重新运行时会检测到失效路径并提示，随后自动清理该任务。也可以在“任务计划程序”中删除 `WinAdapterSwitcher`，或重新运行程序后在设置中关闭开机自启。

除 exe、`config.ini`、可选的 `WinAdapterSwitcher.log` 和计划任务外，程序不产生其他文件；删除程序目录和计划任务即可完成卸载。

## 项目结构

```text
Program.cs                 入口、单实例、命令行入口
TrayApplicationContext.cs  托盘图标、右键菜单、状态刷新
AdapterService.cs          网卡枚举、状态读取、启停（WMI + netsh 回退）
AdapterDetector.cs         首次运行时自动识别有线/无线网卡
SwitchEngine.cs            方案切换与结果汇总
SettingsForm.cs            设置窗口
Localization*.cs           8 种语言的界面文案
Config.cs                  config.ini 读写
AutoStart.cs               Windows 计划任务自启
Ipc.cs                     单实例命令转发（命名管道）
CommandLine.cs             命令行参数解析
TrayIcons.cs               运行时托盘状态图标
SecurityUtil.cs            管理员权限检测与提权重启
Logger.cs                  可选日志
app.manifest               管理员权限、系统兼容性、DPI
app.ico                    程序图标
build.bat                  一键构建脚本
docs/README.zh-CN.md       简体中文 README
```

## 许可证

[MIT License](../LICENSE)
