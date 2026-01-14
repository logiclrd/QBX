using System.IO;

namespace QBX.CodeModel.Statements;

public abstract class ProperSubroutineOpeningStatement : SubroutineOpeningStatement
{
	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write(StatementName);
		writer.Write(' ');
		writer.Write(Name);

		Parameters?.Render(writer);

		if (IsStatic)
			writer.Write(" STATIC");
	}
}
