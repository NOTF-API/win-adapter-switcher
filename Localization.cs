using System;
using System.Collections.Generic;
using System.Globalization;

namespace WinAdapterSwitcher
{
    internal static partial class L
    {
        public const string Auto = "auto";
        public const string ChineseSimplified = "zh-CN";
        public const string ChineseTraditional = "zh-TW";
        public const string English = "en-US";
        public const string Japanese = "ja-JP";
        public const string Korean = "ko-KR";
        public const string German = "de-DE";
        public const string French = "fr-FR";
        public const string Spanish = "es-ES";

        public const string Chinese = ChineseSimplified;

        private static readonly Dictionary<string, string> ZhCnStrings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Status.Unknown", "状态未知" },
            { "Status.Disabled", "已禁用" },
            { "Status.Enabled", "已启用" },
            { "Status.EnabledConnected", "已启用（已连接）" },
            { "Status.EnabledDisconnected", "已启用（未连接）" },
            { "Status.NotConfigured", "未配置" },
            { "Status.NotFound", "未找到（{0}）" },
            { "Role.Wired", "有线" },
            { "Role.Wireless", "无线" },

            { "Settings.Title", "WinAdapterSwitcher 设置" },
            { "Settings.WiredLabel", "有线网卡（例如：以太网）" },
            { "Settings.WirelessLabel", "无线网卡（例如：WLAN）" },
            { "Settings.LanguageLabel", "界面语言" },
            { "Settings.LanguageAuto", "自动（跟随系统）" },
            { "Settings.Refresh", "刷新网卡列表" },
            { "Settings.Hint", "配置会保存到 exe 同目录的 config.ini" },
            { "Settings.Options", "选项" },
            { "Settings.ConfirmDisableAll", "执行「禁用全部网络」前弹出确认" },
            { "Settings.EnableLog", "启用日志文件（WinAdapterSwitcher.log，默认关闭）" },
            { "Settings.CleanAutoStart", "退出程序时清除开机自启任务" },
            { "Settings.AutoStart", "开机自启（登录时自动启动）" },
            { "Settings.AdminYes", "当前权限：管理员" },
            { "Settings.AdminNo", "当前权限：标准用户，无法启用或禁用网卡。" },
            { "Settings.RestartElevated", "以管理员身份重启" },
            { "Settings.Save", "保存" },
            { "Settings.Cancel", "取消" },
            { "Settings.Unconfigured", "（未配置）" },
            { "Settings.ReadAdaptersFailed", "读取网卡列表失败：{0}" },
            { "Settings.RestartFailed", "无法以管理员身份启动程序，请手动右键选择「以管理员身份运行」。" },
            { "Settings.DuplicateAdapter", "有线网卡和无线网卡不能选择同一块网卡。" },
            { "Settings.SaveFailed", "保存配置失败：{0}\r\n请确认程序所在目录可写。" },
            { "Settings.AutoStartFailed", "设置开机自启失败：{0}" },

            { "Menu.ReadingStatus", "正在读取网卡状态..." },
            { "Menu.WiredPriority", "仅开启有线网络" },
            { "Menu.WirelessPriority", "仅开启无线网络" },
            { "Menu.EnableAll", "启用全部网络" },
            { "Menu.DisableAll", "禁用全部网络" },
            { "Menu.Individual", "单独启用/禁用" },
            { "Menu.Refresh", "刷新状态" },
            { "Menu.Elevate", "以管理员身份重启" },
            { "Menu.Settings", "设置..." },
            { "Menu.Exit", "退出" },
            { "Menu.NoAdapters", "（未找到网络适配器）" },
            { "Menu.ReadFailed", "[读取失败]" },
            { "Menu.StatusLine", "有线：{0}    无线：{1}" },
            { "Menu.AdapterItem", "{0}（{1}）" },
            { "Tray.Tooltip", "有线：{0} / 无线：{1}" },
            { "Tray.AdapterStatusUnknown", "无法读取网卡「{0}」的当前状态，请先刷新状态。" },
            { "Tray.SettingsSaved", "设置已保存" },
            { "Confirm.DisableAll", "确定要禁用全部网络吗？\r\n此操作会中断网络连接。" },

            { "Action.WiredDone", "已切换为仅开启有线网络" },
            { "Action.WirelessDone", "已切换为仅开启无线网络" },
            { "Action.AllEnabled", "已启用全部网络。" },
            { "Action.AllDisabled", "已禁用全部网络。" },
            { "Action.EnableAdapter", "启用网卡" },
            { "Action.DisableAdapter", "禁用网卡" },
            { "Action.SwitchWired", "切换有线网络" },
            { "Action.SwitchWireless", "切换无线网络" },
            { "Action.EnableAll", "启用全部网络" },
            { "Action.DisableAll", "禁用全部网络" },
            { "Action.CommandLine", "命令行操作" },
            { "Action.EnabledAdapter", "已启用「{0}」" },
            { "Action.DisabledAdapter", "已禁用「{0}」" },

            { "Error.ConfigNeedsBoth", "请先在「设置」中配置有线网卡和无线网卡。" },
            { "Error.ConfigNeedsAny", "请先在「设置」中配置有线网卡或无线网卡。" },
            { "Error.NoAdapterSelected", "未选择网卡，请先在设置中配置。" },
            { "Error.AdminRequired", "需要管理员权限才能启用或禁用网卡。请以管理员身份重新启动程序。" },
            { "Error.AdapterNotFound", "未找到网卡「{0}」，请重新设置。" },
            { "Error.NetshStartFailed", "无法执行 netsh：{0}" },
            { "Error.NetshTimeout", "netsh 命令执行超时，网卡状态可能未改变。" },
            { "Error.EnableFailed", "启用「{0}」失败" },
            { "Error.DisableFailed", "禁用「{0}」失败" },
            { "Error.NetshExitCode", "（netsh 返回码 {0}）" },
            { "Error.NotEffective", "（操作未生效）" },
            { "Error.DetailSuffix", "：{0}" },
            { "Error.ReadAdapterList", "无法读取网络适配器列表：{0}" },
            { "Error.WmiDetail", "（WMI：{0}）" },
            { "Error.NetshNoList", "netsh 未返回网卡列表" },
            { "Error.NetshShowFailed", "netsh interface show interface 执行失败" },
            { "Error.AutoStartTask", "无法创建或读取开机自启计划任务。" },
            { "Error.EnableNameRequired", "请指定要启用的网卡名称。" },
            { "Error.DisableNameRequired", "请指定要禁用的网卡名称。" },
            { "Error.UnknownArgument", "未知参数：{0}\r\n\r\n{1}" },
            { "Error.WiredDetail", "有线：{0}" },
            { "Error.WirelessDetail", "无线：{0}" },

            { "Program.ConfigLoadFailed", "读取 config.ini 失败，将使用默认配置。\r\n{0}" },
            { "Program.AdminRestartPrompt", "启用或禁用网卡需要管理员权限。\r\n\r\n是否以管理员身份重新启动程序？" },
            { "Program.AdminRestartFailed", "以管理员身份启动失败，将继续以受限模式运行。" },
            { "Program.AutoStartInvalid", "检测到开机自启任务指向的路径已失效：\r\n{0}\r\n\r\n已自动清理该任务。" },
            { "Program.TrayStartFailed", "程序启动失败：{0}" },
            { "Program.UnhandledError", "程序发生错误：{0}" },

            { "Usage.Header", "用法：" },
            { "Usage.StartTray", "  WinAdapterSwitcher.exe                 启动托盘程序" },
            { "Usage.Wired", "  WinAdapterSwitcher.exe --wired         仅开启有线网络" },
            { "Usage.Wifi", "  WinAdapterSwitcher.exe --wifi          仅开启无线网络" },
            { "Usage.Enable", "  WinAdapterSwitcher.exe --enable 网卡名 启用指定网卡" },
            { "Usage.Disable", "  WinAdapterSwitcher.exe --disable 网卡名 禁用指定网卡" },
            { "Usage.AllEnable", "  WinAdapterSwitcher.exe --all-enable    启用全部网络" },
            { "Usage.AllDisable", "  WinAdapterSwitcher.exe --all-disable   禁用全部网络" },
            { "Usage.Settings", "  WinAdapterSwitcher.exe --settings      打开设置" },
            { "Usage.Menu", "  WinAdapterSwitcher.exe --menu          打开托盘菜单" },
            { "Usage.Help", "  WinAdapterSwitcher.exe --help          显示本帮助" }
        };

        private static readonly Dictionary<string, string> ZhTwStrings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Status.Unknown", "狀態未知" },
            { "Status.Disabled", "已停用" },
            { "Status.Enabled", "已啟用" },
            { "Status.EnabledConnected", "已啟用（已連線）" },
            { "Status.EnabledDisconnected", "已啟用（未連線）" },
            { "Status.NotConfigured", "未設定" },
            { "Status.NotFound", "找不到（{0}）" },
            { "Role.Wired", "有線" },
            { "Role.Wireless", "無線" },

            { "Settings.Title", "WinAdapterSwitcher 設定" },
            { "Settings.WiredLabel", "有線網卡（例如：乙太網路）" },
            { "Settings.WirelessLabel", "無線網卡（例如：WLAN）" },
            { "Settings.LanguageLabel", "介面語言" },
            { "Settings.LanguageAuto", "自動（跟隨系統）" },
            { "Settings.Refresh", "重新整理網卡清單" },
            { "Settings.Hint", "設定會儲存到 exe 同目錄的 config.ini" },
            { "Settings.Options", "選項" },
            { "Settings.ConfirmDisableAll", "執行「停用全部網路」前顯示確認" },
            { "Settings.EnableLog", "啟用記錄檔（WinAdapterSwitcher.log，預設關閉）" },
            { "Settings.CleanAutoStart", "結束程式時移除開機自動啟動工作" },
            { "Settings.AutoStart", "開機自動啟動（登入時自動啟動）" },
            { "Settings.AdminYes", "目前權限：系統管理員" },
            { "Settings.AdminNo", "目前權限：標準使用者，無法啟用或停用網卡。" },
            { "Settings.RestartElevated", "以系統管理員身分重新啟動" },
            { "Settings.Save", "儲存" },
            { "Settings.Cancel", "取消" },
            { "Settings.Unconfigured", "（未設定）" },
            { "Settings.ReadAdaptersFailed", "讀取網卡清單失敗：{0}" },
            { "Settings.RestartFailed", "無法以系統管理員身分啟動程式，請手動按右鍵選擇「以系統管理員身分執行」。" },
            { "Settings.DuplicateAdapter", "有線網卡和無線網卡不能選擇同一張網卡。" },
            { "Settings.SaveFailed", "儲存設定失敗：{0}\r\n請確認程式所在目錄可寫入。" },
            { "Settings.AutoStartFailed", "設定開機自動啟動失敗：{0}" },

            { "Menu.ReadingStatus", "正在讀取網卡狀態..." },
            { "Menu.WiredPriority", "僅開啟有線網路" },
            { "Menu.WirelessPriority", "僅開啟無線網路" },
            { "Menu.EnableAll", "啟用全部網路" },
            { "Menu.DisableAll", "停用全部網路" },
            { "Menu.Individual", "個別啟用/停用" },
            { "Menu.Refresh", "重新整理狀態" },
            { "Menu.Elevate", "以系統管理員身分重新啟動" },
            { "Menu.Settings", "設定..." },
            { "Menu.Exit", "結束" },
            { "Menu.NoAdapters", "（找不到網路介面卡）" },
            { "Menu.ReadFailed", "[讀取失敗]" },
            { "Menu.StatusLine", "有線：{0}    無線：{1}" },
            { "Menu.AdapterItem", "{0}（{1}）" },
            { "Tray.Tooltip", "有線：{0} / 無線：{1}" },
            { "Tray.AdapterStatusUnknown", "無法讀取網卡「{0}」的目前狀態，請先重新整理狀態。" },
            { "Tray.SettingsSaved", "設定已儲存" },
            { "Confirm.DisableAll", "確定要停用全部網路嗎？\r\n此操作會中斷網路連線。" },

            { "Action.WiredDone", "已切換為僅開啟有線網路" },
            { "Action.WirelessDone", "已切換為僅開啟無線網路" },
            { "Action.AllEnabled", "已啟用全部網路。" },
            { "Action.AllDisabled", "已停用全部網路。" },
            { "Action.EnableAdapter", "啟用網卡" },
            { "Action.DisableAdapter", "停用網卡" },
            { "Action.SwitchWired", "切換有線網路" },
            { "Action.SwitchWireless", "切換無線網路" },
            { "Action.EnableAll", "啟用全部網路" },
            { "Action.DisableAll", "停用全部網路" },
            { "Action.CommandLine", "命令列操作" },
            { "Action.EnabledAdapter", "已啟用「{0}」" },
            { "Action.DisabledAdapter", "已停用「{0}」" },

            { "Error.ConfigNeedsBoth", "請先在「設定」中設定有線網卡和無線網卡。" },
            { "Error.ConfigNeedsAny", "請先在「設定」中設定有線網卡或無線網卡。" },
            { "Error.NoAdapterSelected", "未選擇網卡，請先在設定中設定。" },
            { "Error.AdminRequired", "需要系統管理員權限才能啟用或停用網卡。請以系統管理員身分重新啟動程式。" },
            { "Error.AdapterNotFound", "找不到網卡「{0}」，請重新設定。" },
            { "Error.NetshStartFailed", "無法執行 netsh：{0}" },
            { "Error.NetshTimeout", "netsh 命令執行逾時，網卡狀態可能未變更。" },
            { "Error.EnableFailed", "啟用「{0}」失敗" },
            { "Error.DisableFailed", "停用「{0}」失敗" },
            { "Error.NetshExitCode", "（netsh 傳回碼 {0}）" },
            { "Error.NotEffective", "（操作未生效）" },
            { "Error.DetailSuffix", "：{0}" },
            { "Error.ReadAdapterList", "無法讀取網路介面卡清單：{0}" },
            { "Error.WmiDetail", "（WMI：{0}）" },
            { "Error.NetshNoList", "netsh 未傳回網卡清單" },
            { "Error.NetshShowFailed", "netsh interface show interface 執行失敗" },
            { "Error.AutoStartTask", "無法建立或讀取開機自動啟動工作。" },
            { "Error.EnableNameRequired", "請指定要啟用的網卡名稱。" },
            { "Error.DisableNameRequired", "請指定要停用的網卡名稱。" },
            { "Error.UnknownArgument", "未知的參數：{0}\r\n\r\n{1}" },
            { "Error.WiredDetail", "有線：{0}" },
            { "Error.WirelessDetail", "無線：{0}" },

            { "Program.ConfigLoadFailed", "讀取 config.ini 失敗，將使用預設設定。\r\n{0}" },
            { "Program.AdminRestartPrompt", "啟用或停用網卡需要系統管理員權限。\r\n\r\n是否以系統管理員身分重新啟動程式？" },
            { "Program.AdminRestartFailed", "以系統管理員身分啟動失敗，將繼續以受限模式執行。" },
            { "Program.AutoStartInvalid", "偵測到開機自動啟動工作指向的路徑已失效：\r\n{0}\r\n\r\n已自動移除該工作。" },
            { "Program.TrayStartFailed", "程式啟動失敗：{0}" },
            { "Program.UnhandledError", "程式發生錯誤：{0}" },

            { "Usage.Header", "用法：" },
            { "Usage.StartTray", "  WinAdapterSwitcher.exe                 啟動托盤程式" },
            { "Usage.Wired", "  WinAdapterSwitcher.exe --wired         僅開啟有線網路" },
            { "Usage.Wifi", "  WinAdapterSwitcher.exe --wifi          僅開啟無線網路" },
            { "Usage.Enable", "  WinAdapterSwitcher.exe --enable 網卡名 啟用指定網卡" },
            { "Usage.Disable", "  WinAdapterSwitcher.exe --disable 網卡名 停用指定網卡" },
            { "Usage.AllEnable", "  WinAdapterSwitcher.exe --all-enable    啟用全部網路" },
            { "Usage.AllDisable", "  WinAdapterSwitcher.exe --all-disable   停用全部網路" },
            { "Usage.Settings", "  WinAdapterSwitcher.exe --settings      開啟設定" },
            { "Usage.Menu", "  WinAdapterSwitcher.exe --menu          開啟托盤選單" },
            { "Usage.Help", "  WinAdapterSwitcher.exe --help          顯示本說明" }
        };

        private static readonly Dictionary<string, string> EnStrings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Status.Unknown", "Unknown" },
            { "Status.Disabled", "Disabled" },
            { "Status.Enabled", "Enabled" },
            { "Status.EnabledConnected", "Enabled (connected)" },
            { "Status.EnabledDisconnected", "Enabled (disconnected)" },
            { "Status.NotConfigured", "Not configured" },
            { "Status.NotFound", "Not found ({0})" },
            { "Role.Wired", "Wired" },
            { "Role.Wireless", "Wireless" },

            { "Settings.Title", "WinAdapterSwitcher Settings" },
            { "Settings.WiredLabel", "Wired adapter (e.g. Ethernet)" },
            { "Settings.WirelessLabel", "Wireless adapter (e.g. WLAN)" },
            { "Settings.LanguageLabel", "Language" },
            { "Settings.LanguageAuto", "Automatic (system language)" },
            { "Settings.Refresh", "Refresh adapters" },
            { "Settings.Hint", "Settings are saved to config.ini next to the exe" },
            { "Settings.Options", "Options" },
            { "Settings.ConfirmDisableAll", "Confirm before disabling all networks" },
            { "Settings.EnableLog", "Enable log file (WinAdapterSwitcher.log, off by default)" },
            { "Settings.CleanAutoStart", "Remove the auto-start task when exiting" },
            { "Settings.AutoStart", "Start with Windows at sign-in" },
            { "Settings.AdminYes", "Current privileges: Administrator" },
            { "Settings.AdminNo", "Current privileges: standard user; adapters cannot be enabled or disabled." },
            { "Settings.RestartElevated", "Restart as administrator" },
            { "Settings.Save", "Save" },
            { "Settings.Cancel", "Cancel" },
            { "Settings.Unconfigured", "(Not configured)" },
            { "Settings.ReadAdaptersFailed", "Failed to read the adapter list: {0}" },
            { "Settings.RestartFailed", "Failed to start as administrator. Right-click the program and choose \"Run as administrator\"." },
            { "Settings.DuplicateAdapter", "Wired and wireless adapters must be different." },
            { "Settings.SaveFailed", "Failed to save settings: {0}\r\nMake sure the program folder is writable." },
            { "Settings.AutoStartFailed", "Failed to update auto-start: {0}" },

            { "Menu.ReadingStatus", "Reading adapter status..." },
            { "Menu.WiredPriority", "Wired network only" },
            { "Menu.WirelessPriority", "Wireless network only" },
            { "Menu.EnableAll", "Enable all networks" },
            { "Menu.DisableAll", "Disable all networks" },
            { "Menu.Individual", "Enable/disable an adapter" },
            { "Menu.Refresh", "Refresh status" },
            { "Menu.Elevate", "Restart as administrator" },
            { "Menu.Settings", "Settings..." },
            { "Menu.Exit", "Exit" },
            { "Menu.NoAdapters", "(No network adapters found)" },
            { "Menu.ReadFailed", "[read failed]" },
            { "Menu.StatusLine", "Wired: {0}    Wireless: {1}" },
            { "Menu.AdapterItem", "{0} ({1})" },
            { "Tray.Tooltip", "Wired: {0} / Wireless: {1}" },
            { "Tray.AdapterStatusUnknown", "Cannot read the current status of adapter \"{0}\". Refresh the status first." },
            { "Tray.SettingsSaved", "Settings saved" },
            { "Confirm.DisableAll", "Disable all networks?\r\nThis will interrupt network connectivity." },

            { "Action.WiredDone", "Switched to wired network only." },
            { "Action.WirelessDone", "Switched to wireless network only." },
            { "Action.AllEnabled", "All networks enabled." },
            { "Action.AllDisabled", "All networks disabled." },
            { "Action.EnableAdapter", "Enable adapter" },
            { "Action.DisableAdapter", "Disable adapter" },
            { "Action.SwitchWired", "Switch to wired network" },
            { "Action.SwitchWireless", "Switch to wireless network" },
            { "Action.EnableAll", "Enable all networks" },
            { "Action.DisableAll", "Disable all networks" },
            { "Action.CommandLine", "Command line action" },
            { "Action.EnabledAdapter", "Enabled \"{0}\"" },
            { "Action.DisabledAdapter", "Disabled \"{0}\"" },

            { "Error.ConfigNeedsBoth", "Configure both the wired and wireless adapters in Settings first." },
            { "Error.ConfigNeedsAny", "Configure at least one adapter in Settings first." },
            { "Error.NoAdapterSelected", "No adapter selected. Configure one in Settings first." },
            { "Error.AdminRequired", "Administrator privileges are required to enable or disable adapters. Restart the program as administrator." },
            { "Error.AdapterNotFound", "Adapter \"{0}\" was not found. Configure it again in Settings." },
            { "Error.NetshStartFailed", "Unable to run netsh: {0}" },
            { "Error.NetshTimeout", "The netsh command timed out; the adapter state may not have changed." },
            { "Error.EnableFailed", "Failed to enable \"{0}\"" },
            { "Error.DisableFailed", "Failed to disable \"{0}\"" },
            { "Error.NetshExitCode", " (netsh exit code {0})" },
            { "Error.NotEffective", " (operation had no effect)" },
            { "Error.DetailSuffix", ": {0}" },
            { "Error.ReadAdapterList", "Unable to read the network adapter list: {0}" },
            { "Error.WmiDetail", " (WMI: {0})" },
            { "Error.NetshNoList", "netsh did not return an adapter list" },
            { "Error.NetshShowFailed", "netsh interface show interface failed" },
            { "Error.AutoStartTask", "Unable to create or read the auto-start scheduled task." },
            { "Error.EnableNameRequired", "Specify the name of the adapter to enable." },
            { "Error.DisableNameRequired", "Specify the name of the adapter to disable." },
            { "Error.UnknownArgument", "Unknown argument: {0}\r\n\r\n{1}" },
            { "Error.WiredDetail", "Wired: {0}" },
            { "Error.WirelessDetail", "Wireless: {0}" },

            { "Program.ConfigLoadFailed", "Failed to read config.ini. Default settings will be used.\r\n{0}" },
            { "Program.AdminRestartPrompt", "Administrator privileges are required to enable or disable adapters.\r\n\r\nRestart the program as administrator?" },
            { "Program.AdminRestartFailed", "Failed to restart as administrator. Continuing in limited mode." },
            { "Program.AutoStartInvalid", "The auto-start task points to a path that no longer exists:\r\n{0}\r\n\r\nThe task has been removed." },
            { "Program.TrayStartFailed", "Failed to start: {0}" },
            { "Program.UnhandledError", "An error occurred: {0}" },

            { "Usage.Header", "Usage:" },
            { "Usage.StartTray", "  WinAdapterSwitcher.exe                 Start the tray application" },
            { "Usage.Wired", "  WinAdapterSwitcher.exe --wired         Wired network only" },
            { "Usage.Wifi", "  WinAdapterSwitcher.exe --wifi          Wireless network only" },
            { "Usage.Enable", "  WinAdapterSwitcher.exe --enable NAME   Enable the named adapter" },
            { "Usage.Disable", "  WinAdapterSwitcher.exe --disable NAME  Disable the named adapter" },
            { "Usage.AllEnable", "  WinAdapterSwitcher.exe --all-enable    Enable all networks" },
            { "Usage.AllDisable", "  WinAdapterSwitcher.exe --all-disable   Disable all networks" },
            { "Usage.Settings", "  WinAdapterSwitcher.exe --settings      Open settings" },
            { "Usage.Menu", "  WinAdapterSwitcher.exe --menu          Open the tray menu" },
            { "Usage.Help", "  WinAdapterSwitcher.exe --help          Show this help" }
        };

        private static readonly Dictionary<string, Dictionary<string, string>> Tables;
        private static string _currentLanguage;

        static L()
        {
            Tables = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            Tables[ChineseSimplified] = ZhCnStrings;
            Tables[ChineseTraditional] = ZhTwStrings;
            Tables[English] = EnStrings;
            Tables[Japanese] = JaStrings;
            Tables[Korean] = KoStrings;
            Tables[German] = DeStrings;
            Tables[French] = FrStrings;
            Tables[Spanish] = EsStrings;
            SetLanguage(Auto);
        }

        public static string CurrentLanguage
        {
            get { return _currentLanguage; }
        }

        public static bool IsChinese
        {
            get { return _currentLanguage == ChineseSimplified || _currentLanguage == ChineseTraditional; }
        }

        public static string Normalize(string setting)
        {
            if (string.IsNullOrEmpty(setting))
            {
                return Auto;
            }

            string value = setting.Trim().ToLowerInvariant();
            if (value == Auto || value == "system" || value == "default")
            {
                return Auto;
            }
            if (value == "cn" || value == "chinese" || value == "simplified chinese")
            {
                return ChineseSimplified;
            }
            if (value == "traditional chinese" || value == "traditional")
            {
                return ChineseTraditional;
            }
            if (value.StartsWith("zh", StringComparison.Ordinal))
            {
                if (value.IndexOf("hant", StringComparison.Ordinal) >= 0 ||
                    value.IndexOf("tw", StringComparison.Ordinal) >= 0 ||
                    value.IndexOf("hk", StringComparison.Ordinal) >= 0 ||
                    value.IndexOf("mo", StringComparison.Ordinal) >= 0)
                {
                    return ChineseTraditional;
                }
                return ChineseSimplified;
            }
            if (value.StartsWith("en", StringComparison.Ordinal))
            {
                return English;
            }
            if (value.StartsWith("ja", StringComparison.Ordinal) || value == "jp")
            {
                return Japanese;
            }
            if (value.StartsWith("ko", StringComparison.Ordinal) || value == "kr")
            {
                return Korean;
            }
            if (value.StartsWith("de", StringComparison.Ordinal))
            {
                return German;
            }
            if (value.StartsWith("fr", StringComparison.Ordinal))
            {
                return French;
            }
            if (value.StartsWith("es", StringComparison.Ordinal))
            {
                return Spanish;
            }
            return Auto;
        }

        public static void SetLanguage(string setting)
        {
            string normalized = Normalize(setting);
            if (normalized == Auto)
            {
                normalized = DetectSystemLanguage();
            }
            _currentLanguage = normalized;
        }

        private static string DetectSystemLanguage()
        {
            string cultureName = "";
            try
            {
                cultureName = CultureInfo.CurrentUICulture.Name;
            }
            catch (Exception)
            {
            }

            string detected = Normalize(cultureName);
            return detected == Auto ? English : detected;
        }

        public static string T(string key)
        {
            Dictionary<string, string> table = GetTable(_currentLanguage);
            string value;
            if (table != null && table.TryGetValue(key, out value))
            {
                return value;
            }

            Dictionary<string, string> fallback = GetTable(English);
            if (fallback != null && fallback.TryGetValue(key, out value))
            {
                return value;
            }
            return key;
        }

        public static string F(string key, params object[] args)
        {
            string format = T(key);
            try
            {
                return string.Format(CultureInfo.CurrentCulture, format, args);
            }
            catch (Exception)
            {
                return format;
            }
        }

        private static Dictionary<string, string> GetTable(string language)
        {
            Dictionary<string, string> table;
            if (language != null && Tables.TryGetValue(language, out table))
            {
                return table;
            }
            return null;
        }
    }
}
