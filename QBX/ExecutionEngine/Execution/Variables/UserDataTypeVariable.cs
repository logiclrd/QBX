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
		{
			var field = dataType.Fields[i];

			var arraySubscripts = field.ArraySubscripts;

			if (arraySubscripts == null)
			{
				if (field.Type.IsString) // strings in UDTs are fixed length
					Fields[i] = new StringVariable(fixedStringLength: field.Type.ByteSize);
				else
					Fields[i] = Variable.Construct(field.Type);
			}
			else
			{
				var array =
					field.Type.IsString
					? Variable.ConstructArrayOfFixedLengthString(fixedLength: field.Type.ByteSize)
					: Variable.ConstructArray(field.Type);

				array.InitializeArray(arraySubscripts);

				Fields[i] = array;
			}
		}
	}

	public override void Reset()
	{
		foreach (var field in Fields)
			field.Reset();
	}

	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;

	public override int CoerceToInt(Evaluable? context) => throw RuntimeException.TypeMismatch(context?.Source);

	public override object GetData() => this;

	public override void SetData(object value)
	{
		if ((value is not UserDataTypeVariable otherUDT)
		 || !otherUDT.DataType.Equals(DataType))
			throw RuntimeException.TypeMismatch();

		if (Fields.Length != otherUDT.Fields.Length)
			throw new Exception("Internal error: UDT field mismatch");

		for (int i = 0; i < Fields.Length; i++)
			Fields[i].SetData(otherUDT.Fields[i].GetData());
	}

	public override int Serialize(Span<byte> buffer)
	{
		foreach (var field in Fields)
		{
			int byteSize = field.Serialize(buffer);

			if (buffer.Length <= byteSize)
				break;

			buffer = buffer.Slice(byteSize);
		}

		return DataType.ByteSize;
	}

	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		foreach (var field in Fields)
		{
			int byteSize = field.Deserialize(buffer);

			if (buffer.Length <= byteSize)
				break;

			buffer = buffer.Slice(byteSize);
		}

		return DataType.ByteSize;
	}
}
