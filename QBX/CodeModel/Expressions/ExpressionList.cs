using System;
using System.Collections.Generic;
using System.IO;

using QBX.CodeModel.Statements;

namespace QBX.CodeModel.Expressions;

public class ExpressionList : IRenderableCode
{
	public List<Expression> Expressions { get; set; } = new List<Expression>();

	public int Count => Expressions.Count;

	public Expression this[int index] => Expressions[index];

	List<ParameterRepresentation>? _representations;

	public ParameterRepresentation GetRepresentation(int index)
	{
		if ((index < 0) || (index >= Count))
			throw new ArgumentOutOfRangeException(nameof(index));

		if ((_representations != null)
		 && (index < _representations.Count))
			return _representations[index];

		return ParameterRepresentation.Standard;
	}

	public void SetRepresentation(int index, ParameterRepresentation representation)
	{
		if ((index < 0) || (index >= Count))
			throw new ArgumentOutOfRangeException(nameof(index));

		if (_representations == null)
			_representations = new List<ParameterRepresentation>();

		while (index >= _representations.Count)
			_representations.Add(ParameterRepresentation.Standard);

		_representations[index] = representation;
	}

	public void ClaimTokens(Statement owner)
	{
		foreach (var expression in Expressions)
			expression.ClaimTokens(owner);
	}

	public void Render(TextWriter writer)
	{
		for (int i=0; i < Expressions.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			if ((_representations != null)
			 && (i < _representations.Count))
			{
				switch (_representations[i])
				{
					case ParameterRepresentation.BYVAL: writer.Write("BYVAL "); break;
					case ParameterRepresentation.SEG: writer.Write("SEG "); break;
				}
			}

			Expressions[i].Render(writer);
		}
	}
}
