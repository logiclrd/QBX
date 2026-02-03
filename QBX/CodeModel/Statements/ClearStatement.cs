using QBX.CodeModel.Expressions;
using System.IO;

namespace QBX.CodeModel.Statements;

public class ClearStatement : Statement
{
	public override StatementType Type => StatementType.Clear;

	public Expression? StringSpaceExpression;
	public Expression? MaximumMemoryAddressExpression;
	public Expression? StackSpaceExpression;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("CLEAR");

		if ((StringSpaceExpression != null)
		 || (MaximumMemoryAddressExpression != null)
		 || (StackSpaceExpression != null))
		{
			writer.Write(' ');
			StringSpaceExpression?.Render(writer);

			if ((MaximumMemoryAddressExpression != null)
			 || (StackSpaceExpression != null))
			{
				writer.Write(", ");
				MaximumMemoryAddressExpression?.Render(writer);

				if (StackSpaceExpression != null)
				{
					writer.Write(", ");
					StackSpaceExpression.Render(writer);
				}
			}
		}
	}
}
