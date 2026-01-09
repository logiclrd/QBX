using System;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public class DummyVariable : Variable
{
	public DummyVariable()
		: base(DataType.Integer)
	{
	}

	static Exception CreateError() => new Exception("Internal error: Attempt to use a DummyVariable as a regular variable");

	public override object GetData() => throw CreateError();
	public override void SetData(object value) => throw CreateError();

	public override int CoerceToInt() => throw CreateError();
	public override string ToString() => throw CreateError();

	public override bool IsZero => throw CreateError();
	public override bool IsPositive => throw CreateError();
	public override bool IsNegative => throw CreateError();
}
