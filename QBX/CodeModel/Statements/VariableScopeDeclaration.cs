using System.IO;

namespace QBX.CodeModel;

public class VariableScopeDeclaration : VariableDeclarationBase
{
	public bool IsArray { get; set; }

	public override bool HasSubscripts => IsArray;

	protected override void RenderArraySubscripts(TextWriter writer)
	{
		if (IsArray)
			writer.Write("()");
	}
}
