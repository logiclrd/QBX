using QBX.ExecutionEngine.Compiled.Statements;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace QBX.ExecutionEngine.Execution;

public class DataParser
{
	public List<IEnumerable<string>> DataSources = new List<IEnumerable<string>>();
	public Dictionary<string, int> Labels = new Dictionary<string, int>();

	IEnumerator<IEnumerable<string>>? _dataSourceEnumerator = null;
	IEnumerator<string>? _currentDataSource;

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
		bool haveCurrent = false;

		while ((_currentDataSource == null) || !(haveCurrent = _currentDataSource.MoveNext()))
		{
			if (_dataSourceEnumerator == null)
				Restart();

			if (!_dataSourceEnumerator.MoveNext())
				throw RuntimeException.OutOfData(statement);

			_currentDataSource = _dataSourceEnumerator.Current.GetEnumerator();
		}

		if (!haveCurrent)
				throw RuntimeException.OutOfData(statement);

		return _currentDataSource.Current;
	}
}
