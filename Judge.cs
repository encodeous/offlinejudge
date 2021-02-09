using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;

namespace judge
{
    public interface ICustomGrader
    {
        public bool Grade(Sio inputData, Sio referenceOutput, Sio submissionOutput);
    }
    public class Judge
    {
        private JudgeConfig config;
        private ICustomGrader _grader;
        private CancellationToken _token;
        private CancellationTokenSource source;
        public Judge(JudgeConfig cfg, ICustomGrader grader, CancellationToken ct, CancellationTokenSource cts)
        {
            config = cfg;
            _grader = grader;
            _token = ct;
            source = cts;
        }

        public void SaveData(int id, string scase, string refsol, string solsol)
        {
            PrintColor($"CASE {id+1}\n" +
                       $"{scase}\n" +
                       $"REFERENCE SOLUTION\n" +
                       $"{refsol}\n" +
                       $"YOUR SOLUTION\n" +
                       $"{solsol}\n", ConsoleColor.Cyan);
        }

        public ConcurrentBag<CaseResult> Results = new ConcurrentBag<CaseResult>();

        public async Task RunJudge(int cases)
        {
            for (int i = 0; i < cases; i++)
            {
                var dataBuffer = new StringBuilder();
                var solOutput = new StringBuilder();
                var refOutput = new StringBuilder();

                var gen = config.Generator.CreateCommand() | dataBuffer;

                var genv = await gen.ExecuteJudge(config.Generator, _token);
                genv.Output = dataBuffer.ToString();
                
                var sol = genv.Output | config.Solution.CreateCommand() | solOutput;
                var refsol = genv.Output | config.Reference.CreateCommand() | refOutput;
                
                var k = await Task.WhenAll(
                    sol.ExecuteJudge(config.Solution, _token),
                    refsol.ExecuteJudge(config.Reference, _token));
                
                k[0].Output = solOutput.ToString();
                k[1].Output = refOutput.ToString();
                Results.Add(new CaseResult()
                {
                    g = genv,
                    r = k[1],
                    s = k[0]
                });
            }
        }
        // Lovely
        int cnt = 0;
        int ac = 0;
        int sumTime = 0;
        double maxMem = 0;
        int maxTime = 0;
        public async Task JudgeSolution(int cases, int threads)
        {
            for (int i = 0; i < config.PreJudgeCommands.Length; i++)
            {
                PrintColor($"Running Pre-Judge Command [{i+1}/{config.PreJudgeCommands.Length}]\n", ConsoleColor.Yellow);
                var k = config.PreJudgeCommands[i];
                var cmd = Cli.Wrap(k.Command)
                    .WithArguments(k.Arguments)
                    .WithWorkingDirectory(Environment.CurrentDirectory);
                try
                {
                    if (config.ShowPreJudgeOutput)
                    {
                        await (cmd | Console.WriteLine).ExecuteAsync();
                    }
                    else
                    {
                        await cmd.ExecuteAsync();
                    }
                }
                catch
                {
                    PrintColor($"Command returned error! [{i+1}/{config.PreJudgeCommands.Length}]\n", ConsoleColor.Yellow);
                }

            }
            
            if (!File.Exists(config.Generator.FileName))
            {
                Console.WriteLine("Generator file not found!");
                return;
            }
            if (!File.Exists(config.Reference.FileName))
            {
                Console.WriteLine("Reference file not found!");
                return;
            }
            if (!File.Exists(config.Solution.FileName))
            {
                Console.WriteLine("Solution file not found!");
                return;
            }
            
            int cpt = (int) (cases / (double) threads);
            // Dw about this sketchy code lol
            for (int i = 0; i < threads - 1; i++)
            {
                Task.Run(async ()=> await RunJudge(cpt));
            }
            Task.Run(async ()=> await RunJudge(cpt + (cases - cpt * threads)));

            for (int i = 0; i < cases; i++)
            {
                while (!_token.IsCancellationRequested)
                {
                    if (Results.TryTake(out var k))
                    {
                        cnt++;
                        try
                        {
                            if (!CheckCase(i, k.g, k.s, k.r))
                            {
                                if (config.ShortCircuit)
                                {
                                    source.Cancel();
                                }
                            }
                            else
                            {
                                ac++;
                            }
                            break;
                        }
                        catch (FileNotFoundException)
                        {
                            source.Cancel();
                            break;
                        }
                    }
                    else
                    {
                        await Task.Delay(10);
                    }
                }

                if (_token.IsCancellationRequested) break;
            }


            Console.WriteLine($"Resources: {sumTime/1000.0:#.###}s, {maxMem:#.###} MB\n" +
                              $"Maximum runtime on single test case: {maxTime/1000.0:#.###}s\n" +
                              $"Final score: {ac}/{cnt}\n");
        }

        public bool CheckCase(int i, ExecutionResult generator, ExecutionResult sol, ExecutionResult refs)
        {
            bool accepted = true;
            if (generator.Result != ExecutorResult.None)
            {
                Console.WriteLine(
                    $"Generator failed! Result: {generator.Result.ToString()}, Exit Code: {generator.ExitCode}");
                // just use a bogus exception to signal, and break out of the current method
                throw new FileNotFoundException();
            }
            if (refs.Result != ExecutorResult.None)
            {
                Console.WriteLine(
                    $"Reference Solution failed! Result: {refs.Result.ToString()}, Exit Code: {refs.ExitCode}");
                throw new FileNotFoundException();
            }
            
            sumTime += sol.TimeMilliseconds;
            maxTime = Math.Max(maxTime, sol.TimeMilliseconds);
            maxMem = Math.Max(maxMem, sol.MemoryMb);
            
            if (sol.Result != ExecutorResult.None)
            {
                SaveData(i, generator.Output, refs.Output, sol.Output);
                Console.Write($"Case #{i + 1}: ");
                PrintStatus(sol.Result);
                if (sol.ExitCode != 0)
                {
                    Console.Write($" (exit code: {sol.ExitCode})");
                }

                Console.WriteLine($" [{sol.TimeMilliseconds / 1000.0:#.###}s,{sol.MemoryMb:#.###} MB]");
                return false;
            }

            if (_grader.Grade(new Sio(generator.Output), new Sio(refs.Output), new Sio(sol.Output)))
            {
                sol.Result = ExecutorResult.AC;
            }
            else
            {
                accepted = false;
                sol.Result = ExecutorResult.WA;
                SaveData(i, generator.Output, refs.Output, sol.Output);
            }
            
            Console.Write($"Case #{i + 1}: ");
            PrintStatus(sol.Result);
            Console.WriteLine(
                $" [{sol.TimeMilliseconds / 1000.0:#.###}s,{sol.MemoryMb:#.###} MB] Ref [{refs.TimeMilliseconds / 1000.0:#.###}s,{refs.MemoryMb:#.###} MB]");

            return accepted;
        }

        public void PrintColor(string text, ConsoleColor color)
        {
            var cc = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = cc;
        }
        
        public void PrintStatus(ExecutorResult result)
        {
            ConsoleColor nc = ConsoleColor.White;
            switch (result)
            {
                case ExecutorResult.AC:
                    nc = ConsoleColor.Green;
                    break;
                case ExecutorResult.MLE:
                    nc = ConsoleColor.Yellow;
                    break;
                case ExecutorResult.RTE:
                    nc = ConsoleColor.Yellow;
                    break;
                case ExecutorResult.WA:
                    nc = ConsoleColor.Red;
                    break;
                case ExecutorResult.TLE:
                    nc = ConsoleColor.DarkCyan;
                    break;
            }
            PrintColor(result.ToString(), nc);
        }
    }
}
