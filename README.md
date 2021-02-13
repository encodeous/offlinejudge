# Offline Judge

Offline Judge is an advanced and configurable `fast/slow` grader that runs on your own computer!

This is a common technique used in contests to validate the accuracy of a solution.

## Quick Start

### Before Starting:

1. OJ requires `dotnetcore 3.1` to be installed, install [dotnet](https://dotnet.microsoft.com/download) if it is not installed already!
2. Make sure you have a compiler that can run your desired solution
3. Download ([Github Releases](https://github.com/encodeous/offlinejudge/releases)) and set up Judge!

### Setting Up: 

- Download Judge from the `Releases` section for your platform.
- **Windows Only (Optional)**: Run `judge.exe install` to install the application and add it to `PATH`
- Create a `judge.yaml`, an example is shown below.
- Modify the configuration to your heart's content.
- To run the judge, open a terminal and execute `judge run`. Make sure the current working directory contains a `judge.yaml`, or specify the path to `judge.yaml` with `judge run <configuration file path>`

## Features

- Pre-judge commands - used for compilation, cleanup etc
- Independent resource limits for `[solution, reference, generator]`
- Parallel Judging - Allows for faster judging
- Short Circuit - Stops execution when a non-ac case is found
- Pretty printing - Nicely formatted judging results, similar to the format used on dmoj.ca
- Token/Exact grading - Able to switch between Exact grading and Tokenized Grading (separated by `\n` or space)

### Version Compatibility

In future versions, old configuration files may **not** be compatible with older versions!

### Example Configuration

```yaml
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
```

### Uninstalling (Windows)

- Run `judge uninstall` and make sure judge is not running.