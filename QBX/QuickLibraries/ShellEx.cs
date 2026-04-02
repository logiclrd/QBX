using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Events;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.OperatingSystem;
using QBX.Utility;

namespace QBX.QuickLibraries;

[QuickLibraryName("SHELLEX.QLB")]
public class ShellEx : QuickLibrary
{
	public ShellEx(Machine machine, EventHub eventHub)
	{
	}

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	[Export(Name = "SHELL$")]
	public StringValue Shell(StringValue commandLine)
	{
		string commandLineString = commandLine.ToString();

		if (string.IsNullOrWhiteSpace(commandLineString))
			return new StringValue();

		ShellStatement.BuildShellExecuteCommand(
			commandLine.ToString(),
			out var shell,
			out var arguments);

		var result = new StringValue();
		var sync = new Lock();

		void HandleOutput(object sender, DataReceivedEventArgs e)
		{
			if (e.Data is string outputLine)
			{
				Span<byte> crlf = stackalloc byte[2];

				crlf[0] = 13;
				crlf[1] = 10;

				lock (sync)
					result.Append(outputLine).Append(crlf);
			}
		}

		var psi = new ProcessStartInfo(shell, arguments);

		psi.UseShellExecute = false;
		psi.RedirectStandardOutput = true;
		psi.RedirectStandardError = true;

		var process = new Process() { StartInfo = psi };

		process.OutputDataReceived += HandleOutput;
		process.ErrorDataReceived += HandleOutput;

		try
		{
			process.Start();

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			process.WaitForExit();

			return result;
		}
		catch (Win32Exception ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), expression: null);
		}
		catch
		{
			throw RuntimeException.IllegalFunctionCall();
		}
	}
}
