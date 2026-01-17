using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Execution;

public class DataParser
{
	public List<IEnumerable<string>> DataSources = new List<IEnumerable<string>>();
	public Dictionary<string, int> Labels = new Dictionary<string, int>();

	IEnumerator<IEnumerable<string>>? _dataSourceEnumerator = null;
	IEnumerator<string>? _currentDataSource;
	bool _haveNext;

	public void AddDataSource(IEnumerable<string> dataSource)
	{
		DataSources.Add(dataSource);
	}

	public void AddLabel(LabelStatement? labelStatement)
	{
		if (labelStatement != null)
			AddLabel(labelStatement.LabelName);
	}

	public void AddLabel(string label)
	{
		Labels[label] = DataSources.Count;
	}

	[MemberNotNull(nameof(_dataSourceEnumerator))]
	public void Restart()
	{
		_currentDataSource = null;
		_dataSourceEnumerator = DataSources.GetEnumerator();
	}

	public bool TryGetLineNumber(string label, out int lineNumber) => Labels.TryGetValue(label, out lineNumber);

	[MemberNotNull(nameof(_dataSourceEnumerator))]
	public void RestartAtLine(string label)
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

	public string GetNextDataItem(CodeModel.Statements.Statement? statement)
	{
		string? ret = _haveNext ? _currentDataSource?.Current : null;

		_haveNext = false;

		while ((ret == null) || !_haveNext)
		{
			bool haveCurrent = false;

			while ((_currentDataSource == null) || !(haveCurrent = _currentDataSource.MoveNext()))
			{
				if (_dataSourceEnumerator == null)
					Restart();

				if (!_dataSourceEnumerator.MoveNext())
				{
					if (ret != null)
						return ret;

					throw RuntimeException.OutOfData(statement);
				}

				_currentDataSource = _dataSourceEnumerator.Current.GetEnumerator();
			}

			if (ret == null)
				ret = _currentDataSource.Current;
			else
				_haveNext = true;
		}

		return ret;
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
			throw RuntimeException.TypeMismatch(targetExpression.SourceExpression?.Token);
		else
		{
			switch (targetVariable)
			{
				case IntegerVariable targetIntegerVariable:
					if (NumberParser.TryAsInteger(valueString, out targetIntegerVariable.Value))
						return;
					break;
				case LongVariable targetLongVariable:
					if (NumberParser.TryAsLong(valueString, out targetLongVariable.Value))
						return;
					break;
				case SingleVariable targetSingleVariable:
					if (NumberParser.TryAsSingle(valueString, out targetSingleVariable.Value))
						return;
					break;
				case DoubleVariable targetDoubleVariable:
					if (NumberParser.TryAsDouble(valueString, out targetDoubleVariable.Value))
						return;
					break;
				case CurrencyVariable targetCurrencyVariable:
					if (NumberParser.TryAsCurrency(valueString, out targetCurrencyVariable.Value))
						return;
					break;
			}

			if (!NumberParser.TryParse(valueString, out var value))
				throw RuntimeException.SyntaxError(targetExpression.SourceExpression?.Token);

			targetVariable.SetData(value);
		}
	}


	public static IEnumerable<string> ParseDataItems(string rawString)
	{
		var dataMemory = rawString.AsMemory();

		bool yieldEmptyString = true;

		while (dataMemory.Length > 0)
		{
			var dataSpan = dataMemory.Span;

			while ((dataSpan.Length > 0) && char.IsWhiteSpace(dataSpan[0]))
			{
				dataMemory = dataMemory.Slice(1);
				dataSpan = dataMemory.Span;
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
				yield return new string(dataSpan.Slice(1, closeQuote - 1));

				if (nextField > dataMemory.Length)
					break;

				dataMemory = dataMemory.Slice(nextField);
			}
			else
			{
				int comma = dataSpan.IndexOf(',');

				var token = (comma >= 0)
					? dataSpan.Slice(0, comma)
					: dataSpan;

				while ((token.Length > 0) && char.IsWhiteSpace(token[token.Length - 1]))
					token = token.Slice(0, token.Length - 1);

				yieldEmptyString = (comma >= 0);
				yield return new string(token);

				dataMemory = dataMemory.Slice((comma >= 0) ? comma + 1 : dataMemory.Length);
			}
		}

		if (yieldEmptyString)
			yield return "";
	}
}
