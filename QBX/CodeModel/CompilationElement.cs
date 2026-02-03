using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using QBX.CodeModel.Statements;

namespace QBX.CodeModel;

public class CompilationElement(CompilationUnit owner) : IRenderableCode
{
	public CompilationUnit Owner => owner;

	public string? Name { get; set; }

	public CompilationElementType Type { get; set; }
	public IReadOnlyList<CodeLine> Lines => _lines;

	public int CachedCursorLine; // Used by DevelopmentEnvironment

	public IEnumerable<Statement> AllStatements => _lines.SelectMany(line => line.Statements);

	List<CodeLine> _lines = new List<CodeLine>();

	public CompilationElement Clone()
	{
		var clone = new CompilationElement(owner);

		clone.Type = Type;
		clone.AddLines(Lines);

		return clone;
	}

	public void Dirty()
	{
		owner.IsPristine = false;
	}

	public void AddLine(CodeLine line)
	{
		_lines.Add(line);
		line.CompilationElement = this;
	}

	public void AddLines(IEnumerable<CodeLine> lines)
	{
		_lines.AddRange(lines);

		foreach (var line in lines)
			line.CompilationElement = this;
	}

	public void InsertLine(int index, CodeLine line)
	{
		_lines.Insert(index, line);
		line.CompilationElement = this;

		for (int lineNumber = index; lineNumber < _lines.Count; lineNumber++)
			_lines[lineNumber].SourceLineIndex.Value = lineNumber;
	}

	public void ReplaceLine(int index, CodeLine newLine)
	{
		if ((index < 0) || (index >= _lines.Count))
			throw new ArgumentOutOfRangeException(nameof(index));

		_lines[index].CompilationElement = null;

		_lines[index] = newLine;

		newLine.CompilationElement = this;
		newLine.SourceLineIndex.Value = index;
	}

	public void RemoveLineAt(int index)
	{
		if ((index >= 0) && (index < _lines.Count))
		{
			_lines[index].CompilationElement = null;
			_lines.RemoveAt(index);

			for (int lineNumber = index; lineNumber < _lines.Count; lineNumber++)
				_lines[lineNumber].SourceLineIndex.Value = lineNumber;
		}
	}

	public static DataType[] MakeDefaultDefTypeMap()
	{
		var identifierTypes = new DataType[26];

		identifierTypes.AsSpan().Fill(DataType.SINGLE);

		return identifierTypes;
	}

	public void ApplyDefTypeStatements(DataType[] identifierTypes, bool stopAtSubroutineOpeningStatement)
	{
		if (identifierTypes.Length != 26)
			throw new Exception("Internal error: identifierTypes array is not of the correct length");

		foreach (var statement in AllStatements)
		{
			if (stopAtSubroutineOpeningStatement
			 && (statement is SubroutineOpeningStatement))
				break;

			if (statement is DefTypeStatement defType)
			{
				foreach (var range in defType.Ranges)
				{
					char ch = range.Start;

					do
					{
						int idx = ch - 'A';

						identifierTypes[idx] = defType.DataType;

						ch++;
					} while (ch <= range.End);
				}
			}
		}
	}

	public void RewriteDefTypeStatements(DataType[] oldRelativeTo, DataType[] newRelativeTo)
	{
		var identifierTypes = (DataType[])oldRelativeTo.Clone();

		ApplyDefTypeStatements(identifierTypes, stopAtSubroutineOpeningStatement: true);

		RemoveAllDefTypeStatements();

		PrependDefTypeStatement(identifierTypes, newRelativeTo, DataType.STRING);
		PrependDefTypeStatement(identifierTypes, newRelativeTo, DataType.CURRENCY);
		PrependDefTypeStatement(identifierTypes, newRelativeTo, DataType.DOUBLE);
		PrependDefTypeStatement(identifierTypes, newRelativeTo, DataType.SINGLE);
		PrependDefTypeStatement(identifierTypes, newRelativeTo, DataType.LONG);
		PrependDefTypeStatement(identifierTypes, newRelativeTo, DataType.INTEGER);
	}

	void RemoveAllDefTypeStatements()
	{
		for (int lineIndex = 0; lineIndex < Lines.Count; lineIndex++)
		{
			var line = Lines[lineIndex];

			for (int statementIndex = 0; statementIndex < line.Statements.Count; statementIndex++)
			{
				switch (line.Statements[statementIndex])
				{
					case SubroutineOpeningStatement:
						return;

					case DefTypeStatement:
						line.RemoveStatementAt(statementIndex);
						statementIndex--;
						break;
				}
			}

			if ((line.Statements.Count == 0) && (line.EndOfLineComment == null))
			{
				_lines.RemoveAt(lineIndex);
				lineIndex--;
			}
		}
	}

	void PrependDefTypeStatement(DataType[] types, DataType[] relativeTo, DataType statementType)
	{
		var defType = new DefTypeStatement();

		defType.DataType = statementType;

		for (int i=0; i < types.Length; i++)
			if ((types[i] == statementType) && (types[i] != relativeTo[i]))
				defType.AddCharacter((char)(i + 'A'));

		if (defType.Ranges.Any())
		{
			var line = new CodeLine();

			line.AppendStatement(defType);

			InsertLine(0, line);
		}
	}

	public void Render(TextWriter writer)
	{
		_lines.ForEach(line => line.Render(writer));
	}
}
