using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace WinAdapterSwitcher
{
    internal static class AutoStart
    {
        private const string TaskName = "WinAdapterSwitcher";
        private const string TaskNamespace = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        private const string LegacyRunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string LegacyValueName = "WinAdapterSwitcher";

        public static bool IsEnabled()
        {
            if (TaskExists())
            {
                return IsTaskEnabled();
            }
            return LegacyRunExists();
        }

        public static void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                CreateTask();
                DeleteLegacyRun();
            }
            else
            {
                DeleteTask();
                DeleteLegacyRun();
            }
            Logger.WriteFormat("Auto-start set to {0}", enabled ? "on" : "off");
        }

        public static void MigrateLegacy()
        {
            if (!LegacyRunExists())
            {
                return;
            }

            try
            {
                if (!TaskExists())
                {
                    CreateTask();
                }
                DeleteLegacyRun();
                Logger.Write("Migrated legacy auto-start Run entry to scheduled task");
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Auto-start migration failed: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Returns the target path when the auto-start task exists but points to a missing file.
        /// Returns null when the task is absent, valid, or cannot be inspected.
        /// </summary>
        public static string GetInvalidTargetPath()
        {
            if (TaskExists())
            {
                string taskPath = GetTaskCommandPath();
                if (!string.IsNullOrEmpty(taskPath) && !File.Exists(taskPath))
                {
                    return taskPath;
                }
                return null;
            }

            string command = GetLegacyRunCommand();
            if (string.IsNullOrEmpty(command))
            {
                return null;
            }

            string path = ExtractPath(command);
            if (path.Length == 0)
            {
                return command;
            }
            return File.Exists(path) ? null : path;
        }

        private static void CreateTask()
        {
            if (!SecurityUtil.IsAdministrator())
            {
                throw new InvalidOperationException(L.T("Error.AutoStartTask"));
            }

            string user = WindowsIdentity.GetCurrent().Name;
            string xml = BuildTaskXml(user);
            string tempFile = Path.Combine(Path.GetTempPath(),
                "WinAdapterSwitcher-" + Guid.NewGuid().ToString("N") + ".xml");

            try
            {
                File.WriteAllText(tempFile, xml, Encoding.Unicode);

                string output;
                int exitCode = RunSchtasks(
                    "/Create /TN \"" + TaskName + "\" /XML \"" + tempFile + "\" /F",
                    out output);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException(
                        string.IsNullOrEmpty(output) ? L.T("Error.AutoStartTask") : output);
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFormat("Delete temporary task file failed: {0}", ex.Message);
                }
            }
        }

        private static void DeleteTask()
        {
            if (!TaskExists())
            {
                return;
            }

            string output;
            int exitCode = RunSchtasks("/Delete /TN \"" + TaskName + "\" /F", out output);
            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    string.IsNullOrEmpty(output) ? L.T("Error.AutoStartTask") : output);
            }
        }

        private static bool TaskExists()
        {
            try
            {
                string output;
                return RunSchtasks("/Query /TN \"" + TaskName + "\"", out output) == 0;
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Query auto-start task failed: {0}", ex.Message);
                return false;
            }
        }

        private static bool IsTaskEnabled()
        {
            try
            {
                XmlDocument document = LoadTaskDocument();
                if (document == null)
                {
                    return true;
                }

                XmlNamespaceManager namespaces = CreateNamespaceManager(document);
                XmlNode settingsEnabled = document.SelectSingleNode(
                    "/t:Task/t:Settings/t:Enabled", namespaces);
                if (settingsEnabled != null && !IsTrue(settingsEnabled.InnerText))
                {
                    return false;
                }

                XmlNode triggerEnabled = document.SelectSingleNode(
                    "/t:Task/t:Triggers/t:LogonTrigger/t:Enabled", namespaces);
                if (triggerEnabled != null && !IsTrue(triggerEnabled.InnerText))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Read auto-start task state failed: {0}", ex.Message);
                return true;
            }
        }

        private static string GetTaskCommandPath()
        {
            try
            {
                XmlDocument document = LoadTaskDocument();
                if (document == null)
                {
                    return null;
                }

                XmlNamespaceManager namespaces = CreateNamespaceManager(document);
                XmlNode commandNode = document.SelectSingleNode(
                    "/t:Task/t:Actions/t:Exec/t:Command", namespaces);
                if (commandNode == null)
                {
                    return null;
                }
                return ExtractPath(commandNode.InnerText);
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Read auto-start task path failed: {0}", ex.Message);
                return null;
            }
        }

        private static XmlDocument LoadTaskDocument()
        {
            string taskFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "System32", "Tasks", TaskName);
            if (!File.Exists(taskFile))
            {
                return null;
            }

            XmlDocument document = new XmlDocument();
            document.Load(taskFile);
            return document;
        }

        private static XmlNamespaceManager CreateNamespaceManager(XmlDocument document)
        {
            XmlNamespaceManager namespaces = new XmlNamespaceManager(document.NameTable);
            namespaces.AddNamespace("t", TaskNamespace);
            return namespaces;
        }

        private static bool IsTrue(string text)
        {
            return string.Equals(text.Trim(), "true", StringComparison.OrdinalIgnoreCase);
        }

        private static int RunSchtasks(string arguments, out string output)
        {
            ProcessStartInfo info = new ProcessStartInfo("schtasks.exe", arguments);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            using (Process process = Process.Start(info))
            {
                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();
                process.WaitForExit();
                output = (standardOutput + "\r\n" + standardError).Trim();
                return process.ExitCode;
            }
        }

        private static string BuildTaskXml(string user)
        {
            string escapedUser = EscapeXml(user);
            string escapedPath = EscapeXml(AppPaths.ExecutablePath);
            StringBuilder xml = new StringBuilder();

            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-16\"?>");
            xml.AppendLine("<Task version=\"1.2\" xmlns=\"" + TaskNamespace + "\">");
            xml.AppendLine("  <RegistrationInfo>");
            xml.AppendLine("    <Author>" + escapedUser + "</Author>");
            xml.AppendLine("    <Description>WinAdapterSwitcher</Description>");
            xml.AppendLine("  </RegistrationInfo>");
            xml.AppendLine("  <Triggers>");
            xml.AppendLine("    <LogonTrigger>");
            xml.AppendLine("      <Enabled>true</Enabled>");
            xml.AppendLine("      <UserId>" + escapedUser + "</UserId>");
            xml.AppendLine("      <Delay>PT10S</Delay>");
            xml.AppendLine("    </LogonTrigger>");
            xml.AppendLine("  </Triggers>");
            xml.AppendLine("  <Principals>");
            xml.AppendLine("    <Principal id=\"Author\">");
            xml.AppendLine("      <UserId>" + escapedUser + "</UserId>");
            xml.AppendLine("      <LogonType>InteractiveToken</LogonType>");
            xml.AppendLine("      <RunLevel>HighestAvailable</RunLevel>");
            xml.AppendLine("    </Principal>");
            xml.AppendLine("  </Principals>");
            xml.AppendLine("  <Settings>");
            xml.AppendLine("    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>");
            xml.AppendLine("    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>");
            xml.AppendLine("    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>");
            xml.AppendLine("    <AllowHardTerminate>true</AllowHardTerminate>");
            xml.AppendLine("    <StartWhenAvailable>true</StartWhenAvailable>");
            xml.AppendLine("    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>");
            xml.AppendLine("    <IdleSettings>");
            xml.AppendLine("      <StopOnIdleEnd>false</StopOnIdleEnd>");
            xml.AppendLine("      <RestartOnIdle>false</RestartOnIdle>");
            xml.AppendLine("    </IdleSettings>");
            xml.AppendLine("    <AllowStartOnDemand>true</AllowStartOnDemand>");
            xml.AppendLine("    <Enabled>true</Enabled>");
            xml.AppendLine("    <Hidden>false</Hidden>");
            xml.AppendLine("    <RunOnlyIfIdle>false</RunOnlyIfIdle>");
            xml.AppendLine("    <WakeToRun>false</WakeToRun>");
            xml.AppendLine("    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>");
            xml.AppendLine("    <Priority>7</Priority>");
            xml.AppendLine("  </Settings>");
            xml.AppendLine("  <Actions Context=\"Author\">");
            xml.AppendLine("    <Exec>");
            xml.AppendLine("      <Command>" + escapedPath + "</Command>");
            xml.AppendLine("    </Exec>");
            xml.AppendLine("  </Actions>");
            xml.AppendLine("</Task>");
            return xml.ToString();
        }

        private static string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private static bool LegacyRunExists()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(LegacyRunKeyPath, false))
                {
                    return key != null && key.GetValue(LegacyValueName) != null;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Read legacy auto-start failed: {0}", ex.Message);
                return false;
            }
        }

        private static string GetLegacyRunCommand()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(LegacyRunKeyPath, false))
                {
                    if (key == null)
                    {
                        return null;
                    }
                    return key.GetValue(LegacyValueName) as string;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Read legacy auto-start path failed: {0}", ex.Message);
                return null;
            }
        }

        private static void DeleteLegacyRun()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(LegacyRunKeyPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(LegacyValueName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Delete legacy auto-start failed: {0}", ex.Message);
            }
        }

        private static string ExtractPath(string command)
        {
            string text = command.Trim();
            if (text.StartsWith("\"", StringComparison.Ordinal))
            {
                int end = text.IndexOf('"', 1);
                if (end > 1)
                {
                    return text.Substring(1, end - 1);
                }
                return text.Trim('"');
            }

            int exeIndex = text.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex >= 0)
            {
                return text.Substring(0, exeIndex + 4);
            }
            return text;
        }
    }
}
