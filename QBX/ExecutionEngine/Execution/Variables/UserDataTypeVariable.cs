using System;
using System.Diagnostics.CodeAnalysis;

using QBX.ExecutionEngine.Compiled;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution.Variables;

public class UserDataTypeVariable : Variable
{
	public Variable[] Fields;

	public UserDataTypeVariable(UserDataType dataType)
		: base(new DataType(dataType))
	{
		ConstructConcreteInstance(dataType);
	}

	UserDataTypeVariable(UserDataType dataType, ExecutionContext context, int memoryAddress)
		: base(new DataType(dataType))
	{
		ConstructPinnedInstance(dataType, context, memoryAddress);
	}

	public static UserDataTypeVariable Pinned(UserDataType dataType, ExecutionContext context, int memoryAddress)
		=> new UserDataTypeVariable(dataType, context, memoryAddress);

	[MemberNotNull(nameof(Fields))]
	void ConstructConcreteInstance(UserDataType dataType)
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

			Fields[i].PinnedMemoryOwner = this;
		}
	}

	[MemberNotNull(nameof(Fields))]
	void ConstructPinnedInstance(UserDataType dataType, ExecutionContext context, int memoryAddress)
	{
		var machine = context.Machine;

		PinnedMemoryContext = context;
		PinnedMemoryAddress = memoryAddress;

		Fields = new Variable[dataType.Fields.Count];

		for (int i = 0; i < dataType.Fields.Count; i++)
		{
			var field = dataType.Fields[i];

			var arraySubscripts = field.ArraySubscripts;

			if (arraySubscripts == null)
			{
				if (field.Type.IsString) // strings in UDTs are fixed length
					Fields[i] = new PinnedStringVariable(machine, memoryAddress, length: field.Type.ByteSize);
				else
					Fields[i] = Variable.ConstructPinned(field.Type, context, memoryAddress, field.Type.ByteSize);

				memoryAddress += field.Type.ByteSize;
			}
			else
			{
				var array =
					field.Type.IsString
					? Variable.ConstructArrayOfFixedLengthString(fixedLength: field.Type.ByteSize)
					: Variable.ConstructArray(field.Type);

				array.InitializePinnedArray(arraySubscripts, context, memoryAddress);

				Fields[i] = array;

				memoryAddress += arraySubscripts.ElementCount * field.Type.ByteSize;
			}

			Fields[i].PinnedMemoryOwner = this;
		}
	}

	public override bool SelfAllocateAndPin => true;

	public override void AllocateAndPin(ExecutionContext context)
	{
		if (IsPinned)
			return;

		if (PinnedMemoryOwner != null)
		{
			PinnedMemoryOwner.AllocateAndPin(context);
			return;
		}

		AllocatePinnedMemory(context);

		var userDataType = base.DataType.UserType ?? throw new Exception("Internal error");

		var pinSpan = context.Machine.SystemMemory.AsSpan().Slice(PinnedMemoryAddress, DataType.ByteSize);

		Serialize(pinSpan);

		ConstructPinnedInstance(userDataType, context, PinnedMemoryAddress);
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
