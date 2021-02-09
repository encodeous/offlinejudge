using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace judge
{
    [Verb("run", HelpText = "Run solutions")]
    public class JudgeOptions
    {
        [Value(0, Required = false, HelpText = "Configuration File Path, defaults to current directory", Default = "judge.yaml")]
        public string Config { get; set; }
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
                    Judge(o).GetAwaiter().GetResult();
                });
        }

        private static async Task Judge(JudgeOptions o)
        {
            while (true)
            {
                cts = new CancellationTokenSource();
                if (!File.Exists(o.Config))
                {
                    File.WriteAllText(o.Config, @"
#
# Offline Judge - Configuration File
# https://github.com/encodeous/offlinejudge
#

# Commands that are run once before judging any cases
pre-judge-commands:
- command: cmd
  arguments: --arguments here
- command: cmd2
  arguments: --arguments here
# Should the output be shown?
show-pre-judge-output: true
# Should the judge stop judging with it hits a non-ac case? (The test data may get buried by cases in the console!)
short-circuit: true
# Number of cases the judge runs
cases: 100
# The max number of parallel executions that occur at any given time
judge-threads: 3
# Should the judge use an exact grader or a space-separated token grader?
token-grader: false
solution:
  # Working directory for the executing process, leave blank for current directory
  working-directory: 
  # Filename or the full file path to the program
  file-name: solution.exe
  arguments: --arguments here
  # Time limit in seconds
  time-limit: 2.5
  # Memory limit in MB
  memory-limit: 512
reference:
  working-directory: 
  file-name: reference.exe
  arguments: --arguments here
  time-limit: 2.5
  memory-limit: 512
generator:
  working-directory: 
  file-name: generator.exe
  arguments: --arguments here
  time-limit: 10
  memory-limit: 1024
");
                    Console.WriteLine("No configuration file found! Creating default configuration...");
                    return;
                }

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(HyphenatedNamingConvention.Instance)
                    .Build();

                var cfg = deserializer.Deserialize<JudgeConfig>(File.ReadAllText(o.Config));

                if (cfg.TokenGrader)
                {
                    var j = new Judge(cfg, new TokenGrader(), cts.Token, cts);
                    await j.JudgeSolution(cfg.Cases, cfg.JudgeThreads);
                }
                else
                {
                    var j = new Judge(cfg, new ExactGrader(), cts.Token, cts);
                    await j.JudgeSolution(cfg.Cases, cfg.JudgeThreads);
                }

                Console.WriteLine("Enter 'r' to rejudge, press any other key to exit!");
                var k = Console.ReadKey();
                if (k.Key != ConsoleKey.R)
                {
                    break;
                }
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Halting Judges...");
            e.Cancel = true;
            cts.Cancel();
        }
    }
}
