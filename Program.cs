using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace WinAdapterSwitcher
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += ApplicationThreadException;
            AppDomain.CurrentDomain.UnhandledException += DomainUnhandledException;

            bool restarting = false;
            List<string> argumentList = new List<string>();
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.Equals(args[i], "--restart", StringComparison.OrdinalIgnoreCase))
                    {
                        restarting = true;
                    }
                    else
                    {
                        argumentList.Add(args[i]);
                    }
                }
            }
            string[] effectiveArgs = argumentList.ToArray();
            if (restarting)
            {
                Thread.Sleep(1500);
            }

            bool createdNew;
            using (Mutex mutex = new Mutex(true, "WinAdapterSwitcher.SingleInstance.v1", out createdNew))
            {
                if (!createdNew)
                {
                    IpcClient.Send(effectiveArgs);
                    return;
                }

                Config config = new Config();
                try
                {
                    config = Config.Load(AppPaths.ConfigFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(L.F("Program.ConfigLoadFailed", ex.Message),
                        "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                L.SetLanguage(config.Language);
                Logger.Init(AppPaths.LogFile, config.LogEnabled);
                Logger.WriteFormat("Executable: {0}", AppPaths.ExecutablePath);

                ValidateAutoStart();
                AutoStart.MigrateLegacy();

                ParsedCommand command = CommandLine.Parse(effectiveArgs);
                if (command.Kind == ParsedCommand.Help)
                {
                    MessageBox.Show(CommandLine.Usage, "WinAdapterSwitcher",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!SecurityUtil.IsAdministrator())
                {
                    DialogResult answer = MessageBox.Show(
                        L.T("Program.AdminRestartPrompt"),
                        "WinAdapterSwitcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (answer == DialogResult.Yes)
                    {
                        if (SecurityUtil.RestartElevated())
                        {
                            return;
                        }
                        MessageBox.Show(L.T("Program.AdminRestartFailed"),
                            "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                if (command.IsAction)
                {
                    AdapterDetector.EnsureConfigured(config);
                    RunHeadless(config, command);
                    return;
                }

                if (command.Kind == ParsedCommand.Unknown)
                {
                    MessageBox.Show(L.F("Error.UnknownArgument", command.Argument, CommandLine.Usage),
                        "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using (TrayApplicationContext context = new TrayApplicationContext(config))
                    {
                        Application.Run(context);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFormat("Tray application failed: {0}", ex);
                    MessageBox.Show(L.F("Program.TrayStartFailed", ex.Message),
                        "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Logger.Write("Application stopped");
            }
        }

        private static void RunHeadless(Config config, ParsedCommand command)
        {
            SwitchEngine engine = new SwitchEngine(config);
            OperationResult result = CommandLine.ExecuteAction(engine, command);
            if (result == null)
            {
                return;
            }
            MessageBox.Show(result.Message, "WinAdapterSwitcher", MessageBoxButtons.OK,
                result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private static void ValidateAutoStart()
        {
            try
            {
                string invalidPath = AutoStart.GetInvalidTargetPath();
                if (string.IsNullOrEmpty(invalidPath))
                {
                    return;
                }

                AutoStart.SetEnabled(false);
                MessageBox.Show(
                    L.F("Program.AutoStartInvalid", invalidPath),
                    "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Auto-start validation failed: {0}", ex.Message);
            }
        }

        private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.WriteFormat("Unhandled UI exception: {0}", e.Exception);
            MessageBox.Show(L.F("Program.UnhandledError", e.Exception.Message),
                "WinAdapterSwitcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.WriteFormat("Unhandled exception: {0}", e.ExceptionObject);
        }
    }
}
