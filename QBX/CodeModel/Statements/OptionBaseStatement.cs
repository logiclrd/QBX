using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public class OptionBaseStatement : Statement
{
	public override StatementType Type => StatementType.OptionBase;

	public short ArrayBase
	{
		get;
		set
		{
			if ((value < 0) || (value > 1))
				throw new Exception("Invalid array base " + value);

			field = value;
		}
	}

	public OptionBaseStatement()
	{
	}

	public OptionBaseStatement(short arrayBase)
	{
		ArrayBase = arrayBase;
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("OPTION BASE ");
		writer.Write(ArrayBase);
	}
}
