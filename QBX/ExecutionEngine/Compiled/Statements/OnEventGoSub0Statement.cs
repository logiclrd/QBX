using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Events;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OnEventGoSub0Statement(CodeModel.Statements.Statement source) : Executable(source), IOnEventStatementConfigurator
{
	public EventType EventType;
	public Evaluable? SourceExpression;

	void IOnEventStatementConfigurator.SetEventType(EventType eventType)
		=> EventType = eventType;
	Evaluable? IOnEventStatementConfigurator.SetSourceExpression(Evaluable? sourceExpression)
		=> SourceExpression = sourceExpression;
	Executable IOnEventStatementConfigurator.AsExecutable() => this;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		int source = Event.NoSource;

		if (EventType == EventType.Key)
		{
			if (SourceExpression == null)
				throw new Exception("OnErrorGoSubLineStatement with no SourceExpression");

			source = SourceExpression.EvaluateAndCoerceToInt(context, stackFrame);
		}

		var evt = new Event(EventType, source);

		context.ClearEventHandler(evt);
	}
}
