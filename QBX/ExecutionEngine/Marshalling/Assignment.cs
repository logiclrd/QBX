using System;

using QBX.Numbers;

namespace QBX.ExecutionEngine.Marshalling;

public abstract class Assignment
{
	public abstract Type Type { get; }

	public abstract void DynamicAssign(object value);

	public void Assign<TValue>(TValue value)
		where TValue : struct
	{
		if (Type.IsEnum)
			DynamicAssign(Enum.ToObject(Type, value));
		else
			DynamicAssign(UncheckedNumberConverter.ToType(value, Type));
	}
}

public class Assignment<T>(Action<T> assign) : Assignment
{
	public override Type Type => typeof(T);

	public readonly Action<T> Assign = assign;

	public override void DynamicAssign(object value)
	{
		Assign((T)value);
	}
}
