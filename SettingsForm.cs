using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinAdapterSwitcher
{
    internal sealed class SettingsForm : Form
    {
        private readonly Config _working;
        private List<AdapterInfo> _adapters;

        private ComboBox _wiredCombo;
        private ComboBox _wirelessCombo;
        private ComboBox _languageCombo;
        private Button _refreshButton;
        private CheckBox _confirmCheck;
        private CheckBox _logCheck;
        private CheckBox _cleanAutoStartCheck;
        private CheckBox _autoStartCheck;
        private Label _adminLabel;
        private Button _restartButton;
        private Button _saveButton;
        private Button _cancelButton;

        public Config SavedConfig
        {
            get { return _working; }
        }

        public SettingsForm(Config config, List<AdapterInfo> adapters)
        {
            _working = config.Clone();
            _adapters = adapters == null ? new List<AdapterInfo>() : adapters;

            Text = L.T("Settings.Title");
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = new Font("Microsoft YaHei UI", 9f);
            AutoScaleDimensions = new SizeF(96f, 96f);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            BuildLayout();
            PopulateAdapters();
            LoadSelections();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            TableLayoutPanel root = new TableLayoutPanel();
            root.AutoSize = true;
            root.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            root.Padding = new Padding(16);
            root.MinimumSize = new Size(600, 0);
            root.ColumnCount = 1;
            root.RowCount = 10;
            root.Location = new Point(0, 0);
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 10; i++)
            {
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            Label wiredLabel = new Label();
            wiredLabel.Text = L.T("Settings.WiredLabel");
            wiredLabel.AutoSize = true;
            wiredLabel.Margin = new Padding(0, 4, 0, 4);
            root.Controls.Add(wiredLabel, 0, 0);

            _wiredCombo = new ComboBox();
            _wiredCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            _wiredCombo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _wiredCombo.MinimumSize = new Size(560, 0);
            _wiredCombo.DropDownWidth = 620;
            _wiredCombo.Margin = new Padding(0, 0, 0, 10);
            root.Controls.Add(_wiredCombo, 0, 1);

            Label wirelessLabel = new Label();
            wirelessLabel.Text = L.T("Settings.WirelessLabel");
            wirelessLabel.AutoSize = true;
            wirelessLabel.Margin = new Padding(0, 4, 0, 4);
            root.Controls.Add(wirelessLabel, 0, 2);

            _wirelessCombo = new ComboBox();
            _wirelessCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            _wirelessCombo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _wirelessCombo.MinimumSize = new Size(560, 0);
            _wirelessCombo.DropDownWidth = 620;
            _wirelessCombo.Margin = new Padding(0, 0, 0, 10);
            root.Controls.Add(_wirelessCombo, 0, 3);

            Label languageLabel = new Label();
            languageLabel.Text = L.T("Settings.LanguageLabel");
            languageLabel.AutoSize = true;
            languageLabel.Margin = new Padding(0, 4, 0, 4);
            root.Controls.Add(languageLabel, 0, 4);

            _languageCombo = new ComboBox();
            _languageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            _languageCombo.Anchor = AnchorStyles.Left;
            _languageCombo.Width = 280;
            _languageCombo.Margin = new Padding(0, 0, 0, 10);
            _languageCombo.Items.Add(new LanguageChoice(L.Auto, L.T("Settings.LanguageAuto")));
            _languageCombo.Items.Add(new LanguageChoice(L.ChineseSimplified, "简体中文"));
            _languageCombo.Items.Add(new LanguageChoice(L.ChineseTraditional, "繁體中文"));
            _languageCombo.Items.Add(new LanguageChoice(L.English, "English"));
            _languageCombo.Items.Add(new LanguageChoice(L.Japanese, "日本語"));
            _languageCombo.Items.Add(new LanguageChoice(L.Korean, "한국어"));
            _languageCombo.Items.Add(new LanguageChoice(L.German, "Deutsch"));
            _languageCombo.Items.Add(new LanguageChoice(L.French, "Français"));
            _languageCombo.Items.Add(new LanguageChoice(L.Spanish, "Español"));
            root.Controls.Add(_languageCombo, 0, 5);

            FlowLayoutPanel refreshPanel = new FlowLayoutPanel();
            refreshPanel.AutoSize = true;
            refreshPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            refreshPanel.FlowDirection = FlowDirection.LeftToRight;
            refreshPanel.WrapContents = false;
            refreshPanel.Margin = new Padding(0, 0, 0, 8);

            _refreshButton = new Button();
            _refreshButton.Text = L.T("Settings.Refresh");
            _refreshButton.AutoSize = true;
            _refreshButton.Margin = new Padding(0, 2, 12, 2);
            _refreshButton.Click += RefreshButtonClick;
            refreshPanel.Controls.Add(_refreshButton);

            Label hint = new Label();
            hint.Text = L.T("Settings.Hint");
            hint.AutoSize = true;
            hint.ForeColor = Color.Gray;
            hint.Margin = new Padding(0, 8, 0, 0);
            refreshPanel.Controls.Add(hint);
            root.Controls.Add(refreshPanel, 0, 6);

            Panel optionsPanel = new Panel();
            optionsPanel.BorderStyle = BorderStyle.FixedSingle;
            optionsPanel.AutoSize = true;
            optionsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            optionsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            optionsPanel.Margin = new Padding(0, 0, 0, 8);

            FlowLayoutPanel options = new FlowLayoutPanel();
            options.AutoSize = true;
            options.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            options.FlowDirection = FlowDirection.TopDown;
            options.WrapContents = false;
            options.Padding = new Padding(10, 8, 10, 8);
            options.Location = new Point(0, 0);

            Label optionsLabel = new Label();
            optionsLabel.Text = L.T("Settings.Options");
            optionsLabel.AutoSize = true;
            optionsLabel.Font = new Font(Font, FontStyle.Bold);
            optionsLabel.Margin = new Padding(0, 0, 0, 8);
            options.Controls.Add(optionsLabel);

            _confirmCheck = new CheckBox();
            _confirmCheck.Text = L.T("Settings.ConfirmDisableAll");
            _confirmCheck.AutoSize = true;
            _confirmCheck.Margin = new Padding(0, 2, 0, 6);
            options.Controls.Add(_confirmCheck);

            _logCheck = new CheckBox();
            _logCheck.Text = L.T("Settings.EnableLog");
            _logCheck.AutoSize = true;
            _logCheck.Margin = new Padding(0, 2, 0, 6);
            options.Controls.Add(_logCheck);

            _cleanAutoStartCheck = new CheckBox();
            _cleanAutoStartCheck.Text = L.T("Settings.CleanAutoStart");
            _cleanAutoStartCheck.AutoSize = true;
            _cleanAutoStartCheck.Margin = new Padding(0, 2, 0, 6);
            options.Controls.Add(_cleanAutoStartCheck);

            _autoStartCheck = new CheckBox();
            _autoStartCheck.Text = L.T("Settings.AutoStart");
            _autoStartCheck.AutoSize = true;
            _autoStartCheck.Margin = new Padding(0, 2, 0, 2);
            options.Controls.Add(_autoStartCheck);

            optionsPanel.Controls.Add(options);
            root.Controls.Add(optionsPanel, 0, 7);

            TableLayoutPanel adminPanel = new TableLayoutPanel();
            adminPanel.AutoSize = true;
            adminPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            adminPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            adminPanel.ColumnCount = 2;
            adminPanel.RowCount = 1;
            adminPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            adminPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            adminPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            adminPanel.Margin = new Padding(0, 4, 0, 4);

            _adminLabel = new Label();
            _adminLabel.AutoSize = true;
            _adminLabel.Anchor = AnchorStyles.Left;
            _adminLabel.Margin = new Padding(0, 6, 8, 0);
            adminPanel.Controls.Add(_adminLabel, 0, 0);

            _restartButton = new Button();
            _restartButton.Text = L.T("Settings.RestartElevated");
            _restartButton.AutoSize = true;
            _restartButton.Anchor = AnchorStyles.Right;
            _restartButton.Margin = new Padding(8, 0, 0, 0);
            _restartButton.Click += RestartButtonClick;
            adminPanel.Controls.Add(_restartButton, 1, 0);
            root.Controls.Add(adminPanel, 0, 8);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.AutoSize = true;
            buttons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttons.Anchor = AnchorStyles.Right;
            buttons.FlowDirection = FlowDirection.RightToLeft;
            buttons.WrapContents = false;
            buttons.Margin = new Padding(0, 8, 0, 0);

            _saveButton = new Button();
            _saveButton.Text = L.T("Settings.Save");
            _saveButton.AutoSize = true;
            _saveButton.Margin = new Padding(8, 2, 0, 2);
            _saveButton.Click += SaveButtonClick;
            buttons.Controls.Add(_saveButton);

            _cancelButton = new Button();
            _cancelButton.Text = L.T("Settings.Cancel");
            _cancelButton.AutoSize = true;
            _cancelButton.DialogResult = DialogResult.Cancel;
            _cancelButton.Margin = new Padding(8, 2, 0, 2);
            buttons.Controls.Add(_cancelButton);

            root.Controls.Add(buttons, 0, 9);
            Controls.Add(root);

            AcceptButton = _saveButton;
            CancelButton = _cancelButton;

            bool admin = SecurityUtil.IsAdministrator();
            _adminLabel.Text = admin
                ? L.T("Settings.AdminYes")
                : L.T("Settings.AdminNo");
            _adminLabel.ForeColor = admin ? Color.ForestGreen : Color.Firebrick;
            _restartButton.Visible = !admin;

            ResumeLayout(true);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Rectangle workingArea = Screen.FromControl(this).WorkingArea;
            int maxWidth = Math.Max(320, workingArea.Width - 24);
            int maxHeight = Math.Max(240, workingArea.Height - 24);
            int width = Math.Min(Width, maxWidth);
            int height = Math.Min(Height, maxHeight);

            if (width < Width || height < Height)
            {
                AutoSize = false;
                AutoScroll = true;
                Size = new Size(width, height);
                Location = new Point(
                    workingArea.Left + (workingArea.Width - Width) / 2,
                    workingArea.Top + (workingArea.Height - Height) / 2);
            }
        }

        private void PopulateAdapters()
        {
            string wiredName = GetSelectedName(_wiredCombo);
            string wirelessName = GetSelectedName(_wirelessCombo);

            _wiredCombo.Items.Clear();
            _wirelessCombo.Items.Clear();

            string noneText = L.T("Settings.Unconfigured");
            AdapterChoice none = new AdapterChoice("", "", noneText);
            _wiredCombo.Items.Add(none);
            _wirelessCombo.Items.Add(new AdapterChoice("", "", noneText));

            for (int i = 0; i < _adapters.Count; i++)
            {
                AdapterInfo adapter = _adapters[i];
                string text = adapter.Name;
                if (!string.IsNullOrEmpty(adapter.Description))
                {
                    text = text + " - " + adapter.Description;
                }
                text = text + "  [" + adapter.StatusText + "]";
                AdapterChoice choice = new AdapterChoice(adapter.Name, adapter.Description, text);
                _wiredCombo.Items.Add(choice);
                _wirelessCombo.Items.Add(new AdapterChoice(adapter.Name, adapter.Description, text));
            }

            SelectChoice(_wiredCombo, wiredName);
            SelectChoice(_wirelessCombo, wirelessName);
        }

        private void LoadSelections()
        {
            SelectChoice(_wiredCombo, _working.WiredAdapter);
            SelectChoice(_wirelessCombo, _working.WirelessAdapter);
            SelectLanguage(_working.Language);

            if (GetSelectedChoice(_wiredCombo).Name.Length == 0)
            {
                SelectFirstOfType(_wiredCombo, AdapterInfo.TypeEthernet);
            }
            if (GetSelectedChoice(_wirelessCombo).Name.Length == 0)
            {
                SelectFirstOfType(_wirelessCombo, AdapterInfo.TypeWireless);
            }

            _confirmCheck.Checked = _working.ConfirmDisableAll;
            _logCheck.Checked = _working.LogEnabled;
            _cleanAutoStartCheck.Checked = _working.CleanAutoStartOnExit;
            _autoStartCheck.Checked = AutoStart.IsEnabled();
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            try
            {
                _adapters = AdapterService.GetAdapters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, L.F("Settings.ReadAdaptersFailed", ex.Message), "WinAdapterSwitcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            PopulateAdapters();
            if (GetSelectedChoice(_wiredCombo).Name.Length == 0)
            {
                SelectFirstOfType(_wiredCombo, AdapterInfo.TypeEthernet);
            }
            if (GetSelectedChoice(_wirelessCombo).Name.Length == 0)
            {
                SelectFirstOfType(_wirelessCombo, AdapterInfo.TypeWireless);
            }
        }

        private void RestartButtonClick(object sender, EventArgs e)
        {
            if (SecurityUtil.RestartElevated())
            {
                Application.Exit();
            }
            else
            {
                MessageBox.Show(this, L.T("Settings.RestartFailed"),
                    "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            AdapterChoice wired = GetSelectedChoice(_wiredCombo);
            AdapterChoice wireless = GetSelectedChoice(_wirelessCombo);
            if (wired.Name.Length > 0 && wireless.Name.Length > 0 &&
                string.Equals(wired.Name, wireless.Name, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, L.T("Settings.DuplicateAdapter"),
                    "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _working.WiredAdapter = wired.Name;
            _working.WiredDescription = wired.Description;
            _working.WirelessAdapter = wireless.Name;
            _working.WirelessDescription = wireless.Description;
            _working.Language = GetSelectedLanguage();
            _working.ConfirmDisableAll = _confirmCheck.Checked;
            _working.LogEnabled = _logCheck.Checked;
            _working.CleanAutoStartOnExit = _cleanAutoStartCheck.Checked;

            try
            {
                _working.Save(AppPaths.ConfigFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, L.F("Settings.SaveFailed", ex.Message),
                    "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                AutoStart.SetEnabled(_autoStartCheck.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, L.F("Settings.AutoStartFailed", ex.Message),
                    "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Logger.SetEnabled(_working.LogEnabled);
            Logger.Write("Settings saved");
            DialogResult = DialogResult.OK;
            Close();
        }

        private static string GetSelectedName(ComboBox combo)
        {
            if (combo == null || combo.SelectedItem == null)
            {
                return "";
            }
            AdapterChoice choice = combo.SelectedItem as AdapterChoice;
            return choice == null ? "" : choice.Name;
        }

        private static AdapterChoice GetSelectedChoice(ComboBox combo)
        {
            AdapterChoice choice = combo.SelectedItem as AdapterChoice;
            if (choice == null)
            {
                return new AdapterChoice("", "", L.T("Settings.Unconfigured"));
            }
            return choice;
        }

        private void SelectLanguage(string setting)
        {
            string normalized = L.Normalize(setting);
            for (int i = 0; i < _languageCombo.Items.Count; i++)
            {
                LanguageChoice choice = _languageCombo.Items[i] as LanguageChoice;
                if (choice != null && choice.Setting == normalized)
                {
                    _languageCombo.SelectedIndex = i;
                    return;
                }
            }
            _languageCombo.SelectedIndex = 0;
        }

        private string GetSelectedLanguage()
        {
            LanguageChoice choice = _languageCombo.SelectedItem as LanguageChoice;
            return choice == null ? L.Auto : choice.Setting;
        }

        private static void SelectChoice(ComboBox combo, string name)
        {
            if (combo == null)
            {
                return;
            }
            int index = 0;
            if (!string.IsNullOrEmpty(name))
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    AdapterChoice choice = combo.Items[i] as AdapterChoice;
                    if (choice != null && string.Equals(choice.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }
            combo.SelectedIndex = index;
        }

        private void SelectFirstOfType(ComboBox combo, int interfaceType)
        {
            for (int i = 0; i < _adapters.Count; i++)
            {
                if (_adapters[i].InterfaceType != interfaceType)
                {
                    continue;
                }
                for (int j = 0; j < combo.Items.Count; j++)
                {
                    AdapterChoice choice = combo.Items[j] as AdapterChoice;
                    if (choice != null && string.Equals(choice.Name, _adapters[i].Name, StringComparison.OrdinalIgnoreCase))
                    {
                        combo.SelectedIndex = j;
                        return;
                    }
                }
            }
        }

        private sealed class LanguageChoice
        {
            public readonly string Setting;
            private readonly string _text;

            public LanguageChoice(string setting, string text)
            {
                Setting = setting;
                _text = text;
            }

            public override string ToString()
            {
                return _text;
            }
        }

        private sealed class AdapterChoice
        {
            public readonly string Name;
            public readonly string Description;
            private readonly string _text;

            public AdapterChoice(string name, string description, string text)
            {
                Name = name;
                Description = description;
                _text = text;
            }

            public override string ToString()
            {
                return _text;
            }
        }
    }
}
