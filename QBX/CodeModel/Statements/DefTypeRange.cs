using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public class DefTypeRange : IComparable<DefTypeRange>, IRenderableCode
{
	public char Start;
	public char? End;

	public void Normalize()
	{
		if (Start == End)
			End = null;
		else if (End.HasValue && (Start > End))
			(Start, End) = (End.Value, Start);
	}

	internal void Validate()
	{
		if (!char.IsAsciiLetterUpper(Start))
			throw new Exception("Internal error: DefTypeRange with invalid Start value");

		if (End.HasValue)
		{
			if (!char.IsAsciiLetterUpper(End.Value))
				throw new Exception("Internal error: DefTypeRange with invalid End value");

			if (End < Start)
				throw new Exception("Internal error: DefTypeRange with out-of-order Start/End");
		}
	}

	public bool OverlapsWith(DefTypeRange other)
	{
		Validate();
		other.Validate();

		if (!End.HasValue && !other.End.HasValue)
			return Math.Abs(Start - other.Start) <= 1;

		if (!End.HasValue)
			return (Start >= other.Start - 1) && (Start <= other.End!.Value + 1);
		if (!other.End.HasValue)
			return (other.Start >= Start - 1) && (other.Start <= End.Value + 1);

		return (End.Value >= other.Start - 1) && (other.End.Value >= Start - 1);
	}

	public void Merge(DefTypeRange other)
	{
		Validate();
		other.Validate();

		void Throw() => throw new InvalidOperationException("Ranges are not adjacent or overlapping");

		char newStart;
		char? newEnd;

		if (!End.HasValue && !other.End.HasValue)
		{
			newStart = Start;
			newEnd = other.Start;

			if (newEnd < newStart)
				(newStart, newEnd) = (newEnd.Value, newStart);

			if (newEnd - newStart > 1)
				Throw();

			if (newEnd == newStart)
				newEnd = null;
		}
		else if (!End.HasValue || !other.End.HasValue)
		{
			int merge;

			if (!End.HasValue)
			{
				newStart = other.Start;
				newEnd = other.End!.Value;
				merge = this.Start;
			}
			else
			{
				newStart = this.Start;
				newEnd = this.End.Value;
				merge = other.Start;
			}

			if (merge == newStart - 1)
				newStart--;
			else if (merge == newEnd + 1)
				newEnd++;
			else if (!((merge >= newStart) && (merge <= newEnd)))
				Throw();
		}
		else
		{
			newStart = other.Start;
			newEnd = other.End!.Value;

			if ((newStart > Start) && (newEnd < End))
				(newStart, newEnd) = (Start, End.Value);
			else if ((newStart > Start) || (newEnd < End))
			{
				if ((Start > newEnd + 1) || (End < newStart - 1))
					Throw();

				if (Start >= newStart)
					newEnd = End.Value;
				else
					newStart = Start;
			}
		}

		Start = newStart;
		End = newEnd;
	}

	public int CompareTo(DefTypeRange? other)
	{
		if (other == null)
			throw new InvalidOperationException("Internal error: Comparing a DefTypeRange to null");

		Validate();
		other.Validate();

		return Start - other.Start;
	}

	public void Render(TextWriter writer)
	{
		Validate();

		writer.Write(char.ToUpper(Start));

		if (End.HasValue && (End.Value != Start))
		{
			if (!char.IsLetter(End.Value))
				throw new Exception("Internal error: DefTypeRange with a non-letter End");
			if (End.Value < Start)
				throw new Exception("Internal error: DefTypeRange with out-of-order Start/End");

			writer.Write('-');
			writer.Write(char.ToUpper(End.Value));
		}
	}
}
