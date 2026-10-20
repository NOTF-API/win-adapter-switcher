using System;
using System.Collections.Generic;

namespace WinAdapterSwitcher
{
    internal static class AdapterDetector
    {
        public static bool EnsureConfigured(Config config)
        {
            List<AdapterInfo> adapters;
            try
            {
                adapters = AdapterService.GetAdapters();
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("Adapter auto-detection failed: {0}", ex.Message);
                return false;
            }
            return EnsureConfigured(config, adapters);
        }

        public static bool EnsureConfigured(Config config, List<AdapterInfo> adapters)
        {
            if (config == null || adapters == null)
            {
                return false;
            }

            bool changed = false;
            if (string.IsNullOrEmpty(config.WiredAdapter))
            {
                AdapterInfo wired = FindFirstOfType(adapters, AdapterInfo.TypeEthernet);
                if (wired != null)
                {
                    config.WiredAdapter = wired.Name;
                    config.WiredDescription = wired.Description;
                    changed = true;
                }
            }
            if (string.IsNullOrEmpty(config.WirelessAdapter))
            {
                AdapterInfo wireless = FindFirstOfType(adapters, AdapterInfo.TypeWireless);
                if (wireless != null)
                {
                    config.WirelessAdapter = wireless.Name;
                    config.WirelessDescription = wireless.Description;
                    changed = true;
                }
            }

            if (changed)
            {
                Logger.WriteFormat("Auto-detected adapters: wired={0}, wireless={1}",
                    config.WiredAdapter, config.WirelessAdapter);
                try
                {
                    config.Save(AppPaths.ConfigFile);
                }
                catch (Exception ex)
                {
                    Logger.WriteFormat("Saving auto-detected adapters failed: {0}", ex.Message);
                }
            }
            return changed;
        }

        private static AdapterInfo FindFirstOfType(List<AdapterInfo> adapters, int interfaceType)
        {
            for (int i = 0; i < adapters.Count; i++)
            {
                if (adapters[i].InterfaceType == interfaceType)
                {
                    return adapters[i];
                }
            }
            return null;
        }
    }
}
