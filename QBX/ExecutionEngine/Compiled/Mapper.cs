using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.LexicalAnalysis;
using QBX.Parser;

using TypeCharacter = QBX.CodeModel.TypeCharacter;

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
//
// User-defined types:
//
//   User-defined types do not have any names, neither for the types nor
//   the fields, but within a compilation element, they are referenced
//   exclusively by name. Mapper is in charge of these "facades" that
//   assign names to UDTs.
//
//   Assignments and calls within a compilation element require the
//   facade to match exactly. Calls across module boundaries only require
//   the underlying user-defined type to match.

public class Mapper
{
	Module _module;
	Mapper? _moduleMapper;

	public Mapper ModuleMapper => _moduleMapper ?? this;

	public readonly Routine Routine;

	bool _isFrozen;

	public bool IsFrozen => _isFrozen;

	public void Freeze()
	{
		_isFrozen = true;
	}

	// Mappers are frozen when there could be stack frames already configured
	// based on them. If you unfreeze a Mapper, you are responsible for ensuring
	// relevant stack frames are correspondingly updated.
	public void Unfreeze()
	{
		_isFrozen = false;
	}

	Dictionary<string, LiteralValue> _constantValueByName = new(StringComparer.OrdinalIgnoreCase);
	HashSet<string> _hiddenConstants = new();

	List<VariableInfo> _variables = new List<VariableInfo>();

	Dictionary<string, int> _variableIndexByName = new(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, int> _arrayIndexByName = new(StringComparer.OrdinalIgnoreCase);
	HashSet<string> _disallowedSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	HashSet<int> _predeclaredArrayIndices = new HashSet<int>();

	Dictionary<string, int> _firstVariableByName = new(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, int> _firstArrayByName = new(StringComparer.OrdinalIgnoreCase);

	HashSet<string> _globalVariableNames = new HashSet<string>();
	HashSet<string> _globalArrayNames = new HashSet<string>();

	public IEnumerable<string> GlobalIdentifiers => _globalVariableNames.Concat(_globalArrayNames);

	PrimitiveDataType[] _identifierTypes = new PrimitiveDataType[26];

	Dictionary<string, DataType> _typeFacadeByName = new(StringComparer.OrdinalIgnoreCase);
	HashSet<string> _hiddenTypeFacades = new();

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

	class VariableInfo(string name, Token? nameToken, int index)
	{
		public string Name => name;
		public Token? NameToken => nameToken;
		public int Index => index;
		public DataType Type = DataType.Integer;

		public bool HasExplicitTypeClause = false;

		public bool IsStaticArray = false;
		public int NumberOfArrayDimensions = -1;

		public int LinkedToModuleVariableIndex = -1;

		public CommonBlock? LinkedToCommonBlock;
		public int LinkedToCommonBlockVariableIndex;

		public bool IsLinked => (LinkedToModuleVariableIndex >= 0) || (LinkedToCommonBlock != null);
	}

	public Mapper(Module module, Routine mainRoutine)
	{
		_module = module;

		Routine = mainRoutine;

		_identifierTypes.AsSpan().Fill(PrimitiveDataType.Single);

		DeclareVariable("@ExitCode", DataType.Long);
	}

	Mapper(Module module, Mapper moduleMapper, Routine subroutine)
	{
		_module = module;

		_moduleMapper = moduleMapper;

		Routine = subroutine;

		_identifierTypes.AsSpan().Fill(PrimitiveDataType.Single);
		_constantValueByName = new Dictionary<string, LiteralValue>(moduleMapper._constantValueByName);
	}

	public void MakeGlobalVariable(string identifier)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");
		if (_moduleMapper != null)
			throw new Exception("Can only make global variables working with the Module Mapper");

		_globalVariableNames.Add(identifier);
	}

	public void MakeGlobalArray(string identifier, DataType type)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");
		if (_moduleMapper != null)
			throw new Exception("Can only make global variables working with the Module Mapper");

		_globalArrayNames.Add(identifier);
	}

	public void MakeStaticArray(int variableIndex)
	{
		if (_variables[variableIndex].IsStaticArray)
			throw new Exception("Internal error: Making the same variable index a static array more than once");

		_variables[variableIndex].IsStaticArray = true;
	}

	public bool IsStaticArray(int variableIndex)
	{
		return _variables[variableIndex].IsStaticArray;
	}

	public void LinkGlobalVariablesAndArrays()
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");
		if (_moduleMapper == null)
			throw new InvalidOperationException("Cannot call LinkGlobalVariable on the Module Mapper");

		// If a SUB or FUNCTION declares a parameter with the same name as a SHARED variable/array,
		// then it occludes the link to the global scope.

		foreach (var name in _moduleMapper._globalVariableNames)
		{
			int moduleIndex = _moduleMapper.ResolveVariable(name);

			if (moduleIndex < 0)
				throw new Exception("Internal error: Failed to resolve global variable '" + name + "'");

			var moduleVariable = _moduleMapper.GetVariable(moduleIndex);

			if (_variableIndexByName.ContainsKey(moduleVariable.Name))
				continue;

			var localVariable = CreateLocalVariable(moduleVariable);

			localVariable.LinkedToModuleVariableIndex = moduleIndex;

			if (!UnderlayVariable(name, localVariable))
				_variables.Remove(localVariable);
		}

		foreach (var name in _moduleMapper._globalArrayNames)
		{
			int moduleIndex = _moduleMapper.ResolveArray(name, arrayType: null, numberOfDimensions: -1, implicitlyCreated: out _);

			if (moduleIndex < 0)
				throw new Exception("Internal error: Failed to resolve global array '" + name + "'");

			var moduleVariable = _moduleMapper.GetVariable(moduleIndex);

			if (_variableIndexByName.ContainsKey(moduleVariable.Name))
				continue;

			var localVariable = CreateLocalVariable(moduleVariable);

			localVariable.LinkedToModuleVariableIndex = moduleIndex;

			if (!UnderlayArray(name, localVariable))
				_variables.Remove(localVariable);
		}
	}

	VariableInfo GetVariable(int index)
		=> _variables[index];

	VariableInfo CreateLocalVariable(VariableInfo remoteVariable)
	{
		int index = _variables.Count;

		var localVariable = new VariableInfo(remoteVariable.Name, nameToken: null, index);

		localVariable.Type = remoteVariable.Type;
		localVariable.HasExplicitTypeClause = remoteVariable.HasExplicitTypeClause;
		localVariable.NumberOfArrayDimensions = remoteVariable.NumberOfArrayDimensions;

		_variables.Add(localVariable);

		return localVariable;
	}

	bool UnderlayVariable(string name, VariableInfo variable)
		=> Underlay(name, variable, _variableIndexByName);

	bool UnderlayArray(string name, VariableInfo variable)
		=> Underlay(name, variable, _arrayIndexByName);

	bool Underlay(string name, VariableInfo variable, Dictionary<string, int> mappingTable)
	{
		bool mapped = false;

		if (variable.Type.IsUserType || variable.HasExplicitTypeClause)
		{
			string unqualifiedName = UnqualifyIdentifier(name);

			if (!mappingTable.ContainsKey(unqualifiedName))
			{
				mappingTable[unqualifiedName] = variable.Index;
				mapped = true;
			}
		}

		if (variable.Type.IsPrimitiveType)
		{
			string qualifiedName = QualifyIdentifier(name);

			if (!mappingTable.ContainsKey(qualifiedName))
			{
				mappingTable[qualifiedName] = variable.Index;
				mapped = true;
			}
		}

		return mapped;
	}

	public bool IsLinkedVariable(string name, DataType dataType)
	{
		name = QualifyIdentifier(name, dataType);

		return
			_variableIndexByName.TryGetValue(name, out var index) &&
			(_variables[index].LinkedToModuleVariableIndex >= 0);
	}

	public bool IsLinkedArray(string name, DataType dataType)
	{
		name = QualifyIdentifier(name, dataType);

		return
			_arrayIndexByName.TryGetValue(name, out var index) &&
			(_variables[index].LinkedToModuleVariableIndex >= 0);
	}

	Stack<PrimitiveDataType[]> _identifierTypesStack = new Stack<PrimitiveDataType[]>();

	public void PushIdentifierTypes()
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		var saved = new PrimitiveDataType[_identifierTypes.Length];

		_identifierTypes.CopyTo(saved);

		_identifierTypesStack.Push(saved);
	}

	public void PopIdentifierTypes()
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		var saved = _identifierTypesStack.Pop();

		saved.CopyTo(_identifierTypes);
	}

	public void ApplyDefTypeStatement(CodeModel.Statements.DefTypeStatement defTypeStatement)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		var dataType = DataType.FromCodeModelDataType(defTypeStatement.DataType);

		if (!dataType.IsPrimitiveType)
			throw new Exception("DefTypeStatement's DataType is not a primitive type");

		foreach (var range in defTypeStatement.Ranges)
			SetIdentifierTypes(range.Start, range.End ?? range.Start, dataType.PrimitiveType);
	}

	public void SetIdentifierTypes(char from, char to, PrimitiveDataType type)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

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
		if (TypeCharacter.TryParse(name.Last(), out var typeCharacter))
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
			if (first == '<') // persistent storage for static variable
			{
				int end = name.IndexOf('>');

				if (end + 1 < name.Length)
					first = char.ToUpperInvariant(name[end + 1]); // fails gracefully if IndexOf doesn't find anything
			}

			int index = (first - 'A');

			return _identifierTypes[index];
		}
	}

	public PrimitiveDataType GetTypeForIdentifier(Identifier name)
	{
		if (name is QualifiedIdentifier qualifiedIdentifier)
			return GetPrimitiveDataType(qualifiedIdentifier.TypeCharacter);

		char first = char.ToUpperInvariant(name.Value.First());

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

	public static PrimitiveDataType GetPrimitiveDataType(TypeCharacter typeCharacter)
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

	public static TypeCharacter GetTypeCharacter(PrimitiveDataType primitiveType)
	{
		char ch;

		switch (primitiveType)
		{
			case PrimitiveDataType.Integer: ch = '%'; break;
			case PrimitiveDataType.Long: ch = '&'; break;
			case PrimitiveDataType.Single: ch = '!'; break;
			case PrimitiveDataType.Double: ch = '#'; break;
			case PrimitiveDataType.String: ch = '$'; break;
			case PrimitiveDataType.Currency: ch = '@'; break;

			default: throw new Exception("Unrecognized type " + primitiveType);
		}

		if (!TypeCharacter.TryParse(ch, out var typeCharacter))
			throw new Exception("Sanity failure");

		return typeCharacter;
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
		if (name.StartsWith("<")) // hidden variables for static scopes
			return name;

		if (CodeModel.TypeCharacter.TryParse(name.Last(), out var typeCharacter))
			return name;

		return QualifyIdentifier(name, GetTypeForIdentifier(name));
	}

	public static string UnqualifyIdentifier(string name)
	{
		if (CodeModel.TypeCharacter.TryParse(name[name.Length - 1], out _))
			name = name.Remove(name.Length - 1);

		return name;
	}

	public QualifiedIdentifier QualifyIdentifier(Identifier name, PrimitiveDataType type)
	{
		if (name is QualifiedIdentifier qualifiedIdentifier)
		{
			if (type != GetPrimitiveDataType(qualifiedIdentifier.TypeCharacter))
				throw new Exception("Internal error: Trying to qualify " + name + " as " + type);

			return qualifiedIdentifier;
		}

		var typeCharacter = GetTypeCharacter(type);

		return new QualifiedIdentifier(name, typeCharacter);
	}

	public Identifier QualifyIdentifier(Identifier name, DataType type)
	{
		if (type.IsUserType)
			return name;

		if (name is QualifiedIdentifier qualifiedIdentifier)
			return qualifiedIdentifier;
		else
			return QualifyIdentifier(name, type.PrimitiveType);
	}

	public QualifiedIdentifier QualifyIdentifier(Identifier name)
	{
		if (name is QualifiedIdentifier qualifiedIdentifier)
			return qualifiedIdentifier;

		return QualifyIdentifier(name, GetTypeForIdentifier(name));
	}

	public static Identifier UnqualifyIdentifier(Identifier name)
	{
		if (name is QualifiedIdentifier qualifiedIdentifier)
			return qualifiedIdentifier.UnqualifiedIdentifier;

		return name;
	}

	public Mapper CreateScope(Routine subroutine)
	{
		if (_moduleMapper != null)
			throw new InvalidOperationException("Cannot create a mapper scope off of a scope");

		return new Mapper(_module, this, subroutine);
	}

	public bool IsLinkedToCommonBlock(int variableIndex)
	{
		return (_variables[variableIndex].LinkedToCommonBlock != null);
	}

	public void LinkCommonVariable(int variableIndex, CommonBlock commonBlock, int commonBlockVariableIndex)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");
		if (_moduleMapper != null)
			throw new Exception("Can only link to a common variable from the Module Mapper");

		var variableInfo = _variables[variableIndex];

		variableInfo.LinkedToCommonBlock = commonBlock;
		variableInfo.LinkedToCommonBlockVariableIndex = commonBlockVariableIndex;
	}

	public void LinkModuleVariable(string name, DataType variableType)
		=> LinkModuleVariable(name, name, variableType);

	public void LinkModuleVariable(string localName, string moduleName, DataType variableType)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");
		if (_moduleMapper == null)
			throw new Exception("Cannot link to a module variable from the Module Mapper");

		int localIndex = ResolveVariable(localName);
		int moduleIndex = _moduleMapper.ResolveVariable(moduleName);

		var variableInfo = _variables[localIndex];

		variableInfo.LinkedToModuleVariableIndex = moduleIndex;
	}

	public void LinkModuleArray(string localName, string moduleName, DataType arrayType)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");
		if (_moduleMapper == null)
			throw new Exception("Cannot link to a module variable from the Module Mapper");

		int localIndex = ResolveArray(localName);
		int moduleIndex = _moduleMapper.ResolveArray(moduleName);

		var variableInfo = _variables[localIndex];

		variableInfo.LinkedToModuleVariableIndex = moduleIndex;
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
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		_disallowedSlugs.Add(slug);
	}

	public bool IsDisallowedSlug(string identifier)
	{
		return _disallowedSlugs.Contains(identifier);
	}

	public void ScanForDisallowedSlugs(IEnumerable<CodeModel.Statements.Statement> statements)
	{
		if (_moduleMapper != null)
		{
			foreach (var global in _moduleMapper.GlobalIdentifiers)
				AddDisallowedSlug(global);
		}

		foreach (var statement in statements)
		{
			switch (statement)
			{
				case CodeModel.Statements.DimStatement dimStatement:
					foreach (var declaration in dimStatement.Declarations)
					{
						if (declaration.UserType != null)
							AddDisallowedSlug(declaration.Name.Value);
					}

					break;
				case CodeModel.Statements.VariableScopeStatement scopeStatement:
					foreach (var declaration in scopeStatement.Declarations)
					{
						if (declaration.UserType != null)
							AddDisallowedSlug(declaration.Name.Value);
					}

					break;
			}
		}
	}

	public void DefineConstant(string name, LiteralValue literalValue)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		if ((GetSlug(name) is string slug)
		 && _disallowedSlugs.Contains(slug))
			throw CompilerException.IdentifierCannotIncludePeriod(default);

		name = UnqualifyIdentifier(name);

		if (_constantValueByName.TryGetValue(name, out _))
			throw CompilerException.DuplicateDefinition(default(Token));
		if (_variableIndexByName.TryGetValue(name, out var index))
			throw CompilerException.DuplicateDefinition(default(Token));

		_constantValueByName[name] = literalValue;
	}

	public void ResetConstants()
	{
		_constantValueByName.Clear();
	}

	public void HideConstants()
	{
		_hiddenConstants.UnionWith(_constantValueByName.Keys);
	}

	public void UnhideConstant(Identifier name)
		=> UnhideConstant(name.Value);

	public void UnhideConstant(string name)
	{
		_hiddenConstants.Remove(UnqualifyIdentifier(name));
	}

	public bool TryResolveConstant(Identifier name, [NotNullWhen(true)] out LiteralValue? literalValue)
	{
		if (_constantValueByName.TryGetValue(name, out literalValue)
		 && !_hiddenConstants.Contains(name.Value))
			return true;
		else if (_moduleMapper != null)
			return _moduleMapper.TryResolveConstant(name, out literalValue);
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
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		_semiscopeMode = SemiscopeMode.Setup;
		_semiscopeOverlay = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
	}

	public void EnterSemiscope()
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		_semiscopeMode = SemiscopeMode.Active;
	}

	public void ExitSemiscope()
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		_semiscopeMode = SemiscopeMode.Inactive;
		_semiscopeOverlay = null;
	}

	public int DeclareVariable(string name, DataType dataType, bool useTypeCharacter = true, bool hasExplicitTypeClause = false, Token? token = null)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		if ((GetSlug(name) is string slug)
		 && _disallowedSlugs.Contains(slug))
			throw CompilerException.IdentifierCannotIncludePeriod(token);

		var qualifiedName = QualifyIdentifier(name, dataType);
		var unqualifiedName = UnqualifyIdentifier(name);

		// You can't DIM a variable with the same name as a SUB or FUNCTION. But, where the error
		// manifests depends on where the code is and the type of collision.
		//
		// - If the collision is with a FUNCTION, it's always a "Duplicate definition" on the
		//   variable declaration.
		// - If the collision is with a DECLARE SUB/FUNCTION, it's always a "Duplicate definition"
		//   on the variable declaration.
		// - Main module: The DIM is permitted, and an error is generated on the SUB/FUNCTION
		//   opening line: "Sub and Function or Variable of the same name"
		// - Inside SUB or FUNCTION: "Duplicate definition on the variable declaration.

		bool isDuplicateDefinition = false;

		var unqualifiedIdentifier = Identifier.Standalone(unqualifiedName);

		if (_module.SubFacades.ContainsKey(unqualifiedIdentifier)
		 || _module.FunctionFacades.ContainsKey(unqualifiedIdentifier))
			isDuplicateDefinition = true;

		if (!isDuplicateDefinition
		 && _module.Routines.TryGetValue(unqualifiedIdentifier, out var routine)
		 && (routine != this.Routine)) // function return value variable
		{
			if ((routine.OpeningStatement is CodeModel.Statements.FunctionStatement)
			 || (_moduleMapper != null))
				isDuplicateDefinition = true;
		}

		if (!isDuplicateDefinition
		 && _constantValueByName.TryGetValue(unqualifiedName, out _)
		 && !_hiddenConstants.Contains(unqualifiedName))
			isDuplicateDefinition = true;

		if (!isDuplicateDefinition
		 && (_moduleMapper != null)
		 && _moduleMapper._constantValueByName.TryGetValue(unqualifiedName, out _))
			isDuplicateDefinition = true;

		if (!isDuplicateDefinition)
		{
			if (hasExplicitTypeClause)
			{
				if (_firstVariableByName.TryGetValue(unqualifiedName, out int firstDeclarationIndex))
				{
					var firstDeclaration = _variables[firstDeclarationIndex];

					if (!firstDeclaration.HasExplicitTypeClause
					 && firstDeclaration.Type.Equals(dataType))
						throw CompilerException.AsClauseRequiredOnFirstDeclaration(token);
				}

				// If any qualified version of this variable name has been used, then it is not
				// legal to declare an unqualified version as well.

				if (GetVariableNames().Select(name => name.UnqualifiedName).Contains(unqualifiedName))
					isDuplicateDefinition = true;
			}

			if (_variableIndexByName.ContainsKey(unqualifiedName))
				isDuplicateDefinition = true;
		}

		string registrationName = useTypeCharacter ? qualifiedName : unqualifiedName;

		// During semiscope setup, we allow new declarations to shadow
		// existing ones for DEF FN parameters.
		if (_semiscopeMode != SemiscopeMode.Setup)
		{
			if (_variableIndexByName.ContainsKey(registrationName)
			 || _variableIndexByName.ContainsKey(qualifiedName))
				isDuplicateDefinition = true;
		}

		if (isDuplicateDefinition)
			throw CompilerException.DuplicateDefinition(token);

		int index = _variables.Count;

		var info = new VariableInfo(registrationName, token, index);

		info.Type = dataType;
		info.HasExplicitTypeClause = hasExplicitTypeClause;

		_variables.Add(info);

		if (!_firstVariableByName.ContainsKey(unqualifiedName))
			_firstVariableByName[unqualifiedName] = index;

		if (_semiscopeMode != SemiscopeMode.Setup)
		{
			_variableIndexByName[registrationName] = index;
			if (dataType.IsPrimitiveType)
				_variableIndexByName[qualifiedName] = index;
		}
		else
		{
			_semiscopeOverlay![registrationName] = index;
			if (dataType.IsPrimitiveType)
				_semiscopeOverlay[qualifiedName] = index;
		}

		return index;
	}

	public int GetVariableByName(string name)
	{
		if (_variableIndexByName.TryGetValue(name, out var index))
			return index;

		return -1;
	}

	public int ResolveVariable(string name, DataType? dataType = null)
	{
		int index;

		// Try to resolve what we're given first; it won't be qualified
		// if its type is a UDT. Also, if it was declared using an explicit
		// AS clause, then the qualification can be omitted.
		if ((_semiscopeOverlay != null)
		 && _semiscopeOverlay.TryGetValue(name, out index)
		 && (_variables[index].Type.IsUserType || _variables[index].HasExplicitTypeClause))
			return index;
		if (_variableIndexByName.TryGetValue(name, out index)
		 && (_variables[index].Type.IsUserType || _variables[index].HasExplicitTypeClause))
			return index;

		// Next try qualifying it. If it's not UDT-typed, then the primary
		// declaration is the qualified one.
		var qualifiedName = (dataType == null)
			? QualifyIdentifier(name)
			: QualifyIdentifier(name, dataType);

		if ((_semiscopeOverlay != null)
			&& _semiscopeOverlay.TryGetValue(qualifiedName, out index))
			return index;
		if (_variableIndexByName.TryGetValue(qualifiedName, out index))
			return index;

		if (_isFrozen)
			return -1;

		return DeclareVariable(qualifiedName, dataType ?? DataType.ForPrimitiveDataType(GetTypeForIdentifier(name)));
	}

	public void AllowArrayRedeclaration(string name, DataType dataType)
	{
		var qualifiedName = QualifyIdentifier(name, dataType);

		if (!_arrayIndexByName.TryGetValue(name, out var nameIndex)
		 || !_arrayIndexByName.TryGetValue(qualifiedName, out var qualifiedNameIndex)
		 || (nameIndex != qualifiedNameIndex))
			throw new Exception("Internal error");

		_predeclaredArrayIndices.Add(nameIndex);
	}

	public int DeclareArray(string name, DataType dataType, int numberOfDimensions, bool useTypeCharacter = true, bool hasExplicitTypeClause = false, Token? token = null)
	{
		if (_isFrozen)
			throw new Exception("The Mapper is frozen");

		var qualifiedName = QualifyIdentifier(name, dataType);
		var unqualifiedName = UnqualifyIdentifier(name);

		bool isDuplicateDefinition = false;

		if (_module.IsRegistered(Identifier.Standalone(unqualifiedName)))
			isDuplicateDefinition = true;

		if (!isDuplicateDefinition)
		{
			if (_firstArrayByName.TryGetValue(unqualifiedName, out int firstDeclarationIndex))
			{
				var firstDeclaration = _variables[firstDeclarationIndex];

				if (hasExplicitTypeClause
				 && !firstDeclaration.HasExplicitTypeClause
				 && firstDeclaration.Type.Equals(dataType))
					throw CompilerException.AsClauseRequiredOnFirstDeclaration(token);
			}
		}

		int index = -1;

		if (!isDuplicateDefinition)
		{
			if (!_arrayIndexByName.TryGetValue(qualifiedName, out index))
				index = -1;

			if (index >= 0)
			{
				var existingVariable = _variables[index];

				if (hasExplicitTypeClause
				 && !existingVariable.HasExplicitTypeClause
				 && existingVariable.Type.Equals(dataType))
					throw CompilerException.AsClauseRequiredOnFirstDeclaration(token);

				if (!_predeclaredArrayIndices.Remove(index))
					isDuplicateDefinition = true;
			}
			else
				index = _variables.Count;
		}

		if (isDuplicateDefinition)
			throw CompilerException.DuplicateDefinition(token);

		string registrationName = useTypeCharacter ? qualifiedName : unqualifiedName;

		var info = new VariableInfo(registrationName, token, index);

		info.Type = dataType;
		info.HasExplicitTypeClause = hasExplicitTypeClause;
		info.NumberOfArrayDimensions = numberOfDimensions;

		_variables.Add(info);

		_arrayIndexByName[registrationName] = index;
		if (dataType.IsPrimitiveType)
			_arrayIndexByName[qualifiedName] = index;

		if (!_firstArrayByName.ContainsKey(unqualifiedName))
			_firstArrayByName[unqualifiedName] = index;

		return index;
	}

	public int ResolveArray(string name, DataType? arrayType = null, int numberOfDimensions = -1, bool useTypeCharacter = true, Token? nameToken = null)
		=> ResolveArray(name, arrayType, numberOfDimensions, createImplicitly: false, out _, useTypeCharacter, nameToken);

	public int ResolveArray(string name, DataType? arrayType, int numberOfDimensions, out bool implicitlyCreated, bool useTypeCharacter = true, Token? nameToken = null)
		=> ResolveArray(name, arrayType, numberOfDimensions, createImplicitly: true, out implicitlyCreated, useTypeCharacter, nameToken);

	int ResolveArray(string name, DataType? arrayType, int numberOfDimensions, bool createImplicitly, out bool implicitlyCreated, bool useTypeCharacter = true, Token? nameToken = null)
	{
		void MatchUpNumberOfDimensions(int index)
		{
			if (numberOfDimensions > 0)
			{
				var variable = _variables[index];

				if (variable.NumberOfArrayDimensions < 0)
					variable.NumberOfArrayDimensions = numberOfDimensions;
				else if (numberOfDimensions != variable.NumberOfArrayDimensions)
					throw CompilerException.WrongNumberOfDimensions(nameToken);
			}
		}

		implicitlyCreated = false;

		int index;

		// Try to resolve what we're given first; it won't be qualified
		// if its type is a UDT. Also, if it was declared using an explicit
		// AS clause, then the qualification can be omitted.
		if (_arrayIndexByName.TryGetValue(name, out index)
		 && (_variables[index].Type.IsUserType || _variables[index].HasExplicitTypeClause))
		{
			MatchUpNumberOfDimensions(index);

			_predeclaredArrayIndices.Remove(index);
			return index;
		}

		// Next try qualifying it. If it's not UDT-typed, then the primary
		// declaration is the qualified one.
		var qualifiedName = (arrayType == null)
			? QualifyIdentifier(name)
			: QualifyIdentifier(name, arrayType);

		if (_arrayIndexByName.TryGetValue(qualifiedName, out index))
		{
			MatchUpNumberOfDimensions(index);

			_predeclaredArrayIndices.Remove(index);
			return index;
		}

		if (_isFrozen)
			return -1;

		if (!createImplicitly)
			return -1;

		if (numberOfDimensions != 1)
			throw new Exception("Internal error: Implicit array creation with more than 1 dimension");

		implicitlyCreated = true;

		if (arrayType == null)
		{
			var elementType = DataType.ForPrimitiveDataType(GetTypeForIdentifier(name));

			arrayType = elementType.MakeArrayType();
		}

		return DeclareArray(qualifiedName, arrayType, numberOfDimensions, useTypeCharacter, token: nameToken);
	}

	public bool IsDeclaredVariableOrArray(Identifier identifier)
		=> IsDeclaredVariableOrArray(identifier.Value);

	public bool IsDeclaredVariableOrArray(string name)
	{
		var qualifiedName = QualifyIdentifier(name);

		// Variable

		if ((_semiscopeOverlay != null)
		 && _semiscopeOverlay.TryGetValue(name, out _))
			return true;
		if (_variableIndexByName.TryGetValue(name, out _))
			return true;

		if (qualifiedName != name)
		{
			if ((_semiscopeOverlay != null)
				&& _semiscopeOverlay.TryGetValue(qualifiedName, out _))
				return true;
			if (_variableIndexByName.TryGetValue(qualifiedName, out _))
				return true;
		}

		// Array

		if (_arrayIndexByName.TryGetValue(name, out _))
			return true;

		if (qualifiedName != name)
		{
			if (_arrayIndexByName.TryGetValue(qualifiedName, out _))
				return true;
		}

		return false;
	}

	public bool IsDeclaredArray(Identifier identifier, DataType dataType)
		=> IsDeclaredArray(identifier.Value, dataType);

	public bool IsDeclaredArray(string name, DataType dataType)
	{
		var qualifiedName = QualifyIdentifier(name, dataType);

		if (_arrayIndexByName.TryGetValue(name, out _))
			return true;

		if (qualifiedName != name)
		{
			if (_arrayIndexByName.TryGetValue(qualifiedName, out _))
				return true;
		}

		return false;
	}

	public IEnumerable<VariableName> GetVariableNames() =>
		_variables.Select(variable => new VariableName(variable.Name, variable.NameToken, variable.Index, variable.IsLinked));

	public List<DataType> GetVariableTypes() =>
		Enumerable.Range(0, _variables.Count)
		.Select(idx => _variables[idx].Type)
		.ToList();

	public IEnumerable<CommonVariableLinkGroup> GetCommonVariableLinkGroups() =>
		_variables
		.Where(info => info.LinkedToCommonBlock != null)
		.GroupBy(info => info.LinkedToCommonBlock!)
		.Select(grouping =>
			new CommonVariableLinkGroup(
				grouping.Key.Name ?? CommonBlock.DefaultBlockName,
				grouping
					.Select(info =>
						new VariableLink()
						{
							LocalIndex = info.Index,
							RemoteIndex = info.LinkedToCommonBlockVariableIndex,
						})
					.ToArray()));

	public List<VariableLink> GetLinkedVariables() =>
		_variables
		.Where(info => info.LinkedToModuleVariableIndex >= 0)
		.Select(info =>
			new VariableLink()
			{
				LocalIndex = info.Index,
				RemoteIndex = info.LinkedToModuleVariableIndex,
			})
		.ToList();

	public void RegisterTypeFacade(UserDataTypeFacade udtFacade)
	{
		if (_moduleMapper != null)
		{
			_moduleMapper.RegisterTypeFacade(udtFacade);
			return;
		}

		if (_typeFacadeByName.ContainsKey(udtFacade.Name))
			throw CompilerException.DuplicateDefinition(udtFacade.Statement?.FirstToken);

		_typeFacadeByName.Add(udtFacade.Name, new DataType(udtFacade));
	}

	public void HideTypeFacades()
	{
		_hiddenTypeFacades.UnionWith(_typeFacadeByName.Keys);
	}

	public void UnhideTypeFacade(Identifier name)
		=> UnhideTypeFacade(name.Value);

	public void UnhideTypeFacade(string name)
	{
		_hiddenTypeFacades.Remove(name);
	}

	public DataType ResolveType(string userType, Token? context = null)
		=> ResolveType(CodeModel.DataType.UserDataType, userType, fixedStringLength: 0, isArray: false, context);

	public DataType ResolveType(CodeModel.DataType primitiveType, string? userTypeName, int fixedStringLength, bool isArray, Token? context)
	{
		if (isArray)
		{
			var scalarType = ResolveType(primitiveType, userTypeName, fixedStringLength, isArray: false, context);

			return scalarType.MakeArrayType();
		}

		if (userTypeName == null)
			return DataType.FromCodeModelDataType(primitiveType, fixedStringLength);

		if (_moduleMapper != null)
			return _moduleMapper.ResolveType(primitiveType, userTypeName, fixedStringLength, isArray, context);
		else
		{
			if (_typeFacadeByName.TryGetValue(userTypeName, out var type)
			 && !_hiddenTypeFacades.Contains(userTypeName))
				return type;

			throw CompilerException.TypeNotDefined(context);
		}
	}

	public DataType ResolveType(CodeModel.VariableDeclarationBase declaration)
		=> ResolveType(declaration, out _);

	public DataType ResolveType(CodeModel.VariableDeclarationBase declaration, out bool useTypeCharacter)
	{
		DataType dataType;

		useTypeCharacter = false;

		if (declaration.Name is QualifiedIdentifier qualifiedName)
		{
			dataType = ResolveType(qualifiedName.TypeCharacter.Type, null, 0, declaration.HasSubscripts, declaration.NameToken);
			useTypeCharacter = true;
		}
		else if (declaration.UserType != null)
			dataType = ResolveType(declaration.UserType);
		else if (declaration.Type != CodeModel.DataType.Unspecified)
		{
			if ((declaration.Type == CodeModel.DataType.STRING)
			 && (declaration.FixedStringLength != null))
			{
				int fixedLength;

				if (!int.TryParse(declaration.FixedStringLength, out fixedLength))
				{
					if (!TryResolveConstant(declaration.FixedStringLength, out var constValue)
					 || !constValue.Type.IsInteger
					 || (constValue is not IntegerLiteralValue integerConstValue)
					 || (integerConstValue.Value < 1))
						throw new CompilerException(declaration.FixedStringLengthToken, "Invalid constant");

					fixedLength = integerConstValue.Value;
				}

				dataType = DataType.MakeFixedStringType(fixedLength);
			}
			else
				dataType = DataType.FromCodeModelDataType(declaration.Type);
		}
		else
		{
			dataType = DataType.ForPrimitiveDataType(GetTypeForIdentifier(declaration.Name));
			useTypeCharacter = true;
		}

		if (declaration.HasSubscripts)
			dataType = dataType.MakeArrayType();

		return dataType;
	}
}
