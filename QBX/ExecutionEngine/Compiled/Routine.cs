using QBX.CodeModel.Statements;
using System.Security.Cryptography;

namespace QBX.ExecutionEngine.Compiled;

public class Routine : ISequence
{
	public string Name;
	public SubroutineOpeningStatement? OpeningStatement;
	public List<DataType> ParameterTypes = new List<DataType>();
	public List<DataType> VariableTypes = new List<DataType>();
	public DataType? ReturnType;
	public List<IExecutable> Statements = new List<IExecutable>();

	void ISequence.Append(IExecutable statement) => Statements.Add(statement);
	int ISequence.Count => Statements.Count;
	IExecutable ISequence.this[int index] => Statements[index];

	public CodeModel.CompilationElement Source;

	public const string MainRoutineName = "@Main";

	public Routine(CodeModel.CompilationElement source, TypeRepository typeRepository)
	{
		Source = source;

		Name = GetName(source);

		foreach (var line in source.Lines)
		{
			if (line.Statements.FirstOrDefault() is SubroutineOpeningStatement subOrFunction)
			{
				OpeningStatement = subOrFunction;

				if (subOrFunction is FunctionStatement function)
				{
					char lastChar = Name.Last();

					if (CodeModel.TypeCharacter.TryParse(lastChar, out var typeCharacter))
					{
						// Can't have two functions whose names differ only by type character.
						// In other words, the type character isn't actually part of the
						// function's name.
						ReturnType = typeRepository.ResolveType(typeCharacter.Type, null, isArray: false, null);
						Name = Name.Remove(Name.Length - 1);
					}
				}

				break;
			}
		}
	}

	public static string GetName(CodeModel.CompilationElement source)
	{
		foreach (var line in source.Lines)
			if (line.Statements.FirstOrDefault() is SubroutineOpeningStatement subOrFunction)
				return subOrFunction.Name;

		return MainRoutineName;
	}

	public void Register(Mapper rootMapper)
	{
		if (OpeningStatement is SubStatement)
			rootMapper.RegisterSub(this);
		else if (OpeningStatement is FunctionStatement)
			rootMapper.RegisterFunction(this);
		else
			throw new Exception("Register called on a non-callable Routine");
	}

	public void TranslateParameters(Mapper mapper, TypeRepository typeRepository)
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
			foreach (var param in OpeningStatement.Parameters.Parameters)
			{
				ParameterTypes.Add(typeRepository.ResolveType(param));

				string name = param.Name;
				string nameWithoutTypeCharacter = name;

				bool typeCharacterPresent = CodeModel.TypeCharacter.TryParse(name.Last(), out var _);

				if (typeCharacterPresent)
					nameWithoutTypeCharacter = name.Remove(name.Length - 1);

				if (mapper.TryGetFunction(nameWithoutTypeCharacter, out var function))
				{
					if (typeCharacterPresent || param.IsArray)
						throw new RuntimeException(param.NameToken, "Duplicate definition");

					var paramType = typeRepository.ResolveType(param);
					var functionType = function.ReturnType;

					if (paramType.Equals(functionType))
						throw new RuntimeException(param.NameToken, "Duplicate definition");
				}
			}
		}
	}
}
