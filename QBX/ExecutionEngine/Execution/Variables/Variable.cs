using System;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public abstract class Variable
{
	public SurfacedVariable? SurfacedVariable;
	public SurfacedVariableDescriptor SurfacedVariableDescriptor;

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

	public static Variable AllocateAndConstructPinned(DataType type, int byteSize, ExecutionContext context)
	{
		if (type.IsArray)
			throw new Exception("Internal error: Construct called instead of ConstructArray for array type");
		if (byteSize < type.ByteSize)
			throw new Exception("Internal error: Not allocating enough memory for pinned value");

		int memoryAddress = context.Machine.DOS.MemoryManager.AllocateMemory(byteSize, context.Machine.DOS.CurrentPSPSegment);

		var variable = ConstructPinned(type, context, memoryAddress, byteSize);

		variable.PinnedMemoryContext = context;

		return variable;
	}

	public static Variable ConstructPinned(DataType type, ExecutionContext context, int memoryAddress, int byteSize)
	{
		if (type.IsArray)
			throw new Exception("Internal error: Construct called instead of ConstructArray for array type");
		else if (type.IsUserType)
			return UserDataTypeVariable.Pinned(type.UserType, context, memoryAddress);
		else
		{
			void ValidateByteSize(int required)
			{
				if (byteSize < required)
					throw new Exception("Internal error: pinned variable buffer is not large enough");
			}

			var machine = context.Machine;

			switch (type.PrimitiveType)
			{
				case PrimitiveDataType.Integer: ValidateByteSize(2);  return new PinnedIntegerVariable(machine, memoryAddress);
				case PrimitiveDataType.Long: ValidateByteSize(4);return new PinnedLongVariable(machine, memoryAddress);
				case PrimitiveDataType.Single: ValidateByteSize(4);return new PinnedSingleVariable(machine, memoryAddress);
				case PrimitiveDataType.Double: ValidateByteSize(8);return new PinnedDoubleVariable(machine, memoryAddress);
				case PrimitiveDataType.Currency: ValidateByteSize(8);return new PinnedCurrencyVariable(machine, memoryAddress);
				case PrimitiveDataType.String: return new PinnedStringVariable(machine, memoryAddress, byteSize);

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

	public virtual bool SelfAllocateAndPin => false;

	public virtual void AllocateAndPin(ExecutionContext context)
	{
		if (PinnedMemoryOwner != null)
			PinnedMemoryOwner.AllocateAndPin(context);
		else
			throw new Exception("Cannot allocate & pin a " + GetType().Name + " directly");
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
