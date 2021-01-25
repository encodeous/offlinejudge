# Offline Judge

Offline Judge is a program that compares the output of a `reference/slow` solution with a `fast` solution.

This is a common technique used in contests to validate the accuracy of a solution.

Offline Judge can be used with the `judge` command.

There are three main commands that Offline Judge offers, `judge run` runs the solutions and validates their output, `judge install` installs the program into `%APPDATA%\judge\judge.exe` and adds it to the `PATH` environment variable, `judge uninstall` deletes the program and removes its self from `PATH`

Here is an example of using judge:

```
judge run gen.exe sol.exe ref.exe -t 2.4
```

It also has many options as show below:

```
  -o, --output          Output cases as a file, and specify a path

  -a, --ac              (Default: false) Output AC cases along with WA cases

  -s, --shortcircuit    (Default: false) Short Circuit - Stop judging on non-AC

  -m, --mem             (Default: 512) Memory Limit in Megabytes

  -t, --time            (Default: 2) Time Limit in Seconds

  -c, --cases           (Default: 10) Cases

  -p, --parallel        (Default: 3) Parallel Judging Threads

  --help                Display this help screen.

  --version             Display version information.

  value pos. 0          Required. Specify Generator Path, or command to run the Generator enclosed in quotations.

  value pos. 1          Required. Solution Path, or command to run the Solution enclosed in quotations.

  value pos. 2          Required. Reference Solution Path, or command to run the Reference Solution enclosed in
                        quotations.
```

