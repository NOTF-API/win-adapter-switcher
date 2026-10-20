using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;

namespace WinAdapterSwitcher
{
    internal static class AdapterService
    {
        private const string WmiNamespace = "root\\StandardCimv2";
        private const string WmiQuery = "SELECT Name, InterfaceDescription, InterfaceGuid, InterfaceAdminStatus, " +
                                        "InterfaceOperationalStatus, InterfaceIndex, InterfaceType, Hidden FROM MSFT_NetAdapter";

        public static List<AdapterInfo> GetAdapters()
        {
            string wmiError = "";
            List<AdapterInfo> adapters = null;

            try
            {
                adapters = GetAdaptersFromWmi();
            }
            catch (Exception ex)
            {
                wmiError = ex.Message;
                Logger.WriteFormat("WMI adapter query failed: {0}", ex.Message);
            }

            if (adapters != null && adapters.Count > 0)
            {
                adapters.Sort(CompareAdapters);
                return adapters;
            }

            try
            {
                List<AdapterInfo> fallback = GetAdaptersFromNetsh();
                fallback.Sort(CompareAdapters);
                return fallback;
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("netsh adapter query failed: {0}", ex.Message);
                if (adapters != null)
                {
                    return adapters;
                }
                string message = L.F("Error.ReadAdapterList", ex.Message);
                if (wmiError.Length > 0)
                {
                    message = message + L.F("Error.WmiDetail", wmiError);
                }
                throw new InvalidOperationException(message);
            }
        }

        public static AdapterInfo FindByName(List<AdapterInfo> adapters, string name)
        {
            if (adapters == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            for (int i = 0; i < adapters.Count; i++)
            {
                if (string.Equals(adapters[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return adapters[i];
                }
            }
            return null;
        }

        public static OperationResult SetEnabled(string name, bool enable)
        {
            if (string.IsNullOrEmpty(name))
            {
                return OperationResult.Fail(L.T("Error.NoAdapterSelected"));
            }
            if (!SecurityUtil.IsAdministrator())
            {
                return OperationResult.Fail(L.T("Error.AdminRequired"));
            }

            bool exists = true;
            try
            {
                exists = FindByName(GetAdapters(), name) != null;
            }
            catch (Exception)
            {
            }
            if (!exists)
            {
                return OperationResult.Fail(L.F("Error.AdapterNotFound", name));
            }

            string action = enable ? "enable" : "disable";
            string arguments = "interface set interface name=\"" + name.Replace("\"", "") + "\" admin=" + action;
            Logger.WriteFormat("netsh {0}", arguments);

            ProcessRunResult run = ProcessRunner.Run("netsh.exe", arguments, 20000);
            if (!run.Started)
            {
                return OperationResult.Fail(L.F("Error.NetshStartFailed", CleanOutput(run.Output)));
            }
            if (run.TimedOut)
            {
                return OperationResult.Fail(L.T("Error.NetshTimeout"));
            }

            for (int attempt = 0; attempt < 17; attempt++)
            {
                try
                {
                    AdapterInfo current = FindByName(GetAdapters(), name);
                    if (current != null && current.AdminKnown && current.Enabled == enable)
                    {
                        string done = L.F(enable ? "Action.EnabledAdapter" : "Action.DisabledAdapter", name);
                        Logger.Write(done);
                        return OperationResult.Ok(done);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFormat("State verification failed: {0}", ex.Message);
                }
                Thread.Sleep(300);
            }

            string details = CleanOutput(run.Output);
            string failed = L.F(enable ? "Error.EnableFailed" : "Error.DisableFailed", name);
            if (run.ExitCode != 0)
            {
                failed = failed + L.F("Error.NetshExitCode", run.ExitCode.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                failed = failed + L.T("Error.NotEffective");
            }
            if (details.Length > 0)
            {
                failed = failed + L.F("Error.DetailSuffix", details);
            }
            Logger.Write(failed);
            return OperationResult.Fail(failed);
        }

        private static List<AdapterInfo> GetAdaptersFromWmi()
        {
            List<AdapterInfo> adapters = new List<AdapterInfo>();
            ManagementScope scope = new ManagementScope(WmiNamespace);
            ObjectQuery query = new ObjectQuery(WmiQuery);

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementObject item in results)
                {
                    using (item)
                    {
                        string name = Convert.ToString(GetProperty(item, "Name"), CultureInfo.InvariantCulture);
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        int interfaceType = ToInt(GetProperty(item, "InterfaceType"), -1);
                        if (interfaceType == AdapterInfo.TypeLoopback)
                        {
                            continue;
                        }

                        object hiddenValue = GetProperty(item, "Hidden");
                        bool hidden = false;
                        try
                        {
                            hidden = hiddenValue != null && Convert.ToBoolean(hiddenValue, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                        if (hidden)
                        {
                            continue;
                        }

                        int adminStatus = ToInt(GetProperty(item, "InterfaceAdminStatus"), -1);
                        int operationalStatus = ToInt(GetProperty(item, "InterfaceOperationalStatus"), -1);

                        AdapterInfo adapter = new AdapterInfo();
                        adapter.Name = name;
                        adapter.Description = Convert.ToString(GetProperty(item, "InterfaceDescription"), CultureInfo.InvariantCulture);
                        adapter.Id = Convert.ToString(GetProperty(item, "InterfaceGuid"), CultureInfo.InvariantCulture);
                        adapter.InterfaceIndex = ToInt(GetProperty(item, "InterfaceIndex"), 0);
                        adapter.InterfaceType = interfaceType;
                        adapter.AdminKnown = adminStatus >= 0;
                        adapter.Enabled = adminStatus == 1;
                        adapter.LinkUp = operationalStatus == 1;
                        if (adapter.AdminKnown)
                        {
                            adapter.RawState = adapter.Enabled ? (adapter.LinkUp ? "Up" : "Disconnected") : "Disabled";
                        }
                        else
                        {
                            adapter.RawState = "Unknown";
                        }
                        adapters.Add(adapter);
                    }
                }
            }

            return adapters;
        }

        private static List<AdapterInfo> GetAdaptersFromNetsh()
        {
            ProcessRunResult run = ProcessRunner.Run("netsh.exe", "interface show interface", 15000);
            if (!run.Started || run.TimedOut)
            {
                throw new InvalidOperationException(L.T("Error.NetshShowFailed"));
            }

            Dictionary<string, AdapterInfo> known = new Dictionary<string, AdapterInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    AdapterInfo info = new AdapterInfo();
                    info.Name = interfaces[i].Name;
                    info.Description = interfaces[i].Description;
                    info.Id = interfaces[i].Id;
                    info.InterfaceType = (int)interfaces[i].NetworkInterfaceType;
                    known[info.Name] = info;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("NetworkInterface enumeration failed: {0}", ex.Message);
            }

            List<AdapterInfo> adapters = new List<AdapterInfo>();
            string[] lines = run.Output.Replace("\r\n", "\n").Split('\n');
            int separator = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                if (IsSeparatorLine(lines[i]))
                {
                    separator = i;
                    break;
                }
            }

            if (separator < 0)
            {
                if (run.Output.Length == 0)
                {
                    throw new InvalidOperationException(L.T("Error.NetshNoList"));
                }
                return adapters;
            }

            for (int i = separator + 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Trim().Length == 0)
                {
                    continue;
                }

                string adminToken;
                string stateToken;
                string typeToken;
                string nameToken;
                if (!TryParseInterfaceLine(line, out adminToken, out stateToken, out typeToken, out nameToken))
                {
                    continue;
                }
                if (nameToken.IndexOf("Loopback", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    nameToken.IndexOf("环回", StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                AdapterInfo info;
                if (!known.TryGetValue(nameToken, out info))
                {
                    info = new AdapterInfo();
                    info.Name = nameToken;
                }

                bool adminKnown = false;
                bool enabled = ParseEnabledToken(adminToken, out adminKnown);
                info.AdminKnown = adminKnown;
                info.Enabled = enabled;
                info.LinkUp = ParseConnectedToken(stateToken);
                info.RawState = stateToken;
                if (info.InterfaceType == 0)
                {
                    info.InterfaceType = ParseInterfaceType(typeToken);
                }
                adapters.Add(info);
            }

            return adapters;
        }

        private static bool TryParseInterfaceLine(string line, out string adminToken,
            out string stateToken, out string typeToken, out string nameToken)
        {
            adminToken = "";
            stateToken = "";
            typeToken = "";
            nameToken = "";

            string trimmed = line.Trim();
            MatchCollection gaps = Regex.Matches(trimmed, "\\s{2,}");
            if (gaps.Count < 3)
            {
                return false;
            }

            int firstEnd = gaps[0].Index;
            int secondStart = gaps[0].Index + gaps[0].Length;
            int secondEnd = gaps[1].Index;
            int thirdStart = gaps[1].Index + gaps[1].Length;
            int thirdEnd = gaps[2].Index;
            int nameStart = gaps[2].Index + gaps[2].Length;

            if (firstEnd <= 0 || nameStart >= trimmed.Length)
            {
                return false;
            }

            adminToken = trimmed.Substring(0, firstEnd).Trim();
            stateToken = trimmed.Substring(secondStart, secondEnd - secondStart).Trim();
            typeToken = trimmed.Substring(thirdStart, thirdEnd - thirdStart).Trim();
            nameToken = trimmed.Substring(nameStart).Trim();
            return adminToken.Length > 0 && nameToken.Length > 0;
        }

        private static bool IsSeparatorLine(string line)
        {
            string trimmed = line.Trim();
            if (trimmed.Length < 8)
            {
                return false;
            }
            bool hasDash = false;
            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                if (c == '-')
                {
                    hasDash = true;
                }
                else if (c != ' ')
                {
                    return false;
                }
            }
            return hasDash;
        }

        private static bool ParseEnabledToken(string token, out bool known)
        {
            known = false;
            string t = (token == null ? "" : token.Trim()).ToLowerInvariant();
            if (t.Length == 0)
            {
                return false;
            }

            if (t.IndexOf("禁用", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("停用", StringComparison.Ordinal) >= 0 ||
                t.StartsWith("disabl", StringComparison.Ordinal) ||
                t == "無効" || t == "无效" || t.StartsWith("deaktiv", StringComparison.Ordinal) ||
                t.StartsWith("désactiv", StringComparison.Ordinal) ||
                t.StartsWith("desactiv", StringComparison.Ordinal) ||
                t.StartsWith("deshabilit", StringComparison.Ordinal) ||
                t.StartsWith("inhabilit", StringComparison.Ordinal) ||
                t.StartsWith("disattiv", StringComparison.Ordinal) ||
                t.StartsWith("отключ", StringComparison.Ordinal) ||
                t.StartsWith("uitgesch", StringComparison.Ordinal) ||
                t.StartsWith("desativ", StringComparison.Ordinal) ||
                t.StartsWith("wyłącz", StringComparison.Ordinal) ||
                t.StartsWith("devre", StringComparison.Ordinal) ||
                t.StartsWith("사용 안", StringComparison.Ordinal))
            {
                known = true;
                return false;
            }

            if (t.IndexOf("启用", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("啟用", StringComparison.Ordinal) >= 0 ||
                t.StartsWith("enabl", StringComparison.Ordinal) ||
                t == "有効" || t == "有效" || t.StartsWith("aktiv", StringComparison.Ordinal) ||
                t.StartsWith("activ", StringComparison.Ordinal) ||
                t.StartsWith("habilit", StringComparison.Ordinal) ||
                t.StartsWith("attiv", StringComparison.Ordinal) ||
                t.StartsWith("включ", StringComparison.Ordinal) ||
                t.StartsWith("ingesch", StringComparison.Ordinal) ||
                t.StartsWith("ativ", StringComparison.Ordinal) ||
                t.StartsWith("włącz", StringComparison.Ordinal) ||
                t.StartsWith("etkin", StringComparison.Ordinal) ||
                t.StartsWith("사용", StringComparison.Ordinal))
            {
                known = true;
                return true;
            }

            return false;
        }

        private static bool ParseConnectedToken(string token)
        {
            string t = (token == null ? "" : token.Trim()).ToLowerInvariant();
            if (t.Length == 0)
            {
                return false;
            }
            if (t.IndexOf("断开", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("未连接", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("disconnect", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("nicht verbunden", StringComparison.Ordinal) >= 0)
            {
                return false;
            }
            return t.IndexOf("连接", StringComparison.Ordinal) >= 0 ||
                   t.IndexOf("connect", StringComparison.Ordinal) >= 0 ||
                   t.IndexOf("接続", StringComparison.Ordinal) >= 0;
        }

        private static int ParseInterfaceType(string typeToken)
        {
            string t = (typeToken == null ? "" : typeToken.Trim()).ToLowerInvariant();
            if (t.IndexOf("wireless", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("802.11", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("无线", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("wlan", StringComparison.Ordinal) >= 0)
            {
                return AdapterInfo.TypeWireless;
            }
            if (t.IndexOf("dedicated", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("ethernet", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("以太", StringComparison.Ordinal) >= 0 ||
                t.IndexOf("专用", StringComparison.Ordinal) >= 0)
            {
                return AdapterInfo.TypeEthernet;
            }
            return 0;
        }

        private static int CompareAdapters(AdapterInfo a, AdapterInfo b)
        {
            return string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        private static object GetProperty(ManagementBaseObject item, string name)
        {
            try
            {
                return item[name];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static int ToInt(object value, int defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }
            try
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private static string CleanOutput(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            string cleaned = text.Replace("\r", " ").Replace("\n", " ").Trim();
            cleaned = Regex.Replace(cleaned, "\\s+", " ");
            if (cleaned.Length > 400)
            {
                cleaned = cleaned.Substring(0, 400) + "...";
            }
            return cleaned;
        }
    }
}
