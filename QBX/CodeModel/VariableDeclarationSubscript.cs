using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel;

public class VariableDeclarationSubscript : IRenderableCode
{
	public Expression? Bound1 { get; set; }
	public Expression? Bound2 { get; set; }

	public void Render(TextWriter writer)
	{
		if (Bound1 == null)
			throw new Exception("Internal error: Variable declaration subscript bound not set");

		Bound1.Render(writer);

		if (Bound2 != null)
		{
			writer.Write(" TO ");
			Bound2.Render(writer);
		}
	}
}
