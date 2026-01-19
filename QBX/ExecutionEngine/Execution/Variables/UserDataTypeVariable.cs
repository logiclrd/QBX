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

	public override int CoerceToInt(Evaluable? context) => throw RuntimeException.TypeMismatch(context?.Source);

	public override object GetData() => this;
	public override void SetData(object value) => throw new NotSupportedException();

	public override void Serialize(Span<byte> buffer)
	{
		foreach (var field in Fields)
		{
			field.Serialize(buffer);

			if (buffer.Length <= field.DataType.ByteSize)
				break;

			buffer = buffer.Slice(field.DataType.ByteSize);
		}
	}

	public override void Deserialize(ReadOnlySpan<byte> buffer)
	{
		foreach (var field in Fields)
		{
			field.Deserialize(buffer);

			if (buffer.Length <= field.DataType.ByteSize)
				break;

			buffer = buffer.Slice(field.DataType.ByteSize);
		}
	}
}
