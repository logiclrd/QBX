using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Subtraction
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		// Remaining possibilities: String, Currency, Double, Single, Long, Integer

		if (left.Type.IsString || right.Type.IsString)
		{
			var blame = left.Type.IsString
				? right.SourceExpression?.Token
				: left.SourceExpression?.Token;

			throw new RuntimeException(blame, "Type mismatch");
		}

		// Remaining possibilities: Currency, Double, Single, Long, Integer

		if (left.Type.IsDouble || right.Type.IsDouble)
		{
			if (!left.Type.IsDouble)
				left = new ConvertToDouble(left);
			if (!right.Type.IsDouble)
				right = new ConvertToDouble(right);

			return new DoubleSubtraction(left, right);
		}

		// Remaining possibilities: Currency, Single, Long, Integer

		if ((left.Type.IsCurrency && right.Type.IsSingle)
		 || (right.Type.IsCurrency && left.Type.IsSingle))
		{
			left = new ConvertToDouble(left);
			right = new ConvertToDouble(right);

			return new DoubleSubtraction(left, right);
		}

		if (left.Type.IsCurrency || right.Type.IsCurrency)
		{
			if (!left.Type.IsCurrency)
				left = new ConvertToCurrency(left);
			if (!right.Type.IsCurrency)
				right = new ConvertToCurrency(right);

			return new CurrencySubtraction(left, right);
		}

		// Remaining possibilities: Single, Long, Integer

		if (left.Type.IsSingle || right.Type.IsSingle)
		{
			if (!left.Type.IsSingle)
				left = new ConvertToSingle(left);
			if (!right.Type.IsSingle)
				right = new ConvertToSingle(right);

			return new SingleSubtraction(left, right);
		}

		// Remaining possibilities: Long, Integer

		if (left.Type.IsLong || right.Type.IsLong)
		{
			if (!left.Type.IsLong)
				left = new ConvertToLong(left);
			if (!right.Type.IsLong)
				right = new ConvertToLong(right);

			return new LongSubtraction(left, right);
		}

		// Remaining possibilities: Integer

		if (!left.Type.IsInteger || !right.Type.IsInteger)
			throw new Exception("Internal error: Expression should be of Integer type");

		return new IntegerSubtraction(left, right);
	}
}

public class IntegerSubtraction(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate()
	{
		var leftValue = (IntegerVariable)left.Evaluate();
		var rightValue = (IntegerVariable)right.Evaluate();

		int difference = leftValue.Value - rightValue.Value;

		if ((difference < short.MinValue) || (difference > short.MaxValue))
			throw new RuntimeException(SourceExpression?.Token ?? SourceStatement?.FirstToken, "Overflow");

		return new IntegerVariable(unchecked((short)difference));
	}
}

public class LongSubtraction(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate()
	{
		var leftValue = (LongVariable)left.Evaluate();
		var rightValue = (LongVariable)right.Evaluate();

		try
		{
			return new LongVariable(leftValue.Value - rightValue.Value);
		}
		catch (OverflowException)
		{
			throw new RuntimeException(SourceExpression?.Token ?? SourceStatement?.FirstToken, "Overflow");
		}
	}
}

public class SingleSubtraction(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate()
	{
		var leftValue = (SingleVariable)left.Evaluate();
		var rightValue = (SingleVariable)right.Evaluate();

		try
		{
			return new SingleVariable(leftValue.Value - rightValue.Value);
		}
		catch (OverflowException)
		{
			throw new RuntimeException(SourceExpression?.Token ?? SourceStatement?.FirstToken, "Overflow");
		}
	}
}

public class DoubleSubtraction(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate()
	{
		var leftValue = (DoubleVariable)left.Evaluate();
		var rightValue = (DoubleVariable)right.Evaluate();

		try
		{
			return new DoubleVariable(leftValue.Value - rightValue.Value);
		}
		catch (OverflowException)
		{
			throw new RuntimeException(SourceExpression?.Token ?? SourceStatement?.FirstToken, "Overflow");
		}
	}
}

public class CurrencySubtraction(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate()
	{
		var leftValue = (CurrencyVariable)left.Evaluate();
		var rightValue = (CurrencyVariable)right.Evaluate();

		try
		{
			return new CurrencyVariable(leftValue.Value - rightValue.Value);
		}
		catch (OverflowException)
		{
			throw new RuntimeException(SourceExpression?.Token ?? SourceStatement?.FirstToken, "Overflow");
		}
	}
}
