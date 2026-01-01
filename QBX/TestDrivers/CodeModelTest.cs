using QBX.LexicalAnalysis;
using QBX.Parser;

using System.Diagnostics;

namespace QBX.TestDrivers;

public class CodeModelTest : HostedProgram
{
	public override bool EnableMainLoop => false;

	public override void Run(CancellationToken cancellationToken)
	{
		using (var reader1 = new StreamReader("../../../../Samples/NIBBLES.BAS"))
		using (var reader2 = new StreamReader("../../../../Samples/NIBBLES.BAS"))
		{
			var lexer = new Lexer(reader1);

			var parser = new BasicParser();

			int lineNumber = 1;

			foreach (var line in parser.ParseCodeLines(lexer))
			{
				var buffer = new StringWriter();

				line.Render(buffer);

				string originalLine = reader2.ReadLine() ?? throw new Exception("Unexpected EOF");
				string recreatedLine = buffer.ToString();

				// CodeLine renders the EOL.
				originalLine += "\r\n";

				Console.WriteLine(recreatedLine.TrimEnd());

				if (recreatedLine.TrimEnd() != originalLine.TrimEnd())
				{
					Console.WriteLine("ORIGINAL:");
					Console.WriteLine(originalLine);

					Debugger.Break();
				}

				lineNumber++;

				if (lineNumber == -510)
					Debugger.Break();
			}
		}
	}
}
