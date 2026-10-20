using System;

namespace WinAdapterSwitcher
{
    internal sealed class AdapterInfo
    {
        public const int TypeEthernet = 6;
        public const int TypeWireless = 71;
        public const int TypeLoopback = 24;

        public string Name = "";
        public string Description = "";
        public string Id = "";
        public int InterfaceIndex = 0;
        public int InterfaceType = 0;
        public bool AdminKnown = false;
        public bool Enabled = false;
        public bool LinkUp = false;
        public string RawState = "";

        public string StatusText
        {
            get
            {
                if (!AdminKnown)
                {
                    return L.T("Status.Unknown");
                }
                if (!Enabled)
                {
                    return L.T("Status.Disabled");
                }
                if (LinkUp)
                {
                    return L.T("Status.EnabledConnected");
                }
                return L.T("Status.EnabledDisconnected");
            }
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                {
                    return Name;
                }
                return Name + " - " + Description;
            }
        }
    }
}
