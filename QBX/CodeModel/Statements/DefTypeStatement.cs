using QBX.CodeModel;

namespace QBX.CodeModel.Statements;

internal class DefTypeStatement : Statement
{
	public override StatementType Type => StatementType.DefType;

	public DataType DataType { get; set; }
	public char? RangeStart { get; set; }
	public char? RangeEnd { get; set; }

	public override void Render(TextWriter writer)
	{
		switch (DataType)
		{
			case DataType.CURRENCY: writer.Write("DEFCUR"); break;
			case DataType.DOUBLE: writer.Write("DEFDBL"); break;
			case DataType.INTEGER: writer.Write("DEFINT"); break;
			case DataType.LONG: writer.Write("DEFLNG"); break;
			case DataType.SINGLE: writer.Write("DEFSNG"); break;
			case DataType.STRING: writer.Write("DEFSTR"); break;

			default: throw new Exception("Internal error: unrecognized data type for DEFtype");
		}

		if (RangeStart.HasValue)
		{
			writer.Write(" {0}", char.ToUpperInvariant(RangeStart.Value));

			if (RangeEnd.HasValue)
				writer.Write("-{0}", char.ToUpperInvariant(RangeEnd.Value));
		}
	}
}
