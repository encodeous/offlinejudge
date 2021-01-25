using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace judge
{
    public interface ICustomGrader
    {
        public bool Grade(Sio inputData, Sio referenceOutput, Sio submissionOutput);
    }
    public class Judge
    {
        private static long MbBytes = 1024 * 1024;
        private JudgeOptions options;
        private ICustomGrader _grader;
        private CancellationToken token;
        private CancellationTokenSource source;
        public Judge(JudgeOptions opt, ICustomGrader grader, CancellationToken ct, CancellationTokenSource cts)
        {
            options = opt;
            _grader = grader;
            token = ct;
            source = cts;
        }

        public void SaveData(int id, string scase, string refsol, string solsol)
        {
            var cc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            string k = $"CASE {id+1}\n" +
                       $"{scase}\n" +
                       $"REFERENCE SOLUTION\n" +
                       $"{refsol}\n" +
                       $"YOUR SOLUTION\n" +
                       $"{solsol}\n";
            if (options.Output != "")
            {
                File.AppendAllText(options.Output, k);
            }
            else
            {
                Console.WriteLine(k);
            }

            Console.ForegroundColor = cc;
        }

        public ConcurrentBag<CaseResult> Results = new ConcurrentBag<CaseResult>();

        public async Task RunJudge(int cases)
        {
            for (int i = 0; i < cases; i++)
            {
                if (token.IsCancellationRequested) break;
                var generator = await Executor.Execute(i, options.Generator, "", MbBytes * options.MemoryLimit, (int) options.TimeLimit * 1000, token);
                if (token.IsCancellationRequested) break;
                var refs = await Executor.Execute(i, options.Reference, generator.Output, MbBytes * options.MemoryLimit, (int) options.TimeLimit * 1000, token);
                if (token.IsCancellationRequested) break;
                var sol = await Executor.Execute(i, options.Solution, generator.Output, MbBytes * options.MemoryLimit, (int) options.TimeLimit * 1000, token);
                Results.Add(new CaseResult()
                {
                    g = generator,
                    r = refs,
                    s = sol
                });
            }
        }
        // Lovely
        int cnt = 0;
        int ac = 0;
        int sumTime = 0;
        long maxMem = 0;
        int maxTime = 0;
        public async Task JudgeSolution(int cases, int threads)
        {
            int cpt = (int) (cases / (double) threads);
            // Dw about this sketchy code lol
            for (int i = 0; i < threads - 1; i++)
            {
                Task.Run(async ()=> await RunJudge(cpt));
            }
            Task.Run(async ()=> await RunJudge(cpt + (cases - cpt * threads)));

            for (int i = 0; i < cases; i++)
            {
                while (!token.IsCancellationRequested)
                {
                    if (Results.TryTake(out var k))
                    {
                        cnt++;
                        try
                        {
                            if (!CheckCase(i, k.g, k.s, k.r))
                            {
                                if (options.ShortCircuit)
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
                        catch (FileNotFoundException e)
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

                if (token.IsCancellationRequested) break;
            }


            Console.WriteLine($"Resources: {sumTime/1000.0:#.###}s, {maxMem/MbBytes} MB\n" +
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

            if (sol.Result != ExecutorResult.None)
            {
                SaveData(i, generator.Output, refs.Output, sol.Output);
                Console.Write($"Case #{i + 1}: ");
                PrintStatus(sol.Result);
                if (sol.ExitCode != 0)
                {
                    Console.WriteLine($" (exit code: {sol.ExitCode})");
                }

                Console.WriteLine($" [{sol.TimeMilliseconds / 1000.0:#.###}s,{sol.MemoryBytes / MbBytes} MB]");
                return false;
            }

            if (_grader.Grade(new Sio(generator.Output), new Sio(refs.Output), new Sio(sol.Output)))
            {
                sol.Result = ExecutorResult.AC;
                if (options.ShowAc)
                {
                    SaveData(i, generator.Output, refs.Output, sol.Output);
                }
            }
            else
            {
                accepted = false;
                sol.Result = ExecutorResult.WA;
                SaveData(i, generator.Output, refs.Output, sol.Output);
            }

            sumTime += sol.TimeMilliseconds;
            maxTime = Math.Max(maxTime, sol.TimeMilliseconds);
            maxMem = Math.Max(maxMem, sol.MemoryBytes);
            Console.Write($"Case #{i + 1}: ");
            PrintStatus(sol.Result);
            Console.WriteLine(
                $" [{sol.TimeMilliseconds / 1000.0:#.###}s,{sol.MemoryBytes / MbBytes} MB] Ref [{refs.TimeMilliseconds / 1000.0:#.###}s,{refs.MemoryBytes / MbBytes} MB]");

            return accepted;
        }

        public void PrintStatus(ExecutorResult result)
        {
            var cc = Console.ForegroundColor;
            ConsoleColor nc = cc;
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
            Console.ForegroundColor = nc;
            Console.Write(result.ToString());
            Console.ForegroundColor = cc;
        }
    }
}
