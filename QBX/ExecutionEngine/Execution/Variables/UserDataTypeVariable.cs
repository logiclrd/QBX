using System;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public class UserDataTypeVariable : Variable
{
	public Variable[] Fields;

	public UserDataTypeVariable(UserDataType dataType)
		: base(new DataType(dataType))
	{
		Fields = new Variable[dataType.Fields.Count];

		for (int i = 0; i < dataType.Fields.Count; i++)
			Fields[i] = Variable.Construct(dataType.Fields[i].Type);
	}

	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;

	public override int CoerceToInt() => throw new Exception("Cannot coerce user data type value to integer");

	public override object GetData() => this;
	public override void SetData(object value) => throw new NotSupportedException();
}
