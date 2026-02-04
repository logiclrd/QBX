using QBX.CodeModel;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.DevelopmentEnvironment;

public class Watch(CompilationUnit unit, CompilationElement element, string expression)
{
	public CompilationUnit CompilationUnit = unit;
	public CompilationElement CompilationElement = element;
	public Routine? Routine;
	public string Expression = expression;
	public bool IsWatchPoint = false;
	public Variable? LastValue = null;
	public StringValue? LastValueFormatted = null;

	public override string ToString() => ToStringValue(out _).ToString();

	static readonly StringValue Empty = new StringValue();
	static readonly StringValue NotWatchable = new StringValue("<Not watchable>");
	static readonly StringValue TypeMismatch = new StringValue("Type mismatch");
	static readonly StringValue True = new StringValue("<TRUE>");
	static readonly StringValue False = new StringValue("<FALSE>");

	public StringValue ToStringValue(out bool highlight)
	{
		highlight = false;

		if (!IsWatchPoint)
			return LastValueFormatted ?? NotWatchable;
		else
		{
			try
			{
				if (LastValue == null)
					return Empty;

				if (!LastValue.DataType.IsNumeric)
					return TypeMismatch;

				bool @break = !LastValue.IsZero;

				highlight = @break;

				return @break ? True : False;
			}
			catch (RuntimeException error)
			{
				return new StringValue(error.Message);
			}
		}
	}
}
