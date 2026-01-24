using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class SleepStatement : Statement
{
	public override StatementType Type => StatementType.Sleep;

	public Expression? Seconds { get; set; }

	public SleepStatement()
	{
	}

	public SleepStatement(Expression seconds)
	{
		Seconds = seconds;
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("SLEEP");

		if (Seconds != null)
		{
			writer.Write(' ');
			Seconds.Render(writer);
		}
	}
}
