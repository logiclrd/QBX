using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Marshalling;

public abstract class PrimitiveMarshaller : Marshaller
{
	public static PrimitiveMarshaller Construct(Type fromType, Type toType)
	{
		if ((fromType == typeof(IntegerVariable))
		 || (toType == typeof(IntegerVariable)))
			return new IntegerPrimitiveMarshaller();
		if ((fromType == typeof(LongVariable))
		 || (toType == typeof(LongVariable)))
			return new LongPrimitiveMarshaller();
		if ((fromType == typeof(SingleVariable))
		 || (toType == typeof(SingleVariable)))
			return new SinglePrimitiveMarshaller();
		if ((fromType == typeof(DoubleVariable))
		 || (toType == typeof(DoubleVariable)))
			return new DoublePrimitiveMarshaller();
		if ((fromType == typeof(CurrencyVariable))
		 || (toType == typeof(CurrencyVariable)))
			return new CurrencyPrimitiveMarshaller();
		if ((fromType == typeof(StringVariable))
		 || (toType == typeof(StringVariable)))
			return new StringPrimitiveMarshaller();

		throw new ArgumentException("Cannot construct a primitive mapper for the specified types");
	}
}

public class IntegerPrimitiveMarshaller : PrimitiveMarshaller
{
	public override void Map(object from, ref object? to)
	{
		short value;

		if (from is IntegerVariable sourceVariable)
			value = sourceVariable.Value;
		else
			value = Convert.ToInt16(from);

		if (to is Assignment assignment)
		{
			if (assignment is Assignment<short> directAssignment)
				directAssignment.Assign(value);
			else
				assignment.Assign(value);
		}
		else if (to is IntegerVariable targetVariable)
			targetVariable.Value = value;
		else
			to = value;
	}
}

public class LongPrimitiveMarshaller : PrimitiveMarshaller
{
	public override void Map(object from, ref object? to)
	{
		int value;

		if (from is LongVariable sourceVariable)
			value = sourceVariable.Value;
		else
			value = Convert.ToInt32(from);

		if (to is Assignment assignment)
		{
			if (assignment is Assignment<int> directAssignment)
				directAssignment.Assign(value);
			else
				assignment.Assign(value);
		}
		else if (to is LongVariable targetVariable)
			targetVariable.Value = value;
		else
			to = value;
	}
}

public class SinglePrimitiveMarshaller : PrimitiveMarshaller
{
	public override void Map(object from, ref object? to)
	{
		float value;

		if (from is SingleVariable sourceVariable)
			value = sourceVariable.Value;
		else
			value = Convert.ToSingle(from);

		if (to is Assignment assignment)
		{
			if (assignment is Assignment<float> directAssignment)
				directAssignment.Assign(value);
			else
				assignment.Assign(value);
		}
		else if (to is SingleVariable targetVariable)
			targetVariable.Value = value;
		else
			to = value;
	}
}

public class DoublePrimitiveMarshaller : PrimitiveMarshaller
{
	public override void Map(object from, ref object? to)
	{
		double value;

		if (from is DoubleVariable sourceVariable)
			value = sourceVariable.Value;
		else
			value = Convert.ToDouble(from);

		if (to is Assignment assignment)
		{
			if (assignment is Assignment<double> directAssignment)
				directAssignment.Assign(value);
			else
				assignment.Assign(value);
		}
		else if (to is DoubleVariable targetVariable)
			targetVariable.Value = value;
		else
			to = value;
	}
}

public class CurrencyPrimitiveMarshaller : PrimitiveMarshaller
{
	public override void Map(object from, ref object? to)
	{
		decimal value;

		if (from is CurrencyVariable sourceVariable)
			value = sourceVariable.Value;
		else
			value = Convert.ToDecimal(from);

		if (to is Assignment assignment)
		{
			if (assignment is Assignment<decimal> directAssignment)
				directAssignment.Assign(value);
			else
				assignment.Assign(value);
		}
		else if (to is CurrencyVariable targetVariable)
			targetVariable.Value = value;
		else
			to = value;
	}
}

public class StringPrimitiveMarshaller : PrimitiveMarshaller
{
	public override void Map(object from, ref object? to)
	{
		StringValue value;

		if (from is StringVariable sourceVariable)
			value = sourceVariable.Value;
		else if (from is StringValue stringValue)
			value = stringValue;
		else
			value = new StringValue(Convert.ToString(from) ?? "");

		if (to is Assignment assignment)
		{
			if (assignment is Assignment<string> directAssignment)
				directAssignment.Assign(value.ToString());
			else
				assignment.DynamicAssign(value.ToString());
		}
		else if (to is StringValue targetValue)
			targetValue.Set(value);
		else if (to is StringVariable targetVariable)
			targetVariable.SetValue(value);
		else
			to = value;
	}
}
