namespace QBX.CodeModel.Statements;

public class CommentStatement(CommentStatementType type, string comment) : Statement
{
	public override StatementType Type => StatementType.Comment;

	public CommentStatementType CommentStatementType { get; set; } = type;
	public string Comment { get; set; } = comment;

	public override void Render(TextWriter writer)
	{
		if (CommentStatementType == CommentStatementType.REM)
		{
			writer.Write("REM");

			if (Comment.Length > 0 && !char.IsWhiteSpace(Comment[0]))
				writer.Write(' ');
		}
		else
			writer.Write('\'');

		writer.Write(Comment);
	}
}
