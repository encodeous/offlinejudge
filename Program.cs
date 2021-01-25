using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace judge
{
    [Verb("run", HelpText = "Run solutions")]
    public class JudgeOptions
    {
        [Value(0, Required = true, HelpText = "Specify Generator Path, or command to run the Generator enclosed in quotations.")]
        public string Generator { get; set; }
        [Value(1, Required = true, HelpText = "Solution Path, or command to run the Solution enclosed in quotations.")]
        public string Solution { get; set; }
        [Value(2, Required = true, HelpText = "Reference Solution Path, or command to run the Reference Solution enclosed in quotations.")]
        public string Reference { get; set; }

        [Option('o', "output", HelpText = "Output cases as a file, and specify a path")]
        public string Output { get; set; } = "";
        [Option('a', "ac", HelpText = "Output AC cases along with WA cases", Default = false)]
        public bool ShowAc { get; set; }
        [Option('s', "shortcircuit", HelpText = "Short Circuit - Stop judging on non-AC", Default = false)]
        public bool ShortCircuit { get; set; }
        [Option('m',"mem", HelpText = "Memory Limit in Megabytes", Default = 512)]
        public int MemoryLimit { get; set; }
        [Option('t',"time", HelpText = "Time Limit in Seconds", Default = 2.0)]
        public double TimeLimit { get; set; }
        [Option('c',"cases", HelpText = "Cases", Default = 10)]
        public int Cases { get; set; }
        [Option('p',"parallel", HelpText = "Parallel Judging Threads", Default = 3)]
        public int Threads { get; set; }
    }
    [Verb("install", false, HelpText = "Install the application, and add the program to PATH")]
    public class InstallOptions
    {
    }
    [Verb("uninstall", false, HelpText = "Uninstall the application, and remove the program from PATH")]
    public class UninstallOptions
    {
    }
    class Program
    {
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        public static string GetFullPath(string fileName)
        {
            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                FileInfo fi = new FileInfo(fullPath);
                if(File.Exists(fullPath) && fi.FullName != Process.GetCurrentProcess().MainModule.FileName)
                    return fullPath;
            }
            return null;
        }
        static bool Install()
        {
            if (ExistsOnPath("judge.exe"))
            {
                return false;
            }
            else
            {
                var installDir = Environment.ExpandEnvironmentVariables("%APPDATA%\\judge");
                if (!Directory.Exists(installDir))
                {
                    Directory.CreateDirectory(installDir);
                }
                File.Copy(Process.GetCurrentProcess().MainModule.FileName, Path.Combine(installDir,"judge.exe"));
                var value = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                value += installDir;
                value += Path.PathSeparator;
                Environment.SetEnvironmentVariable("PATH", value, EnvironmentVariableTarget.User);
                return true;
            }
        }
        static bool Uninstall()
        {
            var installDir = Environment.ExpandEnvironmentVariables("%APPDATA%\\judge");
            var file = Path.Combine(installDir, "judge.exe");
            if (File.Exists(file))
            {
                string newPath = "";
                var values = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                foreach (var path in values.Split(Path.PathSeparator))
                {
                    if (path != installDir && path != "")
                    {
                        newPath += path + Path.PathSeparator;
                    }
                }
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                Process.Start( new ProcessStartInfo()
                {
                    Arguments = "/C choice /C Y /N /D Y /T 1 & rmdir \"" + installDir +"\" /s /q",
                    WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, FileName = "cmd.exe"
                });
                Console.WriteLine("Successfully uninstalled!");
                Environment.Exit(0);
                return true;
            }
            else
            {
                return false;
            }
        }
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<JudgeOptions, InstallOptions, UninstallOptions>(args)
                .WithParsed<InstallOptions>(o =>
                {
                    Console.WriteLine("Installing judge to AppData...");
                    if (Install())
                    {
                        Console.WriteLine("Installed judge! Restart your terminal to use the \"judge\" command!");
                    }
                    else
                    {
                        Console.WriteLine("Failed installing judge! Is it already installed?");
                    }
                    
                })
                .WithParsed<UninstallOptions>(o =>
                {
                    if (!Uninstall())
                    {
                        Console.WriteLine("Failed uninstalling judge! Is it installed?");
                    }
                })
                .WithParsed<JudgeOptions>(o =>
                {
                    Console.CancelKeyPress += Console_CancelKeyPress;
                    var j = new Judge(o, new ExactGrader(), cts.Token, cts);
                    j.JudgeSolution(o.Cases, o.Threads).GetAwaiter().GetResult();
                });
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cts.Cancel();
        }
    }
}
