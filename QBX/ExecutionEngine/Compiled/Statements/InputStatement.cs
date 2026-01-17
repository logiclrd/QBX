using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class InputStatement(string promptString, CodeModel.Statements.Statement source) : Executable(source)
{
	public List<Evaluable> TargetExpressions = new List<Evaluable>();

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		while (true)
		{
			try
			{
				context.VisualLibrary.WriteText(promptString);

				var inputLine = context.VisualLibrary.ReadLine(context.Machine.Keyboard);

				var parser = new DataParser();

				parser.AddDataSource(
					DataParser.ParseDataItems(inputLine));

				parser.ReadDataItems(TargetExpressions, context, stackFrame, source);

				if (!parser.IsAtEnd)
					throw new Exception();

				break;
			}
			catch
			{
				context.VisualLibrary.NewLine();
				context.VisualLibrary.WriteText("Redo from start");
				context.VisualLibrary.NewLine();

				continue;
			}
		}
	}
}
