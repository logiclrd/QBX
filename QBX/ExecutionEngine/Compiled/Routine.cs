using QBX.CodeModel.Statements;

namespace QBX.ExecutionEngine.Compiled;

public class Routine
{
	public string Name;
	public List<DataType> ParameterTypes = new List<DataType>();
	public List<DataType> VariableTypes = new List<DataType>();
	public DataType? ReturnType;
	public List<IExecutable> Statements = new List<IExecutable>();

	public CodeModel.CompilationElement Source;

	public Routine(CodeModel.CompilationElement source)
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
						param => DataType.FromParameterDefinition(param)));
				}

				// TODO: can't have a variable (incl. parameter) with the same name as a function

				break;
			}
		}
	}

	public Execution.Variable Execute(Execution.ExecutionContext context, Execution.Variable[] arguments)
	{
		context.PushFrame(ReturnType, arguments, VariableTypes);

		try
		{
			foreach (var statement in Statements)
			{
				// TODO:
				// if (statement is ExitRoutineStatement)
				//	break;

				statement.Execute(context);
			}

			return context.Variables[0];
		}
		finally
		{
			context.PopFrame();
		}
	}
}
