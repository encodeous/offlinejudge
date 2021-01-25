using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace judge
{
    public class ExecutionResult
    {
        public int Id;
        public string Output;
        public int TimeMilliseconds;
        public long MemoryBytes;
        public ExecutorResult Result;
        public int ExitCode;
    }

    public enum ExecutorResult
    {
        TLE,
        MLE,
        RTE,
        AC,
        WA,
        None
    }
}
