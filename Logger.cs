using System;
using System.IO;
using System.Text;

namespace WinAdapterSwitcher
{
    internal static class Logger
    {
        private static readonly object SyncRoot = new object();
        private static string _path = "";
        private static bool _enabled = false;

        public static bool Enabled
        {
            get { return _enabled; }
        }

        public static void Init(string path, bool enabled)
        {
            lock (SyncRoot)
            {
                _path = path;
                _enabled = enabled;
            }
            if (enabled)
            {
                Write("---- WinAdapterSwitcher started ----");
            }
        }

        public static void SetEnabled(bool enabled)
        {
            lock (SyncRoot)
            {
                _enabled = enabled;
            }
            if (enabled)
            {
                Write("Logging enabled");
            }
        }

        public static void Write(string message)
        {
            lock (SyncRoot)
            {
                if (!_enabled || string.IsNullOrEmpty(_path))
                {
                    return;
                }
                try
                {
                    string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + Environment.NewLine;
                    File.AppendAllText(_path, line, new UTF8Encoding(false));
                }
                catch (Exception)
                {
                    // Logging must never break the application.
                }
            }
        }

        public static void WriteFormat(string format, params object[] args)
        {
            if (!_enabled)
            {
                return;
            }
            try
            {
                Write(string.Format(format, args));
            }
            catch (Exception)
            {
                Write(format);
            }
        }
    }
}
