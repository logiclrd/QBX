using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class OpenStatement : Statement
{
	public override StatementType Type => StatementType.Open;

	public OpenMode OpenMode { get; set; }
	public AccessMode AccessMode { get; set; }
	public LockMode LockMode { get; set; }
	public Expression? FileNameExpression { get; set; }
	public Expression? FileNumberExpression { get; set; }
	public Expression? RecordLengthExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNameExpression == null)
			throw new Exception("Internal error: OpenStatement with no FileNameExpression");
		if (FileNumberExpression == null)
			throw new Exception("Internal error: OpenStatement with no FileNumberExpression");

		writer.Write("OPEN ");
		FileNameExpression.Render(writer);
		writer.Write(" FOR ");

		switch (OpenMode)
		{
			case OpenMode.Random: writer.Write("RANDOM"); break;
			case OpenMode.Binary: writer.Write("BINARY"); break;
			case OpenMode.Input: writer.Write("INPUT"); break;
			case OpenMode.Output: writer.Write("OUTPUT"); break;
			case OpenMode.Append: writer.Write("APPEND"); break;

			default: throw new Exception("Internal Error: Unrecognized OpenMode");
		}

		switch (AccessMode)
		{
			case AccessMode.Unspecified: break;
			case AccessMode.Read: writer.Write(" ACCESS READ"); break;
			case AccessMode.Write: writer.Write(" ACCESS WRITE"); break;
			case AccessMode.ReadWrite: writer.Write(" ACCESS READ WRITE"); break;

			default: throw new Exception("Internal error: Unrecognized AccessMode");
		}

		switch (LockMode)
		{
			case LockMode.None: break;
			case LockMode.Shared: writer.Write(" SHARED"); break;
			case LockMode.LockRead: writer.Write(" LOCK READ"); break;
			case LockMode.LockWrite: writer.Write(" LOCK WRITE"); break;
			case LockMode.LockReadWrite: writer.Write(" LOCK READ WRITE"); break;

			default: throw new Exception("Internal error: Unrecognized LockMode");
		}

		writer.Write(" AS #");
		FileNumberExpression.Render(writer);

		if (RecordLengthExpression != null)
		{
			writer.Write(" LEN = ");
			RecordLengthExpression.Render(writer);
		}
	}
}
