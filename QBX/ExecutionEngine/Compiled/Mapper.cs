using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine.Compiled;

public class Mapper
{
	Mapper? _root;
	Dictionary<string, LiteralValue> _constantValueByName = new Dictionary<string, LiteralValue>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, int> _variableIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
	int _nextVariableIndex;
	HashSet<string> _disallowedSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	Dictionary<int, VariableInfo> _variables = new Dictionary<int, VariableInfo>();
	HashSet<string> _globalVariableNames = new HashSet<string>();
	PrimitiveDataType[] _identifierTypes = new PrimitiveDataType[26];

	// Slugs: avoid conflicts to do with dotted variable names.
	//
	// QuickBASIC allows an identifier to contain a dot in its name:
	//
	//    foo.bar ' means the scalar variable named "foo.bar"
	//
	// QuickBASIC also allows user-defined types to be defined, and dots are
	// used to access their fields:
	//
	//    TYPE test
	//      bar AS INTEGER
	//    END TYPE
	//
	//    DIM foo AS test
	//
	//    foo.bar ' means field bar of foo
	//
	// I have decided to call the part of an identifier that includes a
	// period that preceds the first period the "slug". So if "foo.bar"
	// is an identifier then "foo" is its slug.
	//
	// When parsing a compilation unit, any variables specified in DIM
	// statements with user-defined types are added to the _disallowedSlugs
	// set. When a variable is being implicitly defined, its slug is
	// extracted and an error is raised if that slug is disallowed.
	//
	// QuickBASIC applies this logic to arrays of user-defined types as
	// well, even though there isn't a possibility of collision.

	class VariableInfo(string name, int index)
	{
		public string Name => name;
		public int Index => index;
		public DataType Type = DataType.Integer;
		public int LinkedToRootVariableIndex = -1;
	}

	public Mapper()
	{
		_identifierTypes.AsSpan().Fill(PrimitiveDataType.Single);
		_nextVariableIndex = 0;

		DeclareVariable("@ExitCode", DataType.Long);
	}

	Mapper(Mapper root)
	{
		_root = root;
		_identifierTypes.AsSpan().Fill(PrimitiveDataType.Single);
		_constantValueByName = new Dictionary<string, LiteralValue>(root._constantValueByName);
	}

	public void MakeGlobalVariable(string identifier)
	{
		if (_root != null)
			throw new Exception("Can only make global variables working with the root Mapper");

		identifier = QualifyIdentifier(identifier);

		_globalVariableNames.Add(identifier);
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

		if (first == '@') // special case for @ExitCode
			return PrimitiveDataType.Long;
		else
		{
			int index = (first - 'A');

			return _identifierTypes[index];
		}
	}

	public DataType GetVariableType(int variableIndex)
		=> _variables[variableIndex].Type;

	public string StripTypeCharacter(string identifier)
	{
		if ((identifier.Length > 1) && CodeModel.TypeCharacter.TryParse(identifier.Last(), out _))
			return identifier.Remove(identifier.Length - 1);
		else
			return identifier;
	}

	public string QualifyIdentifier(string name, PrimitiveDataType type)
	{
		switch (name[name.Length - 1])
		{
			case '%':
				if (type != PrimitiveDataType.Integer)
					throw new Exception("Internal error: Trying to qualify " + name + " as " + type);
				return name;
			case '&':
				if (type != PrimitiveDataType.Long)
					throw new Exception("Internal error: Trying to qualify " + name + " as " + type);
				return name;
			case '!':
				if (type != PrimitiveDataType.Single)
					throw new Exception("Internal error: Trying to qualify " + name + " as " + type);
				return name;
			case '#':
				if (type != PrimitiveDataType.Double)
					throw new Exception("Internal error: Trying to qualify " + name + " as " + type);
				return name;
			case '@':
				if (type != PrimitiveDataType.Currency)
					throw new Exception("Internal error: Trying to qualify " + name + " as " + type);
				return name;
			case '$':
				if (type != PrimitiveDataType.String)
					throw new Exception("Internal error: Trying to qualify " + name + " as " + type);
				return name;

			default:
			{
				switch (type)
				{
					case PrimitiveDataType.Integer: return name + '%';
					case PrimitiveDataType.Long: return name + '&';
					case PrimitiveDataType.Single: return name + '!';
					case PrimitiveDataType.Double: return name + '#';
					case PrimitiveDataType.String: return name + '$';
					case PrimitiveDataType.Currency: return name + '@';
				}

				break;
			}
		}

		throw new Exception("Internal error");
	}

	public string QualifyIdentifier(string name, DataType type)
	{
		if (type.IsUserType)
			return name;

		return QualifyIdentifier(name, type.PrimitiveType);
	}

	public string QualifyIdentifier(string name)
	{
		if (CodeModel.TypeCharacter.TryParse(name.Last(), out var typeCharacter))
			return name;

		return QualifyIdentifier(name, GetTypeForIdentifier(name));
	}

	public Mapper CreateScope()
	{
		if (_root != null)
			throw new InvalidOperationException("Cannot create a mapper scope off of a scope");

		return new Mapper(this);
	}

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

	string? GetSlug(string identifier)
	{
		int dotIndex = identifier.IndexOf('.');

		if (dotIndex >= 0)
			return identifier.Substring(0, dotIndex);
		else
			return null;
	}

	public void AddDisallowedSlug(string slug)
	{
		_disallowedSlugs.Add(slug);
	}

	public bool IsDisallowedSlug(string identifier)
	{
		return _disallowedSlugs.Contains(identifier);
	}

	public void ScanForDisallowedSlugs(IEnumerable<CodeModel.Statements.Statement> statements)
	{
		foreach (var dimStatement in statements.OfType<CodeModel.Statements.DimStatement>())
		{
			foreach (var declaration in dimStatement.Declarations)
			{
				if (declaration.UserType != null)
					AddDisallowedSlug(declaration.Name);
			}
		}
	}

	public void DefineConstant(string name, LiteralValue literalValue)
	{
		if ((GetSlug(name) is string slug)
		 && _disallowedSlugs.Contains(slug))
			throw CompilerException.IdentifierCannotIncludePeriod(default);

		name = QualifyIdentifier(name);

		if (_constantValueByName.TryGetValue(name, out _))
			throw CompilerException.DuplicateDefinition(default(Token));
		if (_variableIndexByName.TryGetValue(name, out var index))
			throw CompilerException.DuplicateDefinition(default(Token));

		_constantValueByName[name] = literalValue;
	}

	public bool TryResolveConstant(string name, [NotNullWhen(true)] out LiteralValue? literalValue)
		=> _constantValueByName.TryGetValue(QualifyIdentifier(name), out literalValue);

	public int DeclareVariable(string name, DataType dataType, Token? token = null)
	{
		if ((GetSlug(name) is string slug)
		 && _disallowedSlugs.Contains(slug))
			throw CompilerException.IdentifierCannotIncludePeriod(token);

		name = QualifyIdentifier(name, dataType);

		if (_constantValueByName.TryGetValue(name, out _))
			throw CompilerException.DuplicateDefinition(token);
		if (_variableIndexByName.TryGetValue(name, out var index))
			throw CompilerException.DuplicateDefinition(token);

		index = _nextVariableIndex++;

		var info = new VariableInfo(name, index);

		info.Type = dataType;

		_variables[index] = info;

		return _variableIndexByName[name] = index;
	}

	public int ResolveVariable(string name)
	{
		int index;

		if (_variableIndexByName.TryGetValue(name, out index))
			return index;

		name = QualifyIdentifier(name);

		if (_variableIndexByName.TryGetValue(name, out index))
			return index;

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
