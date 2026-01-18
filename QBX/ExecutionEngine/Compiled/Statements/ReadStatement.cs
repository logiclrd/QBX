using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ReadStatement(Module module, CodeModel.Statements.Statement? source) : Executable(source)
{
	public List<Evaluable> TargetExpressions = new List<Evaluable>();

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		module.DataParser.ReadDataItems(TargetExpressions, context, stackFrame, source);
	}
}
