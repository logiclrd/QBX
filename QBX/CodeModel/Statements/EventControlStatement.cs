using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class EventControlStatement : Statement
{
	public override StatementType Type => StatementType.EventControl;

	public EventType EventType { get; set; }
	public Expression? SourceExpression { get; set; }
	public EventControlAction Action { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		bool needSourceExpression = false;

		switch (EventType)
		{
			case EventType.Com: writer.Write("COM"); needSourceExpression = true; break;
			case EventType.Key: writer.Write("KEY"); break;
			case EventType.Pen: writer.Write("PEN"); break;
			case EventType.Play: writer.Write("PLAY"); break;
			case EventType.OS2Signal: writer.Write("SIGNAL"); needSourceExpression = true; break;
			case EventType.JoystickTrigger: writer.Write("STRIG"); needSourceExpression = true; break;
			case EventType.Timer: writer.Write("TIMER"); break;
			case EventType.UserDefinedEvent: writer.Write("UEVENT"); break;

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
