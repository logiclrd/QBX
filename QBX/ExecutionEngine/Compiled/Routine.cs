using System;
using System.Collections.Generic;
using System.Linq;

using QBX.CodeModel.Statements;
using QBX.ExecutionEngine.Compiled.Statements;

namespace QBX.ExecutionEngine.Compiled;

public class Routine : Sequence
{
	public Module Module;

	public string Name;
	public SubroutineOpeningStatement? OpeningStatement;
	public List<DataType> ParameterTypes = new List<DataType>();
	public List<DataType> VariableTypes = new List<DataType>();
	public List<VariableLink> LinkedVariables = new List<VariableLink>();
	public DataType? ReturnType;
	public int[] ParameterVariableIndices;
	public int ReturnValueVariableIndex = -1;

	public CodeModel.CompilationElement Source;

	// Labels:
	//   The execution model uses the native execution stack to track its own
	//   ongoing stack. There are additional stack frames involved every time a
	//   statement has a body, such as IF, FOR, DO, SELECT CASE, etc. This presents
	//   a unique challenge for GOTO, GOSUB and the "Set Next Statement" debugger
	//   action: switching to another statement involves unwinding and rewinding
	//   the stack. This can, of course, only be done within the same logical
	//   StackFrame shared by all the real stack frames handling the execution of
	//   nested Sequences.
	//
	//   In order to support this, there is an "exception" called GoTo which takes
	//   a StatementPath. This path tells ExecutionContext at each Sequence which
	//   index it should jump to, and at each Executable which subsequence, if any,
	//   it should follow.
	//
	//   Labels/line numbers may be as yet undefined when a GOTO or GOSUB statement
	//   references them, so we might not immediately be able to resolve the target
	//   of such a statement at the time it is translated. So we defer the
	//   resolution. In addition, if changes to the code are made while it is
	//   executing, then the path to a statement following a label, or indeed the
	//   statement itself, may change. To simplify this, an Executable called a
	//   LabelStatement is a no-op that can be located even if it moves, and
	//   the ResolveJumpStatements function recomputes all the mappings following
	//   any change to the code.

	public const string MainRoutineName = "@Main";

	public bool UseRootFrame = false;

	// SUB or FUNCTION
	public Routine(Module module, CodeModel.CompilationElement source)
	{
		Module = module;

		Source = source;

		Name = GetName(source);

		ParameterVariableIndices = Array.Empty<int>();

		foreach (var line in source.Lines)
		{
			if (line.Statements.FirstOrDefault() is ProperSubroutineOpeningStatement subOrFunction)
			{
				OpeningStatement = subOrFunction;

				if (OpeningStatement.Parameters != null)
					ParameterVariableIndices = new int[OpeningStatement.Parameters.Parameters.Count];

				break;
			}
		}

		module.Routines.Add(Name, this);
	}

	public void SetReturnType(Mapper mapper, TypeRepository typeRepository)
	{
		char lastChar = Name.Last();

		if (CodeModel.TypeCharacter.TryParse(lastChar, out var typeCharacter))
		{
			// Can't have two functions whose names differ only by type character.
			// In other words, the type character isn't actually part of the
			// function's name.
			ReturnType = typeRepository.ResolveType(typeCharacter.Type, null, fixedStringLength: 0, isArray: false, null);
			Name = Name.Remove(Name.Length - 1);
		}
		else
		{
			// Ensure that any DEFtype statements preceding the opening statement are in effect.
			foreach (var line in Source.Lines)
			{
				if (line.Statements.FirstOrDefault() is ProperSubroutineOpeningStatement)
					break;

				foreach (var defTypeStatement in line.Statements.OfType<DefTypeStatement>())
					mapper.ApplyDefTypeStatement(defTypeStatement);
			}

			ReturnType = DataType.ForPrimitiveDataType(mapper.GetTypeForIdentifier(Name));
		}
	}

	// DEF FN
	public Routine(Module module, CodeModel.CompilationElement source, DefFnStatement openingStatement, TypeRepository typeRepository)
	{
		Module = module;

		Source = source;

		Name = openingStatement.Name;

		OpeningStatement = openingStatement;

		if (openingStatement.Parameters != null)
			ParameterVariableIndices = new int[openingStatement.Parameters.Parameters.Count];
		else
			ParameterVariableIndices = Array.Empty<int>();

		char lastChar = Name.Last();

		if (CodeModel.TypeCharacter.TryParse(lastChar, out var typeCharacter))
		{
			// Can't have two functions whose names differ only by type character.
			// In other words, the type character isn't actually part of the
			// function's name.
			ReturnType = typeRepository.ResolveType(typeCharacter.Type, null, fixedStringLength: 0, isArray: false, null);
			Name = Name.Remove(Name.Length - 1);
		}

		module.Routines.Add(Name, this);
	}

	public static string GetName(CodeModel.CompilationElement source)
	{
		foreach (var line in source.Lines)
			if (line.Statements.FirstOrDefault() is ProperSubroutineOpeningStatement subOrFunction)
				return subOrFunction.Name;

		return MainRoutineName;
	}

	public void Register(Compilation compilation)
	{
		if (OpeningStatement is SubStatement)
			compilation.RegisterSub(this);
		else if ((OpeningStatement is FunctionStatement) || (OpeningStatement is DefFnStatement))
			compilation.RegisterFunction(this);
		else
			throw new Exception("Register called on a non-callable Routine");
	}

	public void TranslateParameters(Mapper mapper, Compilation compilation)
	{
		if (OpeningStatement == null)
			throw new Exception("TranslateParameters called on a Routine with no OpeningStatement");

		// Can't have a parameter with a type character that has the same name as a function.
		// Can't have a parameter without a type character that has the same name as
		// a function of the same type.
		// Can't have an array parameter, with or without type character, that has the same
		// name as a function.
		//
		// CAN have a parameter named "b" to a function named "b%". Weird.

		if (OpeningStatement.Parameters != null)
		{
			var parameterDefinitions = OpeningStatement.Parameters.Parameters;

			for (int i=0; i < parameterDefinitions.Count; i++)
			{
				var param = parameterDefinitions[i];

				var paramType = compilation.TypeRepository.ResolveType(param, mapper);

				ParameterTypes.Add(paramType);

				string name = param.Name;
				string nameWithoutTypeCharacter = name;

				bool typeCharacterPresent = CodeModel.TypeCharacter.TryParse(name.Last(), out var _);

				if (typeCharacterPresent)
					nameWithoutTypeCharacter = name.Remove(name.Length - 1);

				if (compilation.TryGetFunction(nameWithoutTypeCharacter, out var function))
				{
					if (typeCharacterPresent || param.IsArray)
						throw CompilerException.DuplicateDefinition(param.NameToken);

					var functionType = function.ReturnType;

					if (paramType.Equals(functionType))
						throw CompilerException.DuplicateDefinition(param.NameToken);
				}

				ParameterVariableIndices[i] = mapper.DeclareVariable(name, paramType);
			}
		}
	}

	public void ResolveJumpStatements()
	{
		var labels = new Dictionary<string, StatementPath>();

		foreach (var label in AllStatements.OfType<LabelStatement>())
		{
			if (labels.ContainsKey(label.LabelName))
				throw CompilerException.DuplicateLabel(label.Source);

			labels[label.LabelName] = label.GetPathToStatement();
		}

		foreach (var jump in AllStatements.OfType<JumpStatement>())
		{
			if (!labels.TryGetValue(jump.TargetLabelName, out var targetPath))
				throw CompilerException.LabelNotDefined(jump.Source?.FirstToken);

			jump.TargetPath = targetPath;
		}
	}
}
