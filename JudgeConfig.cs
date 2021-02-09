using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace judge
{
    public class JudgeConfig
    {
        public PreJudgeInfo[] PreJudgeCommands = new []
        {
            new PreJudgeInfo()
                {
                    Arguments = "--arguments here",
                    Command = "cmd"
                },
            new PreJudgeInfo()
            {
                Arguments = "--arguments here",
                Command = "cmd2"
            },
        };
        public bool ShowPreJudgeOutput = true;
        public bool ShortCircuit = true;
        public int Cases = 100;
        public int JudgeThreads = 3;
        public bool TokenGrader = false;
        public ExecutionInfo Solution = new ExecutionInfo()
        {
            Arguments = "--arguments here",
            FileName = "solution.exe",
            MemoryLimit = 512,
            TimeLimit = 2.5
        };
        public ExecutionInfo Reference = new ExecutionInfo()
        {
            Arguments = "--arguments here",
            FileName = "reference.exe",
            MemoryLimit = 512,
            TimeLimit = 2.5
        };
        public ExecutionInfo Generator = new ExecutionInfo()
        {
            Arguments = "--arguments here",
            FileName = "generator.exe",
            MemoryLimit = 1024,
            TimeLimit = 10
        };
    }
}
