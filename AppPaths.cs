using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace WinAdapterSwitcher
{
    internal static class AppPaths
    {
        public static string BaseDirectory
        {
            get
            {
                string location = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(location))
                {
                    location = Application.ExecutablePath;
                }
                return Path.GetDirectoryName(location);
            }
        }

        public static string ExecutablePath
        {
            get
            {
                string location = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(location))
                {
                    location = Application.ExecutablePath;
                }
                return location;
            }
        }

        public static string ConfigFile
        {
            get { return Path.Combine(BaseDirectory, "config.ini"); }
        }

        public static string LogFile
        {
            get { return Path.Combine(BaseDirectory, "WinAdapterSwitcher.log"); }
        }
    }
}
