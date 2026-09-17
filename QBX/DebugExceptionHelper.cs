using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;

using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX;

class DebugExceptionHelper
{
	// Some aspects of parsing are implemented by attempting a particular interpretation and
	// suppressing any resulting exceptions. To assist in debugging, the main debugger
	// exception filter can be configured to ignore SyntaxErrorException exceptions, and
	// then the handler installed by this class manually breaks on SyntaxErrorExceptions
	// that are not thrown in one of the interpretation-testing contexts.

	public static void Install()
	{
		AppDomain.CurrentDomain.FirstChanceException += AppDomain_FirstChanceException;
	}

	private static void AppDomain_FirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
	{
		bool suppress;

		if (e.Exception.Message.StartsWith("Internal"))
			suppress = false;
		else
		{
			suppress = false;

			bool scanForParserTestMethod = false;
			bool scanForIDELoadMethod = false;

			switch (e.Exception)
			{
				case GoTo:
				case Resume:
				case ExitDo:
				case ExitFor:
				case ExitScope:
				case BreakExecution:
				case EndProgram:
				case TerminatedException:
					suppress = true;
					break;

				case SyntaxErrorException:
					scanForParserTestMethod = true;
					break;
				case IOException:
					scanForIDELoadMethod = true;
					break;
				case RuntimeException error:
					switch (error.ErrorNumber)
					{
						case 12: // Invalid access
						case 53: // File not found
						case 57: // Device IO error
						case 67: // Too many open files
						case 75: // Path/file access error
						case 76: // Path not found
							scanForIDELoadMethod = true;
							break;
					}

					break;
			}

			if (scanForParserTestMethod || scanForIDELoadMethod)
			{
				foreach (var frame in new StackTrace().GetFrames())
				{
					var method = frame.GetMethod();

					if (method != null)
					{
						if (scanForParserTestMethod
						 && (method.DeclaringType == typeof(BasicParser))
						 && method.Name.StartsWith("Test"))
							suppress = true;

						if (scanForIDELoadMethod
						 && (method.DeclaringType == typeof(DevelopmentEnvironment.Program))
						 && method.Name.StartsWith("Load"))
							suppress = true;

						if (suppress)
							break;
					}
				}
			}
		}

		if (!suppress)
			Debugger.Break();
	}
}
