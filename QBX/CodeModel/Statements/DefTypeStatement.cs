using System;
using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class DefTypeStatement : Statement
{
	public override StatementType Type => StatementType.DefType;

	public DataType DataType { get; set; }
	public List<DefTypeRange> Ranges { get; } = new List<DefTypeRange>();

	public void AddRange(DefTypeRange range)
	{
		for (int i = 0; i < Ranges.Count; i++)
			if (range.OverlapsWith(Ranges[i]))
			{
				Ranges[i].Merge(range);

				while ((i + 1 < Ranges.Count) && Ranges[i + 1].OverlapsWith(Ranges[i]))
				{
					Ranges[i].Merge(Ranges[i + 1]);
					Ranges.RemoveAt(i);
				}

				return;
			}

		for (int i = 0; i < Ranges.Count; i++)
			if (Ranges[i].CompareTo(range) > 0)
			{
				Ranges.Insert(i, range);
				return;
			}

		Ranges.Add(range);
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		if (Ranges.Count == 0)
			throw new Exception("Internal error: DefTypeStatement with no ranges");

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

		writer.Write(' ');

		for (int i = 0; i < Ranges.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Ranges[i].Render(writer);
		}
	}
}
