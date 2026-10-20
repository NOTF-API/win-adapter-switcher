using System;
using System.Diagnostics;
using System.Text;

namespace WinAdapterSwitcher
{
    internal sealed class ProcessRunResult
    {
        public bool Started = false;
        public bool TimedOut = false;
        public int ExitCode = -1;
        public string Output = "";
    }

    internal static class ProcessRunner
    {
        public static ProcessRunResult Run(string fileName, string arguments, int timeoutMilliseconds)
        {
            ProcessRunResult result = new ProcessRunResult();
            StringBuilder output = new StringBuilder();
            object syncRoot = new object();

            try
            {
                ProcessStartInfo info = new ProcessStartInfo(fileName, arguments);
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.StandardOutputEncoding = Encoding.Default;
                info.StandardErrorEncoding = Encoding.Default;

                using (Process process = new Process())
                {
                    process.StartInfo = info;
                    process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data != null)
                        {
                            lock (syncRoot)
                            {
                                output.AppendLine(e.Data);
                            }
                        }
                    };
                    process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data != null)
                        {
                            lock (syncRoot)
                            {
                                output.AppendLine(e.Data);
                            }
                        }
                    };

                    result.Started = process.Start();
                    if (!result.Started)
                    {
                        return result;
                    }

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (!process.WaitForExit(timeoutMilliseconds))
                    {
                        result.TimedOut = true;
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    process.WaitForExit();
                    result.ExitCode = process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                result.Output = ex.Message;
                return result;
            }

            lock (syncRoot)
            {
                result.Output = output.ToString().Trim();
            }
            return result;
        }
    }
}
