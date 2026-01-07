using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class Mapper
{
	Mapper? _root;
	Dictionary<string, Routine> _subs;
	Dictionary<string, Routine> _functions;
	Dictionary<string, int> _variableIndexByName = new Dictionary<string, int>();
	int _nextVariableIndex;
	Dictionary<int, VariableInfo> _variables = new Dictionary<int, VariableInfo>();
	HashSet<string> _globalVariableNames = new HashSet<string>();
	PrimitiveDataType[] _identifierTypes = new PrimitiveDataType[26];

	class VariableInfo(string name, int index)
	{
		public string Name => name;
		public int Index => index;
		public DataType Type = DataType.Integer;
		public int LinkedToRootVariableIndex = -1;
	}

	public IEnumerable<Routine> AllRegisteredRoutines => _subs.Values.Concat(_functions.Values);

	public Mapper()
	{
		_subs = new Dictionary<string, Routine>();
		_functions = new Dictionary<string, Routine>();
		_identifierTypes.AsSpan().Fill(PrimitiveDataType.Single);
		_nextVariableIndex = 0;

		DeclareVariable("@ExitCode", DataType.Long);
	}

	Mapper(Mapper root)
	{
		_root = root;
		_subs = root._subs;
		_functions = root._functions;
		_identifierTypes.AsSpan().Fill(PrimitiveDataType.Single);
	}

	public void LinkGlobalVariables()
	{
		if (_root == null)
			throw new InvalidOperationException("Cannot call LinkGlobalVariable on the root Mapper");

		foreach (string name in _root._globalVariableNames)
		{
			int localIndex = ResolveVariable(name);
			int rootIndex = _root.ResolveVariable(name);

			var info = _variables[localIndex];

			info.LinkedToRootVariableIndex = rootIndex;
		}
	}

	public void SetIdentifierTypes(char from, char to, PrimitiveDataType type)
	{
		from = char.ToUpperInvariant(from);
		to = char.ToUpperInvariant(to);

		if (from < 'A')
			from = 'A';
		if (from > 'Z')
			from = 'Z';
		if (to < 'A')
			to = 'A';
		if (to > 'Z')
			to = 'Z';

		if (from > to)
			(from, to) = (to, from);

		int index = (from - 'A');
		int count = (to - from + 1);

		_identifierTypes.AsSpan().Slice(index, count).Fill(type);
	}

	public PrimitiveDataType GetTypeForIdentifier(string name)
	{
		if (CodeModel.TypeCharacter.TryParse(name.Last(), out var typeCharacter))
		{
			switch (typeCharacter.Type)
			{
				case CodeModel.DataType.INTEGER: return PrimitiveDataType.Integer;
				case CodeModel.DataType.LONG: return PrimitiveDataType.Long;
				case CodeModel.DataType.SINGLE: return PrimitiveDataType.Single;
				case CodeModel.DataType.DOUBLE: return PrimitiveDataType.Double;
				case CodeModel.DataType.STRING: return PrimitiveDataType.String;
				case CodeModel.DataType.CURRENCY: return PrimitiveDataType.Currency;

				default: throw new Exception("Unrecognized type " + typeCharacter.Type);
			}
		}

		char first = char.ToUpperInvariant(name.First());

		int index = (first - 'A');

		return _identifierTypes[index];
	}

	public string QualifyIdentifier(string name)
	{
		if (CodeModel.TypeCharacter.TryParse(name.Last(), out var typeCharacter))
			return name;

		switch (GetTypeForIdentifier(name))
		{
			case PrimitiveDataType.Integer: return name + '%';
			case PrimitiveDataType.Long: return name + '&';
			case PrimitiveDataType.Single: return name + '!';
			case PrimitiveDataType.Double: return name + '#';
			case PrimitiveDataType.String: return name + '$';
			case PrimitiveDataType.Currency: return name + '@';
		}

		throw new Exception("Internal error");
	}

	public Mapper CreateScope()
	{
		if (_root != null)
			throw new InvalidOperationException("Cannot create a mapper scope off of a scope");

		return new Mapper(this);
	}

	public bool IsRegistered(string name)
	{
		return _subs.ContainsKey(name) || _functions.ContainsKey(name);
	}

	public void RegisterSub(Routine routine)
	{
		_subs[routine.Name] = routine;
	}

	public void RegisterFunction(Routine routine)
	{
		_functions[routine.Name] = routine;
	}

	public bool TryGetRoutine(string routineName, [NotNullWhen(true)] out Routine? routine)
	{
		throw new NotImplementedException();
	}

	public bool TryGetSub(string name, [NotNullWhen(true)] out Routine? sub)
		=> _subs.TryGetValue(name, out sub);

	public bool TryGetFunction(string name, [NotNullWhen(true)] out Routine? function)
		=> _functions.TryGetValue(name, out function);

	public void LinkRootVariable(string name)
	{
		if (_root == null)
			throw new Exception("Cannot link to a root variable from the root");

		name = QualifyIdentifier(name);

		int localIndex = ResolveVariable(name);
		int rootIndex = _root.ResolveVariable(name);

		var variableInfo = _variables[localIndex];

		variableInfo.LinkedToRootVariableIndex = rootIndex;
	}

	public int DeclareVariable(string name, DataType dataType)
	{
		if (_variableIndexByName.TryGetValue(name, out var index))
			throw new Exception("Variable is already declared: " + name);

		index = _nextVariableIndex++;

		var info = new VariableInfo(name, index);

		info.Type = dataType;

		_variables[index] = info;

		return _variableIndexByName[name] = index;
	}

	public int ResolveVariable(string name)
	{
		name = QualifyIdentifier(name);

		if (_variableIndexByName.TryGetValue(name, out var index))
			return index;
		else
			return DeclareVariable(name, DataType.ForPrimitiveDataType(GetTypeForIdentifier(name)));
	}

	public List<DataType> GetVariableTypes() =>
		Enumerable.Range(0, _variables.Count)
		.Select(idx => _variables[idx].Type)
		.ToList();

	public List<VariableLink> GetLinkedVariables() =>
		_variables.Values
		.Where(info => info.LinkedToRootVariableIndex >= 0)
		.Select(info =>
			new VariableLink()
			{
				LocalIndex = info.Index,
				RootIndex = info.LinkedToRootVariableIndex,
			})
		.ToList();
}
