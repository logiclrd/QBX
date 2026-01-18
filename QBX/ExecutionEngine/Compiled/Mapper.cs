using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine.Compiled;

// This class is in charge of linking things up within a CompilationElement.
//
// - Variables:
//
//   The result of compilation has an array of nameless variables, and compiled
//   statements reference them by index. Mapper is in charge of tracking the
//   links from identifiers to variable indices during the compilation phase.
//
// - Arrays:
//
//   In QBASIC, arrays have a different namespace from variables. You can create
//   an array foo%(1 TO 5), and also set foo% = 3, these are separate things.
//   Mapper tracks a separate mapping of arrays to variable indices. The arrays
//   and non-arrays live in the same array, but the names are mapped
//   independently, so e.g. foo% can be variable 1 and foo%() can be variable 2.
//
// - Dotted identifiers:
//
//   For some legacy reason, QuickBASIC supports using identifiers that have
//   dots in the name. For instance, "foo.bar" is a perfectly valid variable
//   name. But, this conflicts with the existence of user data types. If
//   there were a type that had a field "bar", and "foo" was of that type,
//   then "foo.bar" would mean "field bar of variable foo". When such
//   conflicts arise, the field interpretation wins.
//
//   Mapper resolves this by tracking the "slugs", as I call them, of dotted
//   identifiers -- the part of the identifier up to the first dot. If a
//   variable is declared with a user data type, or a SUB or FUNCTION is
//   defined, with a particular name then that name becomes disallowed for
//   slugs in dotted identifiers.
//
// - Identifier types:
//
//   QuickBASIC allows the default data type of an identifier to be set
//   based on the first character of its name. The name can also be
//   qualified, overriding this default. The name without qualification is
//   an alias for the qualified name. So, e.g., if the default type is
//   SINGLE, then "a" and "a!" reference the same variable. You can still
//   also have "a%" as a separate variable. If the default type is INTEGER
//   then "a" references the same variable as "a%" instead.
//
//   These mappings can change in an ongoing basis based on the presence
//   of "DEFtype" statements ("DEFINT", "DEFSNG", etc.). Lines of code
//   after such a statement work with the updated mappings, until the next
//   "DEFtype" statement is encountered.
//
//   Mapper tracks these defaults and automatically qualifies unqualified
//   identifiers when defining and resolving them.

public class Mapper
{
	Mapper? _root;

	public readonly Routine Routine;

	Dictionary<string, LiteralValue> _constantValueByName = new Dictionary<string, LiteralValue>(StringComparer.OrdinalIgnoreCase);

	List<VariableInfo> _variables = new List<VariableInfo>();

	Dictionary<string, int> _variableIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, int> _arrayIndexByName = new Dictionary<string, int>(StringComparer.Ordinal);
	HashSet<string> _disallowedSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	HashSet<string> _globalVariableNames = new HashSet<string>();
	HashSet<string> _globalArrayNames = new HashSet<string>();

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

	public Mapper(Routine mainRoutine)
	{
		Routine = mainRoutine;

		_identifierTypes.AsSpan().Fill(PrimitiveDataType.Single);

		DeclareVariable("@ExitCode", DataType.Long);
	}

	Mapper(Mapper root, Routine subroutine)
	{
		_root = root;

		Routine = subroutine;

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

	public void MakeGlobalArray(string identifier, DataType type)
	{
		if (_root != null)
			throw new Exception("Can only make global variables working with the root Mapper");

		identifier = QualifyIdentifier(identifier, type);

		_globalArrayNames.Add(identifier);
	}

	public void LinkGlobalVariablesAndArrays()
	{
		if (_root == null)
			throw new InvalidOperationException("Cannot call LinkGlobalVariable on the root Mapper");

		foreach (string name in _root._globalVariableNames)
		{
			int rootIndex = _root.ResolveVariable(name);

			var variableType = _root.GetVariableType(rootIndex);

			int localIndex = ResolveVariable(name, variableType);

			var info = _variables[localIndex];

			info.LinkedToRootVariableIndex = rootIndex;
		}

		foreach (string name in _root._globalArrayNames)
		{
			int rootIndex = _root.ResolveArray(name, out _);

			var arrayType = _root.GetVariableType(rootIndex);

			int localIndex = ResolveArray(name, out _, arrayType);

			var info = _variables[localIndex];

			info.LinkedToRootVariableIndex = rootIndex;
		}
	}

	Stack<PrimitiveDataType[]> _identifierTypesStack = new Stack<PrimitiveDataType[]>();

	public void PushIdentifierTypes()
	{
		var saved = new PrimitiveDataType[_identifierTypes.Length];

		_identifierTypes.CopyTo(saved);

		_identifierTypesStack.Push(saved);
	}

	public void PopIdentifierTypes()
	{
		var saved = _identifierTypesStack.Pop();

		saved.CopyTo(_identifierTypes);
	}

	public void ApplyDefTypeStatement(CodeModel.Statements.DefTypeStatement defTypeStatement)
	{
		var dataType = DataType.FromCodeModelDataType(defTypeStatement.DataType);

		if (!dataType.IsPrimitiveType)
			throw new Exception("DefTypeStatement's DataType is not a primitive type");

		foreach (var range in defTypeStatement.Ranges)
			SetIdentifierTypes(range.Start, range.End ?? range.Start, dataType.PrimitiveType);
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

	public string UnqualifyIdentifier(string name)
	{
		if (CodeModel.TypeCharacter.TryParse(name[name.Length - 1], out _))
			name = name.Remove(name.Length - 1);

		return name;
	}

	public Mapper CreateScope(Routine subroutine)
	{
		if (_root != null)
			throw new InvalidOperationException("Cannot create a mapper scope off of a scope");

		return new Mapper(this, subroutine);
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
	{
		if (_constantValueByName.TryGetValue(QualifyIdentifier(name), out literalValue))
			return true;
		else if (_root != null)
			return _root.TryResolveConstant(name, out literalValue);
		else
		{
			literalValue = default;
			return false;
		}
	}

	enum SemiscopeMode
	{
		Inactive,
		Setup,
		Active,
	}

	SemiscopeMode _semiscopeMode;
	Dictionary<string, int>? _semiscopeOverlay;

	public void StartSemiscopeSetup()
	{
		_semiscopeMode = SemiscopeMode.Setup;
		_semiscopeOverlay = new Dictionary<string, int>();
	}

	public void EnterSemiscope()
	{
		_semiscopeMode = SemiscopeMode.Active;
	}

	public void ExitSemiscope()
	{
		_semiscopeMode = SemiscopeMode.Inactive;
		_semiscopeOverlay = null;
	}

	public int DeclareVariable(string name, DataType dataType, Token? token = null)
	{
		if ((GetSlug(name) is string slug)
		 && _disallowedSlugs.Contains(slug))
			throw CompilerException.IdentifierCannotIncludePeriod(token);

		name = QualifyIdentifier(name, dataType);

		if (_constantValueByName.TryGetValue(name, out _))
			throw CompilerException.DuplicateDefinition(token);
		if ((_root != null)
		 && _root._constantValueByName.TryGetValue(name, out _))
			throw CompilerException.DuplicateDefinition(token);

		// During semiscope setup, we allow new declarations to shadow
		// existing ones for DEF FN parameters.
		if (_semiscopeMode != SemiscopeMode.Setup)
		{
			if (_variableIndexByName.TryGetValue(name, out _))
				throw CompilerException.DuplicateDefinition(token);
		}

		int index = _variables.Count;

		var info = new VariableInfo(name, index);

		info.Type = dataType;

		_variables.Add(info);

		if (_semiscopeMode != SemiscopeMode.Setup)
			return _variableIndexByName[name] = index;
		else
			return _semiscopeOverlay![name] = index;
	}

	public int ResolveVariable(string name, DataType? dataType = null)
	{
		int index;

		if (_variableIndexByName.TryGetValue(name, out index))
			return index;
		if ((_semiscopeOverlay != null)
		 && _semiscopeOverlay.TryGetValue(name, out index))
			return index;

		if (dataType == null)
		{
			name = QualifyIdentifier(name);

			if (_variableIndexByName.TryGetValue(name, out index))
				return index;
			if ((_semiscopeOverlay != null)
			 && _semiscopeOverlay.TryGetValue(name, out index))
				return index;
		}

		return DeclareVariable(name, dataType ?? DataType.ForPrimitiveDataType(GetTypeForIdentifier(name)));
	}

	public int DeclareArray(string name, DataType dataType, Token? token = null)
	{
		name = QualifyIdentifier(name, dataType);

		if (_arrayIndexByName.TryGetValue(name, out var index))
			throw CompilerException.DuplicateDefinition(token);

		index = _variables.Count;

		var info = new VariableInfo(name, index);

		info.Type = dataType;

		_variables.Add(info);

		return _arrayIndexByName[name] = index;
	}

	public int ResolveArray(string name, out bool implicitlyCreated, DataType? arrayType = null)
	{
		implicitlyCreated = false;

		int index;

		if (_arrayIndexByName.TryGetValue(name, out index))
			return index;

		if (arrayType == null)
		{
			name = QualifyIdentifier(name);

			if (_arrayIndexByName.TryGetValue(name, out index))
				return index;
		}

		implicitlyCreated = true;

		if (arrayType == null)
		{
			var elementType = DataType.ForPrimitiveDataType(GetTypeForIdentifier(name));

			arrayType = elementType.MakeArrayType();
		}

		return DeclareArray(name, arrayType);
	}

	public List<DataType> GetVariableTypes() =>
		Enumerable.Range(0, _variables.Count)
		.Select(idx => _variables[idx].Type)
		.ToList();

	public List<VariableLink> GetLinkedVariables() =>
		_variables
		.Where(info => info.LinkedToRootVariableIndex >= 0)
		.Select(info =>
			new VariableLink()
			{
				LocalIndex = info.Index,
				RootIndex = info.LinkedToRootVariableIndex,
			})
		.ToList();
}
