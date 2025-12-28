/*
 * TODO
 * 
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class DeviceWidthStatement
{
	public override StatementType Type => StatementType.UnresolvedWidth;

	public Expression? DeviceExpression { get; set; }
	public Expression? WidthExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		if ((DeviceExpression == null) || (WidthExpression == null))
			throw new Exception("Internal error: DeviceWidthStatement with a missing Device or Width expression");

		writer.Write("WIDTH ");
		DeviceExpression.Render(writer);
		writer.Write(", ");
		WidthExpression.Render(writer);
	}
}

*/
