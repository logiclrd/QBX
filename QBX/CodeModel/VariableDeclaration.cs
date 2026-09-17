using System.IO;

namespace QBX.CodeModel;

public class VariableDeclaration : VariableDeclarationBase
{
	public VariableDeclarationSubscriptList? Subscripts { get; set; }

	public override bool HasSubscripts => Subscripts != null;

	public int NumberOfDimensions => Subscripts?.Count ?? 0;

	protected override void RenderArraySubscripts(TextWriter writer)
	{
		Subscripts?.Render(writer);
	}
}
