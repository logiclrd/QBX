using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Events;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OnEventGoSubLineStatement(string target, CodeModel.Statements.Statement source)
	: JumpStatement(target, source), IOnEventStatementConfigurator
{
	public override bool TargetIsInMainModule => true;

	public EventType EventType;
	public Evaluable? SourceExpression;

	void IOnEventStatementConfigurator.SetEventType(EventType eventType)
		=> EventType = eventType;
	Evaluable? IOnEventStatementConfigurator.SetSourceExpression(Evaluable? sourceExpression)
		=> SourceExpression = sourceExpression;
	Executable IOnEventStatementConfigurator.AsExecutable() => this;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetPath == null)
			throw new Exception("Internal error: Executing an unresolved OnErrorGoSubLineStatement");

		int source = 0;

		if (EventType == EventType.Key)
		{
			if (SourceExpression == null)
				throw new Exception("OnErrorGoSubLineStatement with no SourceExpression");

			source = SourceExpression.EvaluateAndCoerceToInt(context, stackFrame);
		}

		var evt = new Event(EventType, source);

		context.SetEventHandler(evt, TargetPath);
	}
}
