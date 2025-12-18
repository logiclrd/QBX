namespace QBX.CodeModel;

public class EndSubStatement : Statement
{
	public override void Render(TextWriter writer)
	{
		writer.Write("END SUB");
	}
}
