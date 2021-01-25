using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace judge
{
    class Executor
    {
        public static async Task<ExecutionResult> Execute(int id, string command, string input, long maxMemoryBytes, int maxTimeMs, CancellationToken ct)
        {
            ExecutionResult result = new ExecutionResult();
            result.Id = id;
            var info = new ProcessStartInfo("cmd.exe", "/C "+command)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            info.WorkingDirectory = Environment.CurrentDirectory;
            var proc = new Process();
            proc.StartInfo = info;
            proc.Start();
            var dt = DateTime.Now;
            Task.Run(async () =>
            {
                await proc.StandardInput.WriteAsync(input);
                await proc.StandardInput.FlushAsync();
            });

            while (!proc.HasExited)
            {
                proc.Refresh();
                if (ct.IsCancellationRequested)
                {
                    proc.Kill();
                    return new ExecutionResult()
                    {
                        Result = ExecutorResult.None
                    };
                }
                if (DateTime.Now - dt > TimeSpan.FromMilliseconds(maxTimeMs))
                {
                    proc.Kill();
                    result.Result = ExecutorResult.TLE;
                    result.TimeMilliseconds = maxTimeMs;
                    return result;
                }

                try
                {
                    result.MemoryBytes = Math.Max(result.MemoryBytes, proc.PrivateMemorySize64);
                }
                catch
                {

                }

                if (result.MemoryBytes > maxMemoryBytes)
                {
                    proc.Kill();
                    result.Result = ExecutorResult.MLE;
                    result.TimeMilliseconds = (int)(DateTime.Now - dt).TotalMilliseconds;
                    result.Output = await proc.StandardOutput.ReadToEndAsync();
                    return result;
                }
                await Task.Delay(10);
            }
            var et = DateTime.Now;
            if (proc.ExitCode != 0)
            {
                result.Result = ExecutorResult.RTE;
                result.ExitCode = proc.ExitCode;
                result.Output = await proc.StandardOutput.ReadToEndAsync();
            }
            else
            {
                result.Result = ExecutorResult.None;
            }

            result.TimeMilliseconds = (int)(et - dt).TotalMilliseconds;

            result.Output = await proc.StandardOutput.ReadToEndAsync();
            return result;
        }
    }
}
