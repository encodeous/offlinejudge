using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;

namespace judge
{
    static class Executor
    {
        public static Command CreateCommand(this ExecutionInfo info)
        {
            return Cli.Wrap(info.FileName)
                .WithArguments(info.Arguments)
                .WithWorkingDirectory(info.WorkingDirectory);
        }
        public static async Task<ExecutionResult> ExecuteJudge(this Command cmd, ExecutionInfo einfo, CancellationToken token)
        {
            var val = await cmd.ExecuteLimitedAsync(einfo.MemoryLimit, TimeSpan.FromSeconds(einfo.TimeLimit), token).ConfigureAwait(false);
            var result = new ExecutionResult();

            result.Result = ExecutorResult.None;
            
            if (val.ExitCode != 0)
            {
                result.Result = ExecutorResult.RTE;
            }

            if (val.MemoryUsedMb >= einfo.MemoryLimit)
            {
                result.Result = ExecutorResult.MLE;
            }

            if (val.RunTime >= TimeSpan.FromSeconds(einfo.TimeLimit))
            {
                result.Result = ExecutorResult.TLE;
            }

            result.TimeMilliseconds = (int)val.RunTime.TotalMilliseconds;
            result.MemoryMb = val.MemoryUsedMb;
            result.ExitCode = val.ExitCode;
            return result;
        }
    }
}