using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class CaseExpression
{
	public static CaseExpression Construct(Evaluable expression, Evaluable? rangeEndExpression, RelationalOperator relationToExpression)
	{
		if (rangeEndExpression is null)
		{
			if (relationToExpression == RelationalOperator.None)
				relationToExpression = RelationalOperator.Equals;

			switch (expression.Type.PrimitiveType)
			{
				case PrimitiveDataType.Integer: return IntegerRelativeCaseExpression.Construct(expression, relationToExpression);
				case PrimitiveDataType.Long: return LongCaseExpression.Construct(expression, relationToExpression);
				case PrimitiveDataType.Single: return SingleCaseExpression.Construct(expression, relationToExpression);
				case PrimitiveDataType.Double: return DoubleCaseExpression.Construct(expression, relationToExpression);
				case PrimitiveDataType.Currency: return CurrencyCaseExpression.Construct(expression, relationToExpression);
				case PrimitiveDataType.String: return StringCaseExpression.Construct(expression, relationToExpression);

				default: throw new Exception("Internal error");
			}
		}
		else
		{
			if (rangeEndExpression.Type != expression.Type)
				throw new Exception("CaseExpression.Construct called on expressions of differing type");

			if (relationToExpression != RelationalOperator.None)
				throw new Exception("CaseExpression.Construct called with a range and a relation");

			return RangeCaseExpression.Construct(
				rangeStartExpression: expression,
				rangeEndExpression);
		}
	}

	public abstract bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame);
}

public abstract class RelativeCaseExpression(Evaluable expression, RelationalOperator relationToExpression) : CaseExpression
{
	public Evaluable Expression => expression;
	public RelationalOperator RelationToExpression => relationToExpression;
}

public abstract class IntegerRelativeCaseExpression(Evaluable expression, RelationalOperator relationToExpression) : RelativeCaseExpression(expression, relationToExpression)
{
	public static IntegerRelativeCaseExpression Construct(Evaluable expression, RelationalOperator relationToExpression)
	{
		switch (relationToExpression)
		{
			case RelationalOperator.Equals: return new IntegerEqualsCaseExpression(expression);
			case RelationalOperator.NotEquals: return new IntegerNotEqualsCaseExpression(expression);
			case RelationalOperator.GreaterThan: return new IntegerGreaterThanCaseExpression(expression);
			case RelationalOperator.LessThan: return new IntegerLessThanCaseExpression(expression);
			case RelationalOperator.GreaterThanOrEquals: return new IntegerGreaterThanOrEqualsCaseExpression(expression);
			case RelationalOperator.LessThanOrEquals: return new IntegerLessThanOrEqualsCaseExpression(expression);

			default: throw new Exception("Internal error");
		}
	}

	class IntegerEqualsCaseExpression(Evaluable expression) : IntegerRelativeCaseExpression(expression, RelationalOperator.Equals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((IntegerVariable)testValue).Value == ((IntegerVariable)conditionValue).Value;
		}
	}

	class IntegerNotEqualsCaseExpression(Evaluable expression) : IntegerRelativeCaseExpression(expression, RelationalOperator.NotEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((IntegerVariable)testValue).Value != ((IntegerVariable)conditionValue).Value;
		}
	}

	class IntegerGreaterThanCaseExpression(Evaluable expression) : IntegerRelativeCaseExpression(expression, RelationalOperator.GreaterThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((IntegerVariable)testValue).Value > ((IntegerVariable)conditionValue).Value;
		}
	}

	class IntegerLessThanCaseExpression(Evaluable expression) : IntegerRelativeCaseExpression(expression, RelationalOperator.LessThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((IntegerVariable)testValue).Value < ((IntegerVariable)conditionValue).Value;
		}
	}

	class IntegerGreaterThanOrEqualsCaseExpression(Evaluable expression) : IntegerRelativeCaseExpression(expression, RelationalOperator.GreaterThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((IntegerVariable)testValue).Value >= ((IntegerVariable)conditionValue).Value;
		}
	}

	class IntegerLessThanOrEqualsCaseExpression(Evaluable expression) : IntegerRelativeCaseExpression(expression, RelationalOperator.LessThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((IntegerVariable)testValue).Value <= ((IntegerVariable)conditionValue).Value;
		}
	}
}

public abstract class LongCaseExpression(Evaluable expression, RelationalOperator relationToExpression) : RelativeCaseExpression(expression, relationToExpression)
{
	public static LongCaseExpression Construct(Evaluable expression, RelationalOperator relationToExpression)
	{
		switch (relationToExpression)
		{
			case RelationalOperator.Equals: return new LongEqualsCaseExpression(expression);
			case RelationalOperator.NotEquals: return new LongNotEqualsCaseExpression(expression);
			case RelationalOperator.GreaterThan: return new LongGreaterThanCaseExpression(expression);
			case RelationalOperator.LessThan: return new LongLessThanCaseExpression(expression);
			case RelationalOperator.GreaterThanOrEquals: return new LongGreaterThanOrEqualsCaseExpression(expression);
			case RelationalOperator.LessThanOrEquals: return new LongLessThanOrEqualsCaseExpression(expression);

			default: throw new Exception("Internal error");
		}
	}

	class LongEqualsCaseExpression(Evaluable expression) : LongCaseExpression(expression, RelationalOperator.Equals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((LongVariable)testValue).Value == ((LongVariable)conditionValue).Value;
		}
	}

	class LongNotEqualsCaseExpression(Evaluable expression) : LongCaseExpression(expression, RelationalOperator.NotEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((LongVariable)testValue).Value != ((LongVariable)conditionValue).Value;
		}
	}

	class LongGreaterThanCaseExpression(Evaluable expression) : LongCaseExpression(expression, RelationalOperator.GreaterThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((LongVariable)testValue).Value > ((LongVariable)conditionValue).Value;
		}
	}

	class LongLessThanCaseExpression(Evaluable expression) : LongCaseExpression(expression, RelationalOperator.LessThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((LongVariable)testValue).Value < ((LongVariable)conditionValue).Value;
		}
	}

	class LongGreaterThanOrEqualsCaseExpression(Evaluable expression) : LongCaseExpression(expression, RelationalOperator.GreaterThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((LongVariable)testValue).Value >= ((LongVariable)conditionValue).Value;
		}
	}

	class LongLessThanOrEqualsCaseExpression(Evaluable expression) : LongCaseExpression(expression, RelationalOperator.LessThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((LongVariable)testValue).Value <= ((LongVariable)conditionValue).Value;
		}
	}
}

public abstract class SingleCaseExpression(Evaluable expression, RelationalOperator relationToExpression) : RelativeCaseExpression(expression, relationToExpression)
{
	public static SingleCaseExpression Construct(Evaluable expression, RelationalOperator relationToExpression)
	{
		switch (relationToExpression)
		{
			case RelationalOperator.Equals: return new SingleEqualsCaseExpression(expression);
			case RelationalOperator.NotEquals: return new SingleNotEqualsCaseExpression(expression);
			case RelationalOperator.GreaterThan: return new SingleGreaterThanCaseExpression(expression);
			case RelationalOperator.LessThan: return new SingleLessThanCaseExpression(expression);
			case RelationalOperator.GreaterThanOrEquals: return new SingleGreaterThanOrEqualsCaseExpression(expression);
			case RelationalOperator.LessThanOrEquals: return new SingleLessThanOrEqualsCaseExpression(expression);

			default: throw new Exception("Internal error");
		}
	}

	class SingleEqualsCaseExpression(Evaluable expression) : SingleCaseExpression(expression, RelationalOperator.Equals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((SingleVariable)testValue).Value == ((SingleVariable)conditionValue).Value;
		}
	}

	class SingleNotEqualsCaseExpression(Evaluable expression) : SingleCaseExpression(expression, RelationalOperator.NotEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((SingleVariable)testValue).Value != ((SingleVariable)conditionValue).Value;
		}
	}

	class SingleGreaterThanCaseExpression(Evaluable expression) : SingleCaseExpression(expression, RelationalOperator.GreaterThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((SingleVariable)testValue).Value > ((SingleVariable)conditionValue).Value;
		}
	}

	class SingleLessThanCaseExpression(Evaluable expression) : SingleCaseExpression(expression, RelationalOperator.LessThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((SingleVariable)testValue).Value < ((SingleVariable)conditionValue).Value;
		}
	}

	class SingleGreaterThanOrEqualsCaseExpression(Evaluable expression) : SingleCaseExpression(expression, RelationalOperator.GreaterThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((SingleVariable)testValue).Value >= ((SingleVariable)conditionValue).Value;
		}
	}

	class SingleLessThanOrEqualsCaseExpression(Evaluable expression) : SingleCaseExpression(expression, RelationalOperator.LessThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((SingleVariable)testValue).Value <= ((SingleVariable)conditionValue).Value;
		}
	}
}

public abstract class DoubleCaseExpression(Evaluable expression, RelationalOperator relationToExpression) : RelativeCaseExpression(expression, relationToExpression)
{
	public static DoubleCaseExpression Construct(Evaluable expression, RelationalOperator relationToExpression)
	{
		switch (relationToExpression)
		{
			case RelationalOperator.Equals: return new DoubleEqualsCaseExpression(expression);
			case RelationalOperator.NotEquals: return new DoubleNotEqualsCaseExpression(expression);
			case RelationalOperator.GreaterThan: return new DoubleGreaterThanCaseExpression(expression);
			case RelationalOperator.LessThan: return new DoubleLessThanCaseExpression(expression);
			case RelationalOperator.GreaterThanOrEquals: return new DoubleGreaterThanOrEqualsCaseExpression(expression);
			case RelationalOperator.LessThanOrEquals: return new DoubleLessThanOrEqualsCaseExpression(expression);

			default: throw new Exception("Internal error");
		}
	}

	class DoubleEqualsCaseExpression(Evaluable expression) : DoubleCaseExpression(expression, RelationalOperator.Equals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((DoubleVariable)testValue).Value == ((DoubleVariable)conditionValue).Value;
		}
	}

	class DoubleNotEqualsCaseExpression(Evaluable expression) : DoubleCaseExpression(expression, RelationalOperator.NotEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((DoubleVariable)testValue).Value != ((DoubleVariable)conditionValue).Value;
		}
	}

	class DoubleGreaterThanCaseExpression(Evaluable expression) : DoubleCaseExpression(expression, RelationalOperator.GreaterThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((DoubleVariable)testValue).Value > ((DoubleVariable)conditionValue).Value;
		}
	}

	class DoubleLessThanCaseExpression(Evaluable expression) : DoubleCaseExpression(expression, RelationalOperator.LessThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((DoubleVariable)testValue).Value < ((DoubleVariable)conditionValue).Value;
		}
	}

	class DoubleGreaterThanOrEqualsCaseExpression(Evaluable expression) : DoubleCaseExpression(expression, RelationalOperator.GreaterThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((DoubleVariable)testValue).Value >= ((DoubleVariable)conditionValue).Value;
		}
	}

	class DoubleLessThanOrEqualsCaseExpression(Evaluable expression) : DoubleCaseExpression(expression, RelationalOperator.LessThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((DoubleVariable)testValue).Value <= ((DoubleVariable)conditionValue).Value;
		}
	}
}

public abstract class CurrencyCaseExpression(Evaluable expression, RelationalOperator relationToExpression) : RelativeCaseExpression(expression, relationToExpression)
{
	public static CurrencyCaseExpression Construct(Evaluable expression, RelationalOperator relationToExpression)
	{
		switch (relationToExpression)
		{
			case RelationalOperator.Equals: return new CurrencyEqualsCaseExpression(expression);
			case RelationalOperator.NotEquals: return new CurrencyNotEqualsCaseExpression(expression);
			case RelationalOperator.GreaterThan: return new CurrencyGreaterThanCaseExpression(expression);
			case RelationalOperator.LessThan: return new CurrencyLessThanCaseExpression(expression);
			case RelationalOperator.GreaterThanOrEquals: return new CurrencyGreaterThanOrEqualsCaseExpression(expression);
			case RelationalOperator.LessThanOrEquals: return new CurrencyLessThanOrEqualsCaseExpression(expression);

			default: throw new Exception("Internal error");
		}
	}

	class CurrencyEqualsCaseExpression(Evaluable expression) : CurrencyCaseExpression(expression, RelationalOperator.Equals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((CurrencyVariable)testValue).Value == ((CurrencyVariable)conditionValue).Value;
		}
	}

	class CurrencyNotEqualsCaseExpression(Evaluable expression) : CurrencyCaseExpression(expression, RelationalOperator.NotEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((CurrencyVariable)testValue).Value != ((CurrencyVariable)conditionValue).Value;
		}
	}

	class CurrencyGreaterThanCaseExpression(Evaluable expression) : CurrencyCaseExpression(expression, RelationalOperator.GreaterThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((CurrencyVariable)testValue).Value > ((CurrencyVariable)conditionValue).Value;
		}
	}

	class CurrencyLessThanCaseExpression(Evaluable expression) : CurrencyCaseExpression(expression, RelationalOperator.LessThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((CurrencyVariable)testValue).Value < ((CurrencyVariable)conditionValue).Value;
		}
	}

	class CurrencyGreaterThanOrEqualsCaseExpression(Evaluable expression) : CurrencyCaseExpression(expression, RelationalOperator.GreaterThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((CurrencyVariable)testValue).Value >= ((CurrencyVariable)conditionValue).Value;
		}
	}

	class CurrencyLessThanOrEqualsCaseExpression(Evaluable expression) : CurrencyCaseExpression(expression, RelationalOperator.LessThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			return ((CurrencyVariable)testValue).Value <= ((CurrencyVariable)conditionValue).Value;
		}
	}
}

public abstract class StringCaseExpression(Evaluable expression, RelationalOperator relationToExpression) : RelativeCaseExpression(expression, relationToExpression)
{
	public static StringCaseExpression Construct(Evaluable expression, RelationalOperator relationToExpression)
	{
		switch (relationToExpression)
		{
			case RelationalOperator.Equals: return new StringEqualsCaseExpression(expression);
			case RelationalOperator.NotEquals: return new StringNotEqualsCaseExpression(expression);
			case RelationalOperator.GreaterThan: return new StringGreaterThanCaseExpression(expression);
			case RelationalOperator.LessThan: return new StringLessThanCaseExpression(expression);
			case RelationalOperator.GreaterThanOrEquals: return new StringGreaterThanOrEqualsCaseExpression(expression);
			case RelationalOperator.LessThanOrEquals: return new StringLessThanOrEqualsCaseExpression(expression);

			default: throw new Exception("Internal error");
		}
	}

	class StringEqualsCaseExpression(Evaluable expression) : StringCaseExpression(expression, RelationalOperator.Equals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			StringValue leftValue = ((StringVariable)testValue).Value;
			StringValue rightValue = ((StringVariable)conditionValue).Value;

			return leftValue.CompareTo(rightValue) == 0;
		}
	}

	class StringNotEqualsCaseExpression(Evaluable expression) : StringCaseExpression(expression, RelationalOperator.NotEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			StringValue leftValue = ((StringVariable)testValue).Value;
			StringValue rightValue = ((StringVariable)conditionValue).Value;

			return leftValue.CompareTo(rightValue) != 0;
		}
	}

	class StringGreaterThanCaseExpression(Evaluable expression) : StringCaseExpression(expression, RelationalOperator.GreaterThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			StringValue leftValue = ((StringVariable)testValue).Value;
			StringValue rightValue = ((StringVariable)conditionValue).Value;

			return leftValue.CompareTo(rightValue) > 0;
		}
	}

	class StringLessThanCaseExpression(Evaluable expression) : StringCaseExpression(expression, RelationalOperator.LessThan)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			StringValue leftValue = ((StringVariable)testValue).Value;
			StringValue rightValue = ((StringVariable)conditionValue).Value;

			return leftValue.CompareTo(rightValue) < 0;
		}
	}

	class StringGreaterThanOrEqualsCaseExpression(Evaluable expression) : StringCaseExpression(expression, RelationalOperator.GreaterThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			StringValue leftValue = ((StringVariable)testValue).Value;
			StringValue rightValue = ((StringVariable)conditionValue).Value;

			return leftValue.CompareTo(rightValue) >= 0;
		}
	}

	class StringLessThanOrEqualsCaseExpression(Evaluable expression) : StringCaseExpression(expression, RelationalOperator.LessThanOrEquals)
	{
		public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
		{
			var conditionValue = expression.Evaluate(context, stackFrame);

			StringValue leftValue = ((StringVariable)testValue).Value;
			StringValue rightValue = ((StringVariable)conditionValue).Value;

			return leftValue.CompareTo(rightValue) <= 0;
		}
	}
}

public abstract class RangeCaseExpression(Evaluable rangeStartExpression, Evaluable rangeEndExpression) : CaseExpression
{
	public static RangeCaseExpression Construct(Evaluable rangeStartExpression, Evaluable rangeEndExpression)
	{
		if (!rangeStartExpression.Type.IsPrimitiveType || !rangeEndExpression.Type.IsPrimitiveType)
			throw new Exception("CaseRangeExpression.Construct called on an expression of non-primitive type");
		if (rangeStartExpression.Type.PrimitiveType != rangeEndExpression.Type.PrimitiveType)
			throw new Exception("CaseRangeExpression.Construct called on an expressions whose types do not match");

		switch (rangeStartExpression.Type.PrimitiveType)
		{
			case PrimitiveDataType.Integer: return new IntegerRangeCaseExpression(rangeStartExpression, rangeEndExpression);
			case PrimitiveDataType.Long: return new LongRangeCaseExpression(rangeStartExpression, rangeEndExpression);
			case PrimitiveDataType.Single: return new SingleRangeCaseExpression(rangeStartExpression, rangeEndExpression);
			case PrimitiveDataType.Double: return new DoubleRangeCaseExpression(rangeStartExpression, rangeEndExpression);
			case PrimitiveDataType.Currency: return new CurrencyRangeCaseExpression(rangeStartExpression, rangeEndExpression);
			case PrimitiveDataType.String: return new StringRangeCaseExpression(rangeStartExpression, rangeEndExpression);

			default: throw new Exception("Internal error");
		}
	}

	public Evaluable RangeStartExpression => rangeStartExpression;
	public Evaluable RangeEndExpression => rangeEndExpression;
}

public class IntegerRangeCaseExpression(Evaluable rangeStartExpression, Evaluable rangeEndExpression) : RangeCaseExpression(rangeStartExpression, rangeEndExpression)
{
	public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
	{
		var rangeStart = (IntegerVariable)rangeStartExpression.Evaluate(context, stackFrame);
		var rangeEnd = (IntegerVariable)rangeEndExpression.Evaluate(context, stackFrame);

		var test = (IntegerVariable)testValue;

		return
			(rangeStart.Value <= test.Value) &&
			(test.Value <= rangeEnd.Value);
	}
}

public class LongRangeCaseExpression(Evaluable rangeStartExpression, Evaluable rangeEndExpression) : RangeCaseExpression(rangeStartExpression, rangeEndExpression)
{
	public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
	{
		var rangeStart = (LongVariable)rangeStartExpression.Evaluate(context, stackFrame);
		var rangeEnd = (LongVariable)rangeEndExpression.Evaluate(context, stackFrame);

		var test = (LongVariable)testValue;

		return
			(rangeStart.Value <= test.Value) &&
			(test.Value <= rangeEnd.Value);
	}
}

public class SingleRangeCaseExpression(Evaluable rangeStartExpression, Evaluable rangeEndExpression) : RangeCaseExpression(rangeStartExpression, rangeEndExpression)
{
	public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
	{
		var rangeStart = (SingleVariable)rangeStartExpression.Evaluate(context, stackFrame);
		var rangeEnd = (SingleVariable)rangeEndExpression.Evaluate(context, stackFrame);

		var test = (SingleVariable)testValue;

		return
			(rangeStart.Value <= test.Value) &&
			(test.Value <= rangeEnd.Value);
	}
}

public class DoubleRangeCaseExpression(Evaluable rangeStartExpression, Evaluable rangeEndExpression) : RangeCaseExpression(rangeStartExpression, rangeEndExpression)
{
	public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
	{
		var rangeStart = (DoubleVariable)rangeStartExpression.Evaluate(context, stackFrame);
		var rangeEnd = (DoubleVariable)rangeEndExpression.Evaluate(context, stackFrame);

		var test = (DoubleVariable)testValue;

		return
			(rangeStart.Value <= test.Value) &&
			(test.Value <= rangeEnd.Value);
	}
}

public class CurrencyRangeCaseExpression(Evaluable rangeStartExpression, Evaluable rangeEndExpression) : RangeCaseExpression(rangeStartExpression, rangeEndExpression)
{
	public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
	{
		var rangeStart = (CurrencyVariable)rangeStartExpression.Evaluate(context, stackFrame);
		var rangeEnd = (CurrencyVariable)rangeEndExpression.Evaluate(context, stackFrame);

		var test = (CurrencyVariable)testValue;

		return
			(rangeStart.Value <= test.Value) &&
			(test.Value <= rangeEnd.Value);
	}
}

public class StringRangeCaseExpression(Evaluable rangeStartExpression, Evaluable rangeEndExpression) : RangeCaseExpression(rangeStartExpression, rangeEndExpression)
{
	public override bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
	{
		var rangeStart = (StringVariable)rangeStartExpression.Evaluate(context, stackFrame);
		var rangeEnd = (StringVariable)rangeEndExpression.Evaluate(context, stackFrame);

		var test = (StringVariable)testValue;

		return
			(rangeStart.Value.CompareTo(test.Value) <= 0) &&
			(test.Value.CompareTo(rangeEnd.Value) <= 0);
	}
}
