using QBX.Parser;
using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

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
		if (e.Exception is SyntaxErrorException)
		{
			bool suppress = false;

			foreach (var frame in new StackTrace().GetFrames())
			{
				var method = frame.GetMethod();

				if ((method != null)
				 && (method.DeclaringType == typeof(BasicParser))
				 && method.Name.StartsWith("Test"))
				{
					suppress = true;
					break;
				}
			}

			if (!suppress)
				Debugger.Break();
		}
	}
}
