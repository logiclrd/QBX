using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware.Fonts;
using QBX.Numbers;
using QBX.OperatingSystem;
using QBX.Parser;

namespace QBX.ExecutionEngine.Execution;

public class DataParser
{
	public List<ItemParser> DataSources = new List<ItemParser>();
	public Dictionary<Identifier, int> Labels = new Dictionary<Identifier, int>();

	IEnumerator<ItemParser>? _dataSourceEnumerator = null;
	List<byte> _newLineBytes = new List<byte>();
	IEnumerator<string>? _currentDataSource;
	ItemParser? _currentDataSourceParser;
	bool _haveNext;

	public void AddDataSource(ItemParser dataSource)
	{
		DataSources.Add(dataSource);
	}

	public void AddLabel(LabelStatement? labelStatement)
	{
		if (labelStatement != null)
			AddLabel(labelStatement.LabelName);
	}

	public void AddLabel(Identifier label)
	{
		Labels[label] = DataSources.Count;
	}

	[MemberNotNull(nameof(_dataSourceEnumerator))]
	public void Restart()
	{
		_currentDataSourceParser?.Restart();

		_currentDataSource = null;
		_currentDataSourceParser = null;
		_dataSourceEnumerator = DataSources.GetEnumerator();
	}

	public bool TryGetLineNumber(Identifier label, out int lineNumber) => Labels.TryGetValue(label, out lineNumber);

	[MemberNotNull(nameof(_dataSourceEnumerator))]
	public void RestartAtLine(Identifier label)
	{
		if (!Labels.TryGetValue(label, out var lineNumber))
			throw new Exception("Internal error: trying to restart DATA at non-existent line number " + lineNumber);

		_currentDataSource = null;

		_dataSourceEnumerator = DataSources.Skip(lineNumber).GetEnumerator();
	}

	[MemberNotNull(nameof(_dataSourceEnumerator))]
	public void RestartAtLine(int lineNumber)
	{
		_currentDataSource = null;

		_dataSourceEnumerator = DataSources.Skip(lineNumber).GetEnumerator();
	}

	[MemberNotNull(nameof(_dataSourceEnumerator))]
	public void RestartFromFile(OpenFile openFile, DOS dos)
	{
		IEnumerable<ItemParser> ReadAndParseLines()
		{
			while (true)
			{
				_newLineBytes.Clear();

				var line = openFile.ReadLine(dos, _newLineBytes);

				if (line == null)
					break;

				yield return ParseDataItems(line.ToString());
			}
		}

		_dataSourceEnumerator = ReadAndParseLines().GetEnumerator();

		_currentDataSource = null;
	}

	public string GetNextDataItem(CodeModel.Statements.Statement? statement)
	{
		string? ret = _haveNext ? _currentDataSource?.Current : null;

		_haveNext = false;

		while ((ret == null) || !_haveNext)
		{
			while ((_currentDataSource == null) || !_currentDataSource.MoveNext())
			{
				if (_dataSourceEnumerator == null)
					Restart();

				if (!_dataSourceEnumerator.MoveNext())
				{
					if (ret != null)
						return ret;

					throw RuntimeException.OutOfData(statement);
				}

				_currentDataSourceParser?.Restart();

				_currentDataSourceParser = _dataSourceEnumerator.Current;
				_currentDataSource = _currentDataSourceParser.GetEnumerator();
			}

			if (ret == null)
				ret = _currentDataSource.Current;
			else
				_haveNext = true;
		}

		return ret;
	}

	public void ReadBytes(Span<byte> span, CodeModel.Expressions.Expression? expression)
	{
		while (span.Length > 0)
		{
			if ((_currentDataSourceParser == null) || !_currentDataSourceParser.HasMoreBytes)
			{
				if (_newLineBytes.Count > 0)
				{
					int numNewLineBytes = Math.Min(_newLineBytes.Count, span.Length);

					_newLineBytes.CopyTo(span.Slice(0, numNewLineBytes));
					_newLineBytes.RemoveRange(0, numNewLineBytes);

					span = span.Slice(numNewLineBytes);

					if (span.Length == 0)
						break;
				}

				if (_dataSourceEnumerator == null)
					Restart();

				if (!_dataSourceEnumerator.MoveNext())
					throw RuntimeException.OutOfData(expression);

				_currentDataSourceParser?.Restart();

				_currentDataSourceParser = _dataSourceEnumerator.Current;
				_currentDataSource = _currentDataSourceParser.GetEnumerator();

				continue;
			}

			int numRead = _currentDataSourceParser.ReadBytes(span);

			span = span.Slice(numRead);

			_haveNext = false;

			if (!_currentDataSourceParser.HasMoreBytes)
			{
				_currentDataSource = null;
				_currentDataSourceParser = null;
			}
		}
	}

	public string ReadLine(CodeModel.Statements.Statement? statement)
	{
		if (_currentDataSource == null)
		{
			if (_dataSourceEnumerator == null)
				Restart();

			if (!_dataSourceEnumerator.MoveNext())
				throw RuntimeException.OutOfData(statement);

			_currentDataSourceParser = _dataSourceEnumerator.Current;
		}

		string line = _currentDataSourceParser?.ReadToEndOfString() ?? "";

		_currentDataSourceParser?.Restart();

		_currentDataSource = null;
		_currentDataSourceParser = null;
		_haveNext = false;

		return line;
	}

	public bool IsAtStart => _currentDataSource == null;
	public bool IsAtEnd
	{
		get
		{
			if (_dataSourceEnumerator == null)
				Restart();

			return (_dataSourceEnumerator != null) && !_haveNext;
		}
	}

	public void ReadDataItems(IEnumerable<Evaluable> targetExpressions, ExecutionContext context, StackFrame stackFrame, CodeModel.Statements.Statement? source = null)
	{
		foreach (var targetExpression in targetExpressions)
			ReadDataItem(targetExpression, context, stackFrame, source);
	}

	public void ReadDataItem(Evaluable targetExpression, ExecutionContext context, StackFrame stackFrame, CodeModel.Statements.Statement? source = null)
	{
		var valueString = GetNextDataItem(source);

		var targetVariable = targetExpression.Evaluate(context, stackFrame);

		if (targetVariable.DataType.IsString)
			targetVariable.SetData(new StringValue(valueString));
		else if (!targetVariable.DataType.IsNumeric)
			throw RuntimeException.TypeMismatch(targetExpression.Source);
		else
		{
			switch (targetVariable)
			{
				case IntegerVariable targetIntegerVariable:
					if (NumberParser.TryAsInteger(valueString, out targetIntegerVariable.Value))
					{
						targetVariable.WritePinnedData();
						return;
					}
					break;
				case LongVariable targetLongVariable:
					if (NumberParser.TryAsLong(valueString, out targetLongVariable.Value))
					{
						targetVariable.WritePinnedData();
						return;
					}
					break;
				case SingleVariable targetSingleVariable:
					if (NumberParser.TryAsSingle(valueString, out targetSingleVariable.Value))
					{
						targetVariable.WritePinnedData();
						return;
					}
					break;
				case DoubleVariable targetDoubleVariable:
					if (NumberParser.TryAsDouble(valueString, out targetDoubleVariable.Value))
					{
						targetVariable.WritePinnedData();
						return;
					}
					break;
				case CurrencyVariable targetCurrencyVariable:
					if (NumberParser.TryAsCurrency(valueString, out targetCurrencyVariable.Value))
					{
						targetVariable.WritePinnedData();
						return;
					}
					break;
			}

			if (!NumberParser.TryParse(valueString, out var value))
				throw RuntimeException.SyntaxError(targetExpression.Source);

			targetVariable.SetData(value);
		}
	}

	public static ItemParser ParseDataItems(string rawString)
		=> new ItemParser(rawString);

	public class ItemParser : IEnumerable<string>
	{
		ReadOnlyMemory<char> _initialDataMemory;
		ReadOnlyMemory<char> _dataMemory;
		ReadOnlyMemory<char> _nextDataMemory;

		public bool HasMoreBytes => _dataMemory.Length > 0;

		public ItemParser(string rawString)
		{
			_initialDataMemory = rawString.AsMemory();

			Restart();
		}

		public void Restart()
		{
			_dataMemory = _initialDataMemory;
			_nextDataMemory = _dataMemory;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

		public string ReadToEndOfString()
		{
			string str = _dataMemory.ToString();

			_dataMemory = ReadOnlyMemory<char>.Empty;
			_nextDataMemory = _dataMemory;

			return str;
		}

		public IEnumerator<string> GetEnumerator()
		{
			bool yieldEmptyString = true;

			while (_dataMemory.Length > 0)
			{
				_nextDataMemory = _dataMemory;

				var dataSpan = _nextDataMemory.Span;

				while ((dataSpan.Length > 0) && char.IsWhiteSpace(dataSpan[0]))
				{
					_nextDataMemory = _nextDataMemory.Slice(1);
					dataSpan = _nextDataMemory.Span;
				}

				if (dataSpan.Length == 0)
					break;

				if (dataSpan[0] == '"')
				{
					int closeQuote = dataSpan.Slice(1).IndexOf('"') + 1;

					if (closeQuote < 0)
					{
						yieldEmptyString = false;
						yield return new string(dataSpan.Slice(1));
						break;
					}

					int nextField = closeQuote + 1;

					while ((nextField < dataSpan.Length) && char.IsWhiteSpace(dataSpan[nextField]))
						nextField++;

					if ((nextField < dataSpan.Length) && (dataSpan[nextField] != ','))
						throw RuntimeException.SyntaxError(default);

					nextField++;

					yieldEmptyString = false;

					if (nextField > _nextDataMemory.Length)
						nextField = _nextDataMemory.Length;

					_nextDataMemory = _nextDataMemory.Slice(nextField);

					yield return new string(dataSpan.Slice(1, closeQuote - 1));
				}
				else
				{
					int scanIndex = 0;

					var token = ReadOnlySpan<char>.Empty;

					while (true)
					{
						int nextCharacterOfInterestIndex = dataSpan.Slice(scanIndex).IndexOfAny(',', '"');

						if (nextCharacterOfInterestIndex < 0)
						{
							token = dataSpan;
							yieldEmptyString = false;
							break;
						}

						char ch = dataSpan[scanIndex + nextCharacterOfInterestIndex];

						if (ch == '"')
						{
							int endQuoteIndex = dataSpan.Slice(scanIndex + nextCharacterOfInterestIndex + 1).IndexOf('"');

							if (endQuoteIndex < 0)
							{
								// Dangling string; consume to end
								token = dataSpan;
								yieldEmptyString = false;
								break;
							}
							else
								scanIndex = scanIndex + nextCharacterOfInterestIndex + 1 + endQuoteIndex + 1;
						}
						else if (ch == ',')
						{
							token = dataSpan.Slice(0, scanIndex + nextCharacterOfInterestIndex);
							yieldEmptyString = true;
							break;
						}
					}

					if (token.Length < _nextDataMemory.Length)
						_nextDataMemory = _nextDataMemory.Slice(token.Length + 1);
					else
						_nextDataMemory = Memory<char>.Empty;

					while ((token.Length > 0) && char.IsWhiteSpace(token[token.Length - 1]))
						token = token.Slice(0, token.Length - 1);

					yield return new string(token);
				}

				_dataMemory = _nextDataMemory;
			}

			if (yieldEmptyString)
				yield return "";
		}

		public int ReadBytes(Span<byte> buffer)
		{
			// Doing this invalidates the enumerator's Current value and rereads bytes that
			// were read. It is up to the caller to account for this.

			int numRead;

			for (numRead = 0; numRead < buffer.Length; numRead++)
			{
				if (_dataMemory.Length == 0)
					break;

				buffer[numRead] = CP437Encoding.GetByteSemantic(_dataMemory.Span[0]);

				_dataMemory = _dataMemory.Slice(1);
			}

			_nextDataMemory = _dataMemory;

			return numRead;
		}
	}
}
