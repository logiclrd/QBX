using System;

using QBX.CodeModel;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public class FieldAccessExpression(Evaluable expression, int fieldIndex, DataType fieldType) : Evaluable
{
	public override void CollapseConstantSubexpressions()
	{
		expression.CollapseConstantSubexpressions();
	}

	public static Evaluable Construct(Evaluable? expression, Identifier fieldName, Token? fieldToken)
	{
		if (expression == null)
			throw new Exception("FieldAccessExpression.Construct requires expression");

		var dataType = expression.Type;

		if (!dataType.IsUserType)
			throw new CompilerException(expression.Source, "Left expression of a FieldAccessExpression must evaluate to a user-defined type");

		var userType = dataType.UserType;
		var userTypeFacade = dataType.UserTypeFacade;

		var unqualifiedFieldName = Mapper.UnqualifyIdentifier(fieldName);

		for (int fieldIndex = 0; fieldIndex < userType.Fields.Count; fieldIndex++)
		{
			var thisField = userType.Fields[fieldIndex];
			var thisFieldName = userTypeFacade.FieldNames[fieldIndex];

			if (thisFieldName == unqualifiedFieldName)
			{
				if (fieldName != unqualifiedFieldName)
				{
					if (fieldName is not QualifiedIdentifier qualifiedFieldName)
						throw new Exception("Internal error: name changed when unqualifying but field name is not a QualifiedIdentifier");

					var typeFromName = DataType.FromCodeModelDataType(qualifiedFieldName.TypeCharacter.Type);

					if (!typeFromName.Equals(thisField.Type))
						throw CompilerException.TypeMismatch(fieldToken);
				}

				return new FieldAccessExpression(expression, fieldIndex, thisField.Type);
			}
		}

		throw CompilerException.ElementNotDefined(expression.Source);
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

	public override LiteralValue EvaluateConstant() => throw CompilerException.ValueIsNotConstant(expression.Source);

	public override bool IsAssignable => expression.IsAssignable;
}
