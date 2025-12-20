using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PixelSetStatement : Statement
{
	public override StatementType Type => StatementType.PixelSet;

	public PixelSetDefaultColour DefaultColour { get; set; }
	public bool StepCoordinates { get; set; }
	public Expression? XExpression { get; set; }
	public Expression? YExpression { get; set; }
	public Expression? ColourExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		switch (DefaultColour)
		{
			case PixelSetDefaultColour.Foreground: writer.Write("PSET "); break;
			case PixelSetDefaultColour.Background: writer.Write("PRESET "); break;

			default: throw new Exception("Internal error: Unrecognized PixelSetDefaultColour value");
		}

		if (StepCoordinates)
			writer.Write("STEP");

		writer.Write('(');
		XExpression!.Render(writer);
		writer.Write(", ");
		YExpression!.Render(writer);
		writer.Write(')');

		if (ColourExpression != null)
		{
			writer.Write(", ");
			ColourExpression.Render(writer);
		}
	}
}
