using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class CloseArgument(bool numberSign, Expression fileNumberExpression) : IRenderableCode
{
	public bool NumberSign => numberSign;
	public Expression FileNumberExpression => fileNumberExpression;

	public void Render(TextWriter writer)
	{
		if (numberSign)
			writer.Write('#');
		fileNumberExpression.Render(writer);
	}
}
