using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class OnStatement : Statement
{
	public override StatementType Type => StatementType.On;

	public EventType EventType { get; set; }
	public Expression? SourceExpression { get; set; }
	public GoSubStatement? Action { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (EventType == EventType.Unknown)
			throw new Exception("Internal error: OnStatement with no EventType");
		if (Action == null)
			throw new Exception("Internal error: OnStatement with no Action");

		writer.Write("ON ");

		bool needSourceExpression = true;

		switch (EventType)
		{
			case EventType.Com: writer.Write("COM"); break;
			case EventType.Key: writer.Write("KEY"); break;
			case EventType.Pen: writer.Write("PEN"); needSourceExpression = false; break;
			case EventType.Play: writer.Write("PLAY"); break;
			case EventType.OS2Signal: writer.Write("SIGNAL"); break;
			case EventType.JoystickTrigger: writer.Write("STRIG"); break;
			case EventType.Timer: writer.Write("TIMER"); break;
			case EventType.UserDefinedEvent: writer.Write("UEVENT"); needSourceExpression = false; break;

			default: throw new Exception("Internal error: OnStatement with unrecognized EventType");
		}

		if (needSourceExpression)
		{
			if (SourceExpression == null)
				throw new Exception("Internal error: OnStatement missing required SourceExpression");

			writer.Write('(');
			SourceExpression.Render(writer);
			writer.Write(") ");
		}
		else if (SourceExpression != null)
			throw new Exception("Internal error: OnStatement has an unnecessary SourceExpression");

		Action.Render(writer);
	}
}
