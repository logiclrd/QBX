<img width="642" height="432" alt="image" src="https://github.com/user-attachments/assets/023d1b49-6c3f-4d84-bdc4-ac848a9405db" />

# QBX

QBX is an _environment emulator_ that aims to bring back the nostalgic feel of programming with QuickBASIC. It is a mostly-complete highly-compatible reimplementation of QuickBASIC 7.1 in cross-platform C# code targeting .NET 10. Fire up QBX and immediately that classic blue text editor and IDE, complete with all the old keyboard shortcuts and editing keys. Run and debug code, show watches and run statements in the `Immediate` window.

If you used QuickBASIC 7.1 back in the day, QBX will take you right back to those days (especially in full-screen mode! `Alt+Enter`). If you're new to classic BASIC, QBX lets you fiddle with and learn QuickBASIC the way you would have been able to in the early 1990s.

## Getting started

Before anything else, you will need to have .NET 10.0 installed. This can be downloaded from Microsoft:

* <https://dotnet.microsoft.com/en-us/download/dotnet/10.0>

Then, the easiest ways to run QBX are using a **Release build** or **From source** using the `dotnet` command-line interface.

### Release builds

_As August 2026, QBX release builds are available only for Windows._

QBX release builds are created and published within GitHub by a workflow action:

* <https://github.com/logiclrd/QBX/releases>

Download the latest release package, decompress it, and then double-click on the `QBX` program file / launch `QBX.exe`.

### From source

Download the QBX repository. This can be done either using Git by "cloning" the repository, or by requesting a download from GitHub. After you have unpacked the archive, navigate to the solution root, and then issue the following `dotnet` command:

* `dotnet run --project=QBX`

## Debugging

If you want to debug the QBX source code, there are (at least) two options.

### Visual Studio

On Windows, you can download the full-featured Visual Studio (current version 2026). For non-commercial use, Microsoft offers a free "Community" version. You can then load QBX.sln and launch the QBX project. There are aspects to the QBX code that make certain things difficult to debug conventionally, but for the majority of QBX functions, you can simply set a break point and step through.

### Visual Studio Code

The alternative text editor Visual Studio Code can also, with the right extensions, be used to debug the QBX source code. Ensure that you have the `C#` and `C# Dev Kit` extensions installed. Then open the QBX repository root as a workspace in Visual Studio Code. The simplest way to do this is to navigate to that directory in a shell and issue the command `code .`.

The C# debugging experience is not as comprehensive in Visual Studio Code as in the full Visual Studio, but it's still quite functional, and some of the QBX development was done using Visual Studio Code on Linux.

## Contributing

If you are interested in contributing to QBX, then the first step is to sign up for a GitHub account and _fork_ the repository. Within your fork, you can make branches and commit changes to them however you want. When you have a branch that contains a code change you want to propose, GitHub allows you to open a "pull request".

TL;DR: GitHub PRs welcome. :-)

### License

The QBX project is licensed under the Lesser GPL license. If you make contributions, your contributions also fall under this license. You automatically agree to this when you propose changes.