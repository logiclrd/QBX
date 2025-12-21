using QBX.CodeModel.Expressions;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class EventControlStatement : Statement
{
	public override StatementType Type => StatementType.EventControl;

	public EventType EventType { get; set; }
	public Expression? SourceExpression { get; set; }
	public EventControlAction Action { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		bool needSourceExpression = true;

		switch (EventType)
		{
			case EventType.Com: writer.Write("COM"); break;
			case EventType.Key: writer.Write("KEY"); break;
			case EventType.Timer: writer.Write("TIMER"); needSourceExpression = false; break;

			default: throw new Exception("Internal error: unrecognized EventType");
		}

		if (needSourceExpression)
		{
			if (SourceExpression == null)
				throw new Exception("Internal error: Missing required SourceExpression");

			writer.Write('(');
			SourceExpression.Render(writer);
			writer.Write(')');
		}
		else if (SourceExpression != null)
			throw new Exception("Internal error: Unnecessary SourceExpression present");

		switch (Action)
		{
			case EventControlAction.Enable: writer.Write(" ON"); break;
			case EventControlAction.Disable: writer.Write(" OFF"); break;
			case EventControlAction.Suspend: writer.Write(" STOP"); break;

			default: throw new Exception("Internal error: unrecognized TimerAction");
		}
	}
}
