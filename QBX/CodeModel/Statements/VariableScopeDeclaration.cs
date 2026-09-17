using System.IO;

namespace QBX.CodeModel;

public class VariableScopeDeclaration : VariableDeclarationBase
{
	public bool IsArray { get; set; }

	protected override void RenderArraySubscripts(TextWriter writer)
	{
		if (IsArray)
			writer.Write("()");
	}
}
