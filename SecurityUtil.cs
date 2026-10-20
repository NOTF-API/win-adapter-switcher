using System;
using System.Diagnostics;
using System.Security.Principal;

namespace WinAdapterSwitcher
{
    internal static class SecurityUtil
    {
        public static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool RestartElevated()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(AppPaths.ExecutablePath);
                info.UseShellExecute = true;
                info.Verb = "runas";
                info.WorkingDirectory = AppPaths.BaseDirectory;
                info.Arguments = "--restart";
                Process.Start(info);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Failed to restart elevated: {0}", ex.Message);
                return false;
            }
        }
    }
}
