using QBX.CodeModel.Statements;

namespace QBX.ExecutionEngine.Compiled;

public class Routine : ISequence
{
	public string Name;
	public List<DataType> ParameterTypes = new List<DataType>();
	public List<DataType> VariableTypes = new List<DataType>();
	public DataType? ReturnType;
	public List<IExecutable> Statements = new List<IExecutable>();

	void ISequence.Append(IExecutable statement) => Statements.Add(statement);
	int ISequence.Count => Statements.Count;
	IExecutable ISequence.this[int index] => Statements[index];

	public CodeModel.CompilationElement Source;

	public Routine(CodeModel.CompilationElement source, TypeRepository typeRepository)
	{
		Source = source;

		Name = "@Main";

		foreach (var line in source.Lines)
		{
			if (line.Statements.FirstOrDefault() is SubroutineOpeningStatement subOrFunction)
			{
				Name = subOrFunction.Name;

				if (subOrFunction.Parameters != null)
				{
					ParameterTypes.AddRange(subOrFunction.Parameters.Parameters.Select(
						param => typeRepository.ResolveType(param)));
				}

				// TODO: can't have a variable (incl. parameter) with the same name as a function

				break;
			}
		}
	}
}
