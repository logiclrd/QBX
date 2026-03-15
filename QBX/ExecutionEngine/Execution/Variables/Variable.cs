using System;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public abstract class Variable
{
	public static Variable Construct(DataType type)
	{
		if (type.IsArray)
			throw new Exception("Internal error: Construct called instead of ConstructArray for array type");
		else if (type.IsUserType)
			return new UserDataTypeVariable(type.UserType);
		else
		{
			switch (type.PrimitiveType)
			{
				case PrimitiveDataType.Integer: return new IntegerVariable();
				case PrimitiveDataType.Long: return new LongVariable();
				case PrimitiveDataType.Single: return new SingleVariable();
				case PrimitiveDataType.Double: return new DoubleVariable();
				case PrimitiveDataType.Currency: return new CurrencyVariable();
				case PrimitiveDataType.String: return new StringVariable(type.ByteSize);

				default: throw new Exception("Internal error: Unrecognized data type in Variable.Construct " + type);
			}
		}
	}

	public static Variable ConstructPinned(DataType type, ExecutionContext context, int memoryAddress)
	{
		if (type.IsArray)
			throw new Exception("Internal error: Construct called instead of ConstructArray for array type");
		else if (type.IsUserType)
			return UserDataTypeVariable.Pinned(type.UserType, context, memoryAddress);
		else
		{
			var machine = context.Machine;

			switch (type.PrimitiveType)
			{
				case PrimitiveDataType.Integer: return new PinnedIntegerVariable(machine, memoryAddress);
				case PrimitiveDataType.Long: return new PinnedLongVariable(machine, memoryAddress);
				case PrimitiveDataType.Single: return new PinnedSingleVariable(machine, memoryAddress);
				case PrimitiveDataType.Double: return new PinnedDoubleVariable(machine, memoryAddress);
				case PrimitiveDataType.Currency: return new PinnedCurrencyVariable(machine, memoryAddress);
				case PrimitiveDataType.String: return new PinnedStringVariable(machine, memoryAddress, type.ByteSize);

				default: throw new Exception("Internal error: Unrecognized data type in Variable.Construct " + type);
			}
		}
	}

	public static ArrayVariable ConstructArray(DataType type)
		=> new ArrayVariable(type);

	public static ArrayVariable ConstructArrayOfFixedLengthString(int fixedLength)
		=> new ArrayVariable(DataType.String, fixedStringLength: fixedLength);

	public DataType DataType { get; }

	internal Variable? PinnedMemoryOwner;
	internal int PinnedMemoryAddress = -1;
	internal ExecutionContext? PinnedMemoryContext = null;

	internal bool IsPinned => (PinnedMemoryAddress >= 0);

	public Variable(DataType dataType)
	{
		DataType = dataType;
	}

	protected internal void AllocatePinnedMemory(ExecutionContext context, int byteSize = -1)
	{
		if (byteSize < 0)
			byteSize = DataType.ByteSize;

		PinnedMemoryContext = context;
		PinnedMemoryAddress = PinnedMemoryContext.Machine.DOS.MemoryManager.AllocateMemory(byteSize, context.Machine.DOS.CurrentPSPSegment);
	}

	protected internal void ReleasePinnedMemory()
	{
		if (PinnedMemoryContext != null)
		{
			PinnedMemoryContext.QueuePinnedMemoryRelease(PinnedMemoryAddress);
			PinnedMemoryContext = null;
		}
	}

	~Variable()
	{
		ReleasePinnedMemory();
	}

	public virtual void ReadPinnedData() { }
	public virtual void WritePinnedData() { }

	public abstract object GetData();
	public abstract void SetData(object value);
	public abstract int CoerceToInt(Evaluable? context);

	public abstract int Serialize(Span<byte> buffer);
	public abstract int Deserialize(ReadOnlySpan<byte> buffer);

	public abstract void Reset();

	public abstract bool IsZero { get; }
	public abstract bool IsPositive { get; }
	public abstract bool IsNegative { get; }
}
