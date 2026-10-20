using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WinAdapterSwitcher
{
    internal sealed class IpcServer : IDisposable
    {
        public const string PipeName = "WinAdapterSwitcher.Ipc";

        private readonly Control _dispatcher;
        private readonly Action<string[]> _handler;
        private volatile bool _running = true;
        private Thread _thread;

        public IpcServer(Control dispatcher, Action<string[]> handler)
        {
            _dispatcher = dispatcher;
            _handler = handler;
        }

        public void Start()
        {
            _thread = new Thread(Loop);
            _thread.IsBackground = true;
            _thread.Name = "WinAdapterSwitcher.Ipc";
            _thread.Start();
        }

        private void Loop()
        {
            while (_running)
            {
                try
                {
                    using (NamedPipeServerStream pipe = new NamedPipeServerStream(
                        PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None))
                    {
                        pipe.WaitForConnection();

                        List<string> lines = new List<string>();
                        using (StreamReader reader = new StreamReader(pipe, new UTF8Encoding(false)))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lines.Add(line);
                            }
                        }

                        if (!_running)
                        {
                            break;
                        }

                        string[] args = lines.ToArray();
                        try
                        {
                            if (_dispatcher != null && !_dispatcher.IsDisposed)
                            {
                                _dispatcher.BeginInvoke(new Action(delegate { _handler(args); }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFormat("IPC dispatch failed: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_running)
                    {
                        Logger.WriteFormat("IPC server error: {0}", ex.Message);
                        Thread.Sleep(500);
                    }
                }
            }
        }

        public void Dispose()
        {
            _running = false;
            try
            {
                using (NamedPipeClientStream client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    client.Connect(500);
                }
            }
            catch (Exception)
            {
            }
            if (_thread != null)
            {
                try
                {
                    _thread.Join(1000);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    internal static class IpcClient
    {
        public static bool Send(string[] args)
        {
            try
            {
                using (NamedPipeClientStream client = new NamedPipeClientStream(".", IpcServer.PipeName, PipeDirection.Out))
                {
                    client.Connect(1500);
                    using (StreamWriter writer = new StreamWriter(client, new UTF8Encoding(false)))
                    {
                        if (args != null)
                        {
                            for (int i = 0; i < args.Length; i++)
                            {
                                writer.WriteLine(args[i]);
                            }
                        }
                        writer.Flush();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFormat("IPC client failed: {0}", ex.Message);
                return false;
            }
        }
    }
}
