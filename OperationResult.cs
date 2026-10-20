using System;

namespace WinAdapterSwitcher
{
    internal sealed class OperationResult
    {
        public bool Success;
        public string Message;

        public OperationResult(bool success, string message)
        {
            Success = success;
            Message = message == null ? "" : message;
        }

        public static OperationResult Ok(string message)
        {
            return new OperationResult(true, message);
        }

        public static OperationResult Fail(string message)
        {
            return new OperationResult(false, message);
        }
    }
}
