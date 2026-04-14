using System;
using System.Collections.Generic;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ComputedBranchStatement : Statement
{
	public override StatementType Type => StatementType.ComputedBranch;

	public Expression? Expression;
	public ComputedBranchType BranchType;
	public List<ComputedBranchTarget> Targets = new List<ComputedBranchTarget>();

	protected override void RenderImplementation(TextWriter writer)
	{
		if (Expression == null)
			throw new Exception("Internal error: ComputedBranchStatement with no Expression");
		if (!Enum.IsDefined(BranchType))
			throw new Exception("Internal error: ComputedBranchStatement with invalid BranchType");
		if (Targets.Count == 0)
			throw new Exception("Internal error: ComputedBranchStatement with no Targets");

		writer.Write("ON ");
		Expression.Render(writer);

		switch (BranchType)
		{
			case ComputedBranchType.GoTo: writer.Write(" GOTO "); break;
			case ComputedBranchType.GoSub: writer.Write(" GOSUB "); break;
		}

		for (int i = 0; i < Targets.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Targets[i].Render(writer);
		}
	}
}
