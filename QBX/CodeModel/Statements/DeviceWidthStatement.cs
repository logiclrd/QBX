using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class DeviceWidthStatement : Statement
{
	public override StatementType Type => StatementType.DeviceWidth;

	public Expression? DeviceExpression { get; set; }
	public Expression? WidthExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if ((DeviceExpression == null) || (WidthExpression == null))
			throw new Exception("Internal error: DeviceWidthStatement with a missing Device or Width expression");

		writer.Write("WIDTH ");
		DeviceExpression.Render(writer);
		writer.Write(", ");
		WidthExpression.Render(writer);
	}
}
