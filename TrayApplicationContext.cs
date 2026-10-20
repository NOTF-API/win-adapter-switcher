using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinAdapterSwitcher
{
    internal sealed class TrayApplicationContext : ApplicationContext
    {
        private readonly Config _config;
        private readonly SwitchEngine _engine;

        private readonly Control _dispatcher;
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _menu;
        private Timer _timer;
        private IpcServer _ipc;

        private ToolStripMenuItem _statusItem;
        private ToolStripMenuItem _wiredPriorityItem;
        private ToolStripMenuItem _wirelessPriorityItem;
        private ToolStripMenuItem _allEnableItem;
        private ToolStripMenuItem _allDisableItem;
        private ToolStripMenuItem _individualItem;
        private ToolStripMenuItem _refreshItem;
        private ToolStripMenuItem _elevateItem;
        private ToolStripMenuItem _settingsItem;
        private ToolStripMenuItem _exitItem;

        private readonly Dictionary<string, Icon> _iconCache = new Dictionary<string, Icon>();
        private Icon _currentIcon;

        private List<AdapterInfo> _adapters = new List<AdapterInfo>();
        private string _lastError = "";
        private bool _disposed;

        public TrayApplicationContext(Config config)
        {
            _config = config;
            _engine = new SwitchEngine(config);

            _dispatcher = new Control();
            _dispatcher.CreateControl();

            BuildMenu();
            CreateNotifyIcon();

            RefreshStatus();
            if (AdapterDetector.EnsureConfigured(_config, _adapters))
            {
                RefreshStatus();
            }

            _timer = new Timer();
            _timer.Interval = 10000;
            _timer.Tick += TimerTick;
            _timer.Start();

            _ipc = new IpcServer(_dispatcher, HandleIpcCommand);
            _ipc.Start();

            Logger.Write("Tray application started");
        }

        private void CreateNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();
            _currentIcon = GetStateIcon(null, null);
            _notifyIcon.Icon = _currentIcon;
            _notifyIcon.Text = "WinAdapterSwitcher";
            _notifyIcon.ContextMenuStrip = _menu;
            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += NotifyIconDoubleClick;
        }

        private void BuildMenu()
        {
            _menu = new ContextMenuStrip();
            _menu.Opening += MenuOpening;
            _menu.ShowImageMargin = false;

            _statusItem = new ToolStripMenuItem(L.T("Menu.ReadingStatus"));
            _statusItem.Enabled = false;
            _menu.Items.Add(_statusItem);
            _menu.Items.Add(new ToolStripSeparator());

            _wiredPriorityItem = new ToolStripMenuItem(L.T("Menu.WiredPriority"));
            _wiredPriorityItem.Click += WiredPriorityClick;
            _menu.Items.Add(_wiredPriorityItem);

            _wirelessPriorityItem = new ToolStripMenuItem(L.T("Menu.WirelessPriority"));
            _wirelessPriorityItem.Click += WirelessPriorityClick;
            _menu.Items.Add(_wirelessPriorityItem);

            _allEnableItem = new ToolStripMenuItem(L.T("Menu.EnableAll"));
            _allEnableItem.Click += AllEnableClick;
            _menu.Items.Add(_allEnableItem);

            _allDisableItem = new ToolStripMenuItem(L.T("Menu.DisableAll"));
            _allDisableItem.Click += AllDisableClick;
            _menu.Items.Add(_allDisableItem);

            _menu.Items.Add(new ToolStripSeparator());

            _individualItem = new ToolStripMenuItem(L.T("Menu.Individual"));
            _menu.Items.Add(_individualItem);

            _refreshItem = new ToolStripMenuItem(L.T("Menu.Refresh"));
            _refreshItem.Click += RefreshClick;
            _menu.Items.Add(_refreshItem);

            _elevateItem = new ToolStripMenuItem(L.T("Menu.Elevate"));
            _elevateItem.Visible = !SecurityUtil.IsAdministrator();
            _elevateItem.Click += ElevateClick;
            _menu.Items.Add(_elevateItem);

            _settingsItem = new ToolStripMenuItem(L.T("Menu.Settings"));
            _settingsItem.Click += SettingsClick;
            _menu.Items.Add(_settingsItem);

            _menu.Items.Add(new ToolStripSeparator());

            _exitItem = new ToolStripMenuItem(L.T("Menu.Exit"));
            _exitItem.Click += ExitClick;
            _menu.Items.Add(_exitItem);
        }

        private void MenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RefreshStatus();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_menu != null && _menu.Visible)
            {
                return;
            }
            RefreshStatus();
        }

        private void NotifyIconDoubleClick(object sender, EventArgs e)
        {
            OpenSettings();
        }

        private void RefreshStatus()
        {
            try
            {
                _adapters = AdapterService.GetAdapters();
                _lastError = "";
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                _adapters = new List<AdapterInfo>();
                Logger.WriteFormat("Refresh failed: {0}", ex.Message);
            }

            AdapterInfo wired = FindConfigured(_config.WiredAdapter);
            AdapterInfo wireless = FindConfigured(_config.WirelessAdapter);

            Icon icon = GetStateIcon(StatusOf(wired, _config.WiredAdapter), StatusOf(wireless, _config.WirelessAdapter));
            if (_notifyIcon != null && !ReferenceEquals(_currentIcon, icon))
            {
                _notifyIcon.Icon = icon;
                _currentIcon = icon;
            }

            UpdateTooltip(wired, wireless);
            UpdateMenuTexts(wired, wireless);
        }

        private void UpdateTooltip(AdapterInfo wired, AdapterInfo wireless)
        {
            if (_notifyIcon == null)
            {
                return;
            }
            string text = L.F("Tray.Tooltip",
                ShortStatus(_config.WiredAdapter, wired),
                ShortStatus(_config.WirelessAdapter, wireless));
            if (text.Length > 63)
            {
                text = text.Substring(0, 63);
            }
            try
            {
                _notifyIcon.Text = text;
            }
            catch (Exception)
            {
            }
        }

        private void UpdateMenuTexts(AdapterInfo wired, AdapterInfo wireless)
        {
            if (_menu == null)
            {
                return;
            }

            string status = L.F("Menu.StatusLine",
                ShortStatus(_config.WiredAdapter, wired),
                ShortStatus(_config.WirelessAdapter, wireless));
            if (_lastError.Length > 0)
            {
                status = status + "    " + L.T("Menu.ReadFailed");
            }
            _statusItem.Text = status;
            _statusItem.ToolTipText = _lastError;

            while (_individualItem.DropDownItems.Count > 0)
            {
                ToolStripItem old = _individualItem.DropDownItems[0];
                _individualItem.DropDownItems.RemoveAt(0);
                old.Dispose();
            }

            if (_adapters.Count == 0)
            {
                ToolStripMenuItem empty = new ToolStripMenuItem(L.T("Menu.NoAdapters"));
                empty.Enabled = false;
                _individualItem.DropDownItems.Add(empty);
                return;
            }

            for (int i = 0; i < _adapters.Count; i++)
            {
                AdapterInfo adapter = _adapters[i];
                string text = adapter.Name;
                if (!string.IsNullOrEmpty(adapter.Description))
                {
                    text = text + " - " + adapter.Description;
                }
                text = L.F("Menu.AdapterItem", text, ShortAdapterStatus(adapter));

                ToolStripMenuItem item = new ToolStripMenuItem(text);
                item.Tag = adapter;
                item.Click += IndividualAdapterClick;
                _individualItem.DropDownItems.Add(item);
            }
        }

        private void IndividualAdapterClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            AdapterInfo adapter = item == null ? null : item.Tag as AdapterInfo;
            if (adapter == null)
            {
                return;
            }
            if (!adapter.AdminKnown)
            {
                MessageBox.Show(L.F("Tray.AdapterStatusUnknown", adapter.Name),
                    "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool target = !adapter.Enabled;
            OperationResult result = _engine.SetAdapter(adapter.Name, target);
            RunAction(target ? L.T("Action.EnableAdapter") : L.T("Action.DisableAdapter"), result);
        }

        private void WiredPriorityClick(object sender, EventArgs e)
        {
            RunAction(L.T("Action.SwitchWired"), _engine.ApplyWiredPriority());
        }

        private void WirelessPriorityClick(object sender, EventArgs e)
        {
            RunAction(L.T("Action.SwitchWireless"), _engine.ApplyWirelessPriority());
        }

        private void AllEnableClick(object sender, EventArgs e)
        {
            RunAction(L.T("Action.EnableAll"), _engine.EnableAll());
        }

        private void AllDisableClick(object sender, EventArgs e)
        {
            if (_config.ConfirmDisableAll)
            {
                DialogResult answer = MessageBox.Show(
                    L.T("Confirm.DisableAll"),
                    "WinAdapterSwitcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes)
                {
                    return;
                }
            }
            RunAction(L.T("Action.DisableAll"), _engine.DisableAll());
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            RefreshStatus();
        }

        private void SettingsClick(object sender, EventArgs e)
        {
            OpenSettings();
        }

        private void ElevateClick(object sender, EventArgs e)
        {
            if (SecurityUtil.RestartElevated())
            {
                ExitApplication();
            }
            else
            {
                MessageBox.Show(L.T("Settings.RestartFailed"),
                    "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExitClick(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void OpenSettings()
        {
            RefreshStatus();
            using (SettingsForm form = new SettingsForm(_config, _adapters))
            {
                DialogResult result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _config.CopyFrom(form.SavedConfig);
                    L.SetLanguage(_config.Language);
                    Logger.SetEnabled(_config.LogEnabled);
                    ApplyLanguage();
                    ShowBalloon(L.T("Tray.SettingsSaved"), ToolTipIcon.Info);
                }
            }
        }

        private void ApplyLanguage()
        {
            _statusItem.Text = L.T("Menu.ReadingStatus");
            _wiredPriorityItem.Text = L.T("Menu.WiredPriority");
            _wirelessPriorityItem.Text = L.T("Menu.WirelessPriority");
            _allEnableItem.Text = L.T("Menu.EnableAll");
            _allDisableItem.Text = L.T("Menu.DisableAll");
            _individualItem.Text = L.T("Menu.Individual");
            _refreshItem.Text = L.T("Menu.Refresh");
            _elevateItem.Text = L.T("Menu.Elevate");
            _settingsItem.Text = L.T("Menu.Settings");
            _exitItem.Text = L.T("Menu.Exit");
            RefreshStatus();
        }

        private void RunAction(string title, OperationResult result)
        {
            if (result == null)
            {
                return;
            }

            Logger.WriteFormat("{0}: {1} - {2}", title, result.Success ? "OK" : "FAILED", result.Message);
            RefreshStatus();

            if (result.Success)
            {
                ShowBalloon(result.Message, ToolTipIcon.Info);
            }
            else
            {
                MessageBox.Show(result.Message, "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowBalloon(string message, ToolTipIcon icon)
        {
            if (_notifyIcon == null || string.IsNullOrEmpty(message))
            {
                return;
            }
            string text = message.Replace("\r", " ").Replace("\n", " ");
            if (text.Length > 250)
            {
                text = text.Substring(0, 250) + "...";
            }
            try
            {
                _notifyIcon.ShowBalloonTip(3000, "WinAdapterSwitcher", text, icon);
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Balloon tip failed: {0}", ex.Message);
            }
        }

        private AdapterInfo FindConfigured(string name)
        {
            return AdapterService.FindByName(_adapters, name);
        }

        private static bool? StatusOf(AdapterInfo adapter, string configuredName)
        {
            if (string.IsNullOrEmpty(configuredName))
            {
                return null;
            }
            if (adapter == null || !adapter.AdminKnown)
            {
                return null;
            }
            return adapter.Enabled;
        }

        private static string ShortStatus(string configuredName, AdapterInfo adapter)
        {
            if (string.IsNullOrEmpty(configuredName))
            {
                return L.T("Status.NotConfigured");
            }
            if (adapter == null)
            {
                return L.F("Status.NotFound", configuredName);
            }
            return ShortAdapterStatus(adapter);
        }

        private static string ShortAdapterStatus(AdapterInfo adapter)
        {
            if (!adapter.AdminKnown)
            {
                return L.T("Status.Unknown");
            }
            return adapter.Enabled ? L.T("Status.Enabled") : L.T("Status.Disabled");
        }

        private Icon GetStateIcon(bool? wiredEnabled, bool? wirelessEnabled)
        {
            string key = SlotKey(wiredEnabled) + "/" + SlotKey(wirelessEnabled);
            Icon icon;
            if (!_iconCache.TryGetValue(key, out icon))
            {
                icon = TrayIcons.Create(wiredEnabled, wirelessEnabled);
                _iconCache[key] = icon;
            }
            return icon;
        }

        private static char SlotKey(bool? value)
        {
            if (!value.HasValue)
            {
                return 'u';
            }
            return value.Value ? '1' : '0';
        }

        private void HandleIpcCommand(string[] args)
        {
            ParsedCommand command = CommandLine.Parse(args);

            if (args == null || args.Length == 0 || command.Kind == ParsedCommand.Menu)
            {
                RefreshStatus();
                _menu.Show(Cursor.Position);
                return;
            }
            if (command.Kind == ParsedCommand.Settings)
            {
                OpenSettings();
                return;
            }
            if (command.Kind == ParsedCommand.Help)
            {
                MessageBox.Show(CommandLine.Usage, "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (command.IsAction)
            {
                OperationResult result = CommandLine.ExecuteAction(_engine, command);
                RunAction(L.T("Action.CommandLine"), result);
                return;
            }

            MessageBox.Show(L.F("Error.UnknownArgument", command.Argument, CommandLine.Usage),
                "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ExitApplication()
        {
            if (_config.CleanAutoStartOnExit)
            {
                try
                {
                    AutoStart.SetEnabled(false);
                }
                catch (Exception ex)
                {
                    Logger.WriteFormat("Cleaning auto-start on exit failed: {0}", ex.Message);
                }
            }
            Logger.Write("Exit requested");
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                    _timer = null;
                }
                if (_ipc != null)
                {
                    _ipc.Dispose();
                    _ipc = null;
                }
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }
                if (_menu != null)
                {
                    _menu.Dispose();
                    _menu = null;
                }
                foreach (Icon icon in _iconCache.Values)
                {
                    icon.Dispose();
                }
                _iconCache.Clear();
                _currentIcon = null;
                if (_dispatcher != null)
                {
                    _dispatcher.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
