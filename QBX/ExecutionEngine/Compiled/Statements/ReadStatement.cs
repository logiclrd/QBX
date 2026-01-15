using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ReadStatement(Module module, CodeModel.Statements.Statement? source) : Executable(source)
{
	public List<Evaluable> TargetExpressions = new List<Evaluable>();

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		foreach (var targetExpression in TargetExpressions)
		{
			var valueString = module.DataParser.GetNextDataItem(source);

			var targetVariable = targetExpression.Evaluate(context, stackFrame);

			if (targetVariable.DataType.IsString)
				targetVariable.SetData(new StringValue(valueString));
			else
			{
				if (!NumberParser.TryParse(valueString, out var value))
					throw RuntimeException.SyntaxError(targetExpression.SourceExpression?.Token);

				targetVariable.SetData(value);
			}
		}
	}
}
