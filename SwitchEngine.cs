using System;
using System.Collections.Generic;
using System.Text;

namespace WinAdapterSwitcher
{
    internal sealed class SwitchEngine
    {
        private readonly Config _config;

        public SwitchEngine(Config config)
        {
            _config = config;
        }

        public OperationResult ApplyWiredPriority()
        {
            return ApplyPair(true, false, L.T("Action.WiredDone"));
        }

        public OperationResult ApplyWirelessPriority()
        {
            return ApplyPair(false, true, L.T("Action.WirelessDone"));
        }

        public OperationResult EnableAll()
        {
            return ApplyAll(true);
        }

        public OperationResult DisableAll()
        {
            return ApplyAll(false);
        }

        public OperationResult SetAdapter(string name, bool enabled)
        {
            return AdapterService.SetEnabled(name, enabled);
        }

        private OperationResult ApplyPair(bool wiredEnabled, bool wirelessEnabled, string successMessage)
        {
            bool hasWired = !string.IsNullOrEmpty(_config.WiredAdapter);
            bool hasWireless = !string.IsNullOrEmpty(_config.WirelessAdapter);
            if (!hasWired || !hasWireless)
            {
                return OperationResult.Fail(L.T("Error.ConfigNeedsBoth"));
            }

            OperationResult wired = AdapterService.SetEnabled(_config.WiredAdapter, wiredEnabled);
            OperationResult wireless = AdapterService.SetEnabled(_config.WirelessAdapter, wirelessEnabled);

            if (wired.Success && wireless.Success)
            {
                return OperationResult.Ok(successMessage);
            }
            return OperationResult.Fail(Combine(
                L.F("Error.WiredDetail", wired.Message),
                L.F("Error.WirelessDetail", wireless.Message)));
        }

        private OperationResult ApplyAll(bool enabled)
        {
            List<string> names = new List<string>();
            List<string> labels = new List<string>();
            if (!string.IsNullOrEmpty(_config.WiredAdapter))
            {
                names.Add(_config.WiredAdapter);
                labels.Add(L.T("Role.Wired"));
            }
            if (!string.IsNullOrEmpty(_config.WirelessAdapter))
            {
                names.Add(_config.WirelessAdapter);
                labels.Add(L.T("Role.Wireless"));
            }

            if (names.Count == 0)
            {
                return OperationResult.Fail(L.T("Error.ConfigNeedsAny"));
            }

            List<string> details = new List<string>();
            bool allOk = true;
            for (int i = 0; i < names.Count; i++)
            {
                OperationResult result = AdapterService.SetEnabled(names[i], enabled);
                details.Add(labels[i] + L.F("Error.DetailSuffix", result.Message));
                if (!result.Success)
                {
                    allOk = false;
                }
            }

            if (allOk)
            {
                return OperationResult.Ok(L.T(enabled ? "Action.AllEnabled" : "Action.AllDisabled"));
            }
            return OperationResult.Fail(Combine(details.ToArray()));
        }

        private static string Combine(params string[] parts)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }
                sb.Append(parts[i]);
            }
            return sb.ToString();
        }
    }
}
