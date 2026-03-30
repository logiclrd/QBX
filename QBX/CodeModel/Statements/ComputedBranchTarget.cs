using System.IO;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class ComputedBranchTarget(Identifier labelName, Token token) : IRenderableCode
{
	public Identifier LabelName => labelName;
	public Token Token => token;

	public void Render(TextWriter writer)
	{
		writer.Write(labelName.Value);
	}
}
