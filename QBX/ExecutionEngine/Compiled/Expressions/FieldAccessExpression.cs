using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public class FieldAccessExpression(Evaluable expression, int fieldIndex, DataType fieldType) : Evaluable
{
	public override void CollapseConstantSubexpressions()
	{
		expression.CollapseConstantSubexpressions();
	}

	public static Evaluable Construct(Evaluable? expression, string fieldName)
	{
		if (expression == null)
			throw new Exception("FieldAccessExpression.Construct requires expression");

		var dataType = expression.Type;

		if (!dataType.IsUserType)
			throw new CompilerException(expression.SourceExpression?.Token, "Left expression of a FieldAccessExpression must evaluate to a user-defined type");

		var userType = dataType.UserType;

		for (int fieldIndex = 0; fieldIndex < userType.Fields.Count; fieldIndex++)
		{
			var field = userType.Fields[fieldIndex];

			if (field.Name.Equals(fieldName, StringComparison.Ordinal))
				return new FieldAccessExpression(expression, fieldIndex, field.Type);
		}

		throw CompilerException.ElementNotDefined(expression.SourceExpression?.Token);
	}

	public override DataType Type => fieldType;

	public Evaluable? Expression => expression;
	public int FieldIndex => fieldIndex;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (expression == null)
			throw new Exception("FieldAccessExpression has no expression");

		var structure = (UserDataTypeVariable)expression.Evaluate(context, stackFrame);

		return structure.Fields[FieldIndex];
	}

	public override LiteralValue EvaluateConstant() => throw CompilerException.ValueIsNotConstant(expression?.SourceExpression?.Token);
}
