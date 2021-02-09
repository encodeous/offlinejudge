using System;

namespace CliWrap
{
    /// <summary>
    /// Represents the result of an execution of a command.
    /// </summary>
    public class ExtendedCommandResult
    {
        /// <summary>
        /// Exit code set by the underlying process.
        /// </summary>
        public int ExitCode { get; }
        
        /// <summary>
        /// Number of Megabytes used by the process
        /// </summary>
        public double MemoryUsedMb { get; }

        /// <summary>
        /// When the command was started.
        /// </summary>
        public DateTimeOffset StartTime { get; }

        /// <summary>
        /// When the command finished executing.
        /// </summary>
        public DateTimeOffset ExitTime { get; }

        /// <summary>
        /// Total duration of the command execution.
        /// </summary>
        public TimeSpan RunTime => ExitTime - StartTime;

        /// <summary>
        /// Initializes an instance of <see cref="CommandResult"/>.
        /// </summary>
        public ExtendedCommandResult(int exitCode, DateTimeOffset startTime, DateTimeOffset exitTime, double memUsed)
        {
            ExitCode = exitCode;
            StartTime = startTime;
            ExitTime = exitTime;
            MemoryUsedMb = memUsed;
        }
    }
}