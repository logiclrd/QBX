using QBX.ExecutionEngine.Execution;
using QBX.LexicalAnalysis;
using System;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class RestoreStatement(Module module, CodeModel.Statements.Statement source) : Executable(source), IUnresolvedLineReference
{
	public string? LabelName { get; set; }

	public Token? LabelToken { get; set; }

	int _lineNumber;

	public void Resolve(Routine routine)
	{
		if (LabelName != null)
		{
			if (!module.DataParser.TryGetLineNumber(LabelName, out _lineNumber))
				throw CompilerException.LabelNotDefined(LabelToken);
		}
	}

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (LabelName != null)
			module.DataParser.RestartAtLine(_lineNumber);
		else
			module.DataParser.Restart();
	}
}
