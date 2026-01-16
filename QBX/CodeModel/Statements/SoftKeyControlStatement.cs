using System.IO;

namespace QBX.CodeModel.Statements;

public class SoftKeyControlStatement : Statement
{
	public override StatementType Type => StatementType.SoftKeyControl;

	public bool Enable;

	protected override void RenderImplementation(TextWriter writer)
	{
		if (Enable)
			writer.Write("KEY ON");
		else
			writer.Write("KEY OFF");
	}
}
