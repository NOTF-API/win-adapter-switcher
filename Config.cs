using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WinAdapterSwitcher
{
    /// <summary>
    /// Lightweight INI based configuration stored next to the executable.
    /// </summary>
    internal sealed class Config
    {
        public string WiredAdapter = "";
        public string WiredDescription = "";
        public string WirelessAdapter = "";
        public string WirelessDescription = "";
        public string Language = L.Auto;
        public bool ConfirmDisableAll = true;
        public bool LogEnabled = false;
        public bool CleanAutoStartOnExit = false;

        public Config Clone()
        {
            Config copy = new Config();
            copy.WiredAdapter = WiredAdapter;
            copy.WiredDescription = WiredDescription;
            copy.WirelessAdapter = WirelessAdapter;
            copy.WirelessDescription = WirelessDescription;
            copy.Language = Language;
            copy.ConfirmDisableAll = ConfirmDisableAll;
            copy.LogEnabled = LogEnabled;
            copy.CleanAutoStartOnExit = CleanAutoStartOnExit;
            return copy;
        }

        public void CopyFrom(Config other)
        {
            if (other == null)
            {
                return;
            }
            WiredAdapter = other.WiredAdapter;
            WiredDescription = other.WiredDescription;
            WirelessAdapter = other.WirelessAdapter;
            WirelessDescription = other.WirelessDescription;
            Language = other.Language;
            ConfirmDisableAll = other.ConfirmDisableAll;
            LogEnabled = other.LogEnabled;
            CleanAutoStartOnExit = other.CleanAutoStartOnExit;
        }

        public static Config Load(string path)
        {
            Config config = new Config();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return config;
            }

            Dictionary<string, string> values = ReadIni(path);
            config.WiredAdapter = Get(values, "Adapters", "Wired", "");
            config.WiredDescription = Get(values, "Adapters", "WiredDescription", "");
            config.WirelessAdapter = Get(values, "Adapters", "Wireless", "");
            config.WirelessDescription = Get(values, "Adapters", "WirelessDescription", "");
            config.Language = L.Normalize(Get(values, "Options", "Language", L.Auto));
            config.ConfirmDisableAll = GetBool(values, "Options", "ConfirmDisableAll", true);
            config.LogEnabled = GetBool(values, "Options", "EnableLog", false);
            config.CleanAutoStartOnExit = GetBool(values, "Options", "CleanAutoStartOnExit", false);
            return config;
        }

        public void Save(string path)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("; WinAdapterSwitcher configuration");
            sb.AppendLine("; This file is stored next to WinAdapterSwitcher.exe.");
            sb.AppendLine();
            sb.AppendLine("[Adapters]");
            sb.AppendLine("Wired=" + WiredAdapter);
            sb.AppendLine("WiredDescription=" + WiredDescription);
            sb.AppendLine("Wireless=" + WirelessAdapter);
            sb.AppendLine("WirelessDescription=" + WirelessDescription);
            sb.AppendLine();
            sb.AppendLine("[Options]");
            sb.AppendLine("Language=" + L.Normalize(Language));
            sb.AppendLine("ConfirmDisableAll=" + BoolText(ConfirmDisableAll));
            sb.AppendLine("EnableLog=" + BoolText(LogEnabled));
            sb.AppendLine("CleanAutoStartOnExit=" + BoolText(CleanAutoStartOnExit));

            UTF8Encoding encoding = new UTF8Encoding(false);
            File.WriteAllText(path, sb.ToString(), encoding);
        }

        private static string BoolText(bool value)
        {
            return value ? "1" : "0";
        }

        private static Dictionary<string, string> ReadIni(string path)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string section = "";
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                {
                    continue;
                }
                if (line.StartsWith("[") && line.EndsWith("]") && line.Length > 2)
                {
                    section = line.Substring(1, line.Length - 2).Trim();
                    continue;
                }
                int equals = line.IndexOf('=');
                if (equals <= 0)
                {
                    continue;
                }
                string key = line.Substring(0, equals).Trim();
                string value = line.Substring(equals + 1).Trim();
                values[section + "/" + key] = value;
            }
            return values;
        }

        private static string Get(Dictionary<string, string> values, string section, string key, string defaultValue)
        {
            string value;
            if (values.TryGetValue(section + "/" + key, out value))
            {
                return value;
            }
            return defaultValue;
        }

        private static bool GetBool(Dictionary<string, string> values, string section, string key, bool defaultValue)
        {
            string value;
            if (!values.TryGetValue(section + "/" + key, out value))
            {
                return defaultValue;
            }
            value = value.Trim();
            if (value.Length == 0)
            {
                return defaultValue;
            }
            if (value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "on", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (value == "0" || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "no", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "off", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return defaultValue;
        }
    }
}
