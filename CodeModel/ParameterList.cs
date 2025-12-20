namespace QBX.CodeModel;

public class ParameterList : IRenderableCode
{
	public List<ParameterDefinition> Parameters { get; } = new List<ParameterDefinition>();

	public void Render(TextWriter writer)
	{
		writer.Write(" (");

		for (int i = 0; i < Parameters.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Parameters[i].Render(writer);
		}

		writer.Write(")");
	}
}
