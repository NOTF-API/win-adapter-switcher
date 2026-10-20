using System;

namespace WinAdapterSwitcher
{
    internal sealed class ParsedCommand
    {
        public const string None = "none";
        public const string Wired = "wired";
        public const string Wireless = "wireless";
        public const string Enable = "enable";
        public const string Disable = "disable";
        public const string AllEnable = "all-enable";
        public const string AllDisable = "all-disable";
        public const string Settings = "settings";
        public const string Menu = "menu";
        public const string Help = "help";
        public const string Unknown = "unknown";

        public string Kind = None;
        public string Argument = "";
        public string Raw = "";

        public bool IsAction
        {
            get
            {
                return Kind == Wired || Kind == Wireless || Kind == Enable ||
                       Kind == Disable || Kind == AllEnable || Kind == AllDisable;
            }
        }
    }

    internal static class CommandLine
    {
        public static string Usage
        {
            get
            {
                return L.T("Usage.Header") + "\r\n" +
                       L.T("Usage.StartTray") + "\r\n" +
                       L.T("Usage.Wired") + "\r\n" +
                       L.T("Usage.Wifi") + "\r\n" +
                       L.T("Usage.Enable") + "\r\n" +
                       L.T("Usage.Disable") + "\r\n" +
                       L.T("Usage.AllEnable") + "\r\n" +
                       L.T("Usage.AllDisable") + "\r\n" +
                       L.T("Usage.Settings") + "\r\n" +
                       L.T("Usage.Menu") + "\r\n" +
                       L.T("Usage.Help");
            }
        }

        public static ParsedCommand Parse(string[] args)
        {
            ParsedCommand command = new ParsedCommand();
            if (args == null || args.Length == 0)
            {
                return command;
            }

            string first = (args[0] == null ? "" : args[0].Trim()).ToLowerInvariant();
            command.Raw = first;

            if (first == "--wired" || first == "-wired" || first == "/wired")
            {
                command.Kind = ParsedCommand.Wired;
            }
            else if (first == "--wifi" || first == "-wifi" || first == "/wifi" ||
                     first == "--wireless" || first == "-wireless" || first == "/wireless")
            {
                command.Kind = ParsedCommand.Wireless;
            }
            else if (first == "--enable" || first == "-enable" || first == "/enable")
            {
                command.Kind = ParsedCommand.Enable;
                command.Argument = args.Length > 1 ? args[1] : "";
            }
            else if (first == "--disable" || first == "-disable" || first == "/disable")
            {
                command.Kind = ParsedCommand.Disable;
                command.Argument = args.Length > 1 ? args[1] : "";
            }
            else if (first == "--all-enable" || first == "-all-enable" || first == "/all-enable")
            {
                command.Kind = ParsedCommand.AllEnable;
            }
            else if (first == "--all-disable" || first == "-all-disable" || first == "/all-disable")
            {
                command.Kind = ParsedCommand.AllDisable;
            }
            else if (first == "--settings" || first == "-settings" || first == "/settings")
            {
                command.Kind = ParsedCommand.Settings;
            }
            else if (first == "--menu" || first == "-menu" || first == "/menu")
            {
                command.Kind = ParsedCommand.Menu;
            }
            else if (first == "--help" || first == "-h" || first == "/?" || first == "/help")
            {
                command.Kind = ParsedCommand.Help;
            }
            else
            {
                command.Kind = ParsedCommand.Unknown;
                command.Argument = args[0];
            }

            return command;
        }

        public static OperationResult ExecuteAction(SwitchEngine engine, ParsedCommand command)
        {
            if (command == null)
            {
                return null;
            }
            switch (command.Kind)
            {
                case ParsedCommand.Wired:
                    return engine.ApplyWiredPriority();
                case ParsedCommand.Wireless:
                    return engine.ApplyWirelessPriority();
                case ParsedCommand.Enable:
                    if (string.IsNullOrEmpty(command.Argument))
                    {
                        return OperationResult.Fail(L.T("Error.EnableNameRequired"));
                    }
                    return engine.SetAdapter(command.Argument, true);
                case ParsedCommand.Disable:
                    if (string.IsNullOrEmpty(command.Argument))
                    {
                        return OperationResult.Fail(L.T("Error.DisableNameRequired"));
                    }
                    return engine.SetAdapter(command.Argument, false);
                case ParsedCommand.AllEnable:
                    return engine.EnableAll();
                case ParsedCommand.AllDisable:
                    return engine.DisableAll();
                default:
                    return null;
            }
        }
    }
}
