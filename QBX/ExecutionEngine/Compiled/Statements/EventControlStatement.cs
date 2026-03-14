using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Events;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class EventControlStatement(CodeModel.Statements.EventControlStatement source) : Executable(source)
{
	public EventType EventType;
	public Evaluable? SourceExpression;
	public EventEnabledState EnabledState;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		Lazy<int> source = new Lazy<int>(
			() =>
			{
				if (SourceExpression == null)
					throw new Exception("EventControlStatement with no SourceExpression");

				return SourceExpression.EvaluateAndCoerceToInt(context, stackFrame);
			});

		EventConfigurationChange change;

		switch (EventType)
		{
			case EventType.Key:
				change = EventConfigurationChange.EnableDisableKey(source.Value, EnabledState);
				break;
			case EventType.Pen:
				change = EventConfigurationChange.EnableDisablePen(EnabledState);
				break;
			case EventType.Play:
				change = EventConfigurationChange.EnableDisablePlay(EnabledState);
				break;
			case EventType.Timer:
				change = EventConfigurationChange.EnableDisableTimer(EnabledState);
				break;
			case EventType.UserEvent:
				change = EventConfigurationChange.EnableDisableUserEvents(EnabledState);
				break;

			default: throw new Exception("Internal error");
		}

		context.EventHub.DispatchConfigurationChange(change);
	}
}
