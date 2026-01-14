using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PutSpriteStatement : Statement
{
	public override StatementType Type => StatementType.PutSprite;

	public bool Step { get; set; }
	public Expression? XExpression { get; set; }
	public Expression? YExpression { get; set; }

	public Expression? SourceExpression { get; set; }

	public PutSpriteAction? ActionVerb { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("PUT ");

		if (Step)
			writer.Write("STEP");

		if ((XExpression == null) || (YExpression == null))
			throw new Exception("Internal error: PutSpriteStatement missing one or both of the coordinate expressions");

		writer.Write('(');
		XExpression.Render(writer);
		writer.Write(", ");
		YExpression.Render(writer);
		writer.Write("), ");

		if (SourceExpression == null)
			throw new Exception("Internal error: GetSpriteStatement missing the Target expression");

		SourceExpression.Render(writer);

		if (ActionVerb != null)
		{
			writer.Write(", ");

			switch (ActionVerb)
			{
				case PutSpriteAction.PixelSet: writer.Write("PSET"); break;
				case PutSpriteAction.PixelSetInverted: writer.Write("PRESET"); break;
				case PutSpriteAction.And: writer.Write("AND"); break;
				case PutSpriteAction.Or: writer.Write("OR"); break;
				case PutSpriteAction.ExclusiveOr: writer.Write("XOR"); break;

				default: throw new Exception("Internal error: Unrecognized ActionVerb value");
			}
		}
	}
}
