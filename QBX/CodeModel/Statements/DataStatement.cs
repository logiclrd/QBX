using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public class DataStatement : Statement
{
	public override StatementType Type => StatementType.Data;

	public List<Token> DataItems { get; set; }

	public DataStatement(List<Token> dataItems)
	{
		DataItems = dataItems;
	}

	public override void Render(TextWriter writer)
	{
		writer.Write("DATA");

		if (DataItems.Count > 0)
		{
			writer.Write(' ');

			for (int i = 0; i < DataItems.Count; i++)
			{
				if (i > 0)
					writer.Write(',');

				writer.Write(DataItems[i].Value);
			}
		}
	}
}
