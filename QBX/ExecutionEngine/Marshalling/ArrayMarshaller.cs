using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

using Array = QBX.ExecutionEngine.Execution.Array;

namespace QBX.ExecutionEngine.Marshalling;

public abstract class ArrayMarshaller : Marshaller
{
	public static ArrayMarshaller Construct(PrimitiveDataType type)
	{
		switch (type)
		{
			case PrimitiveDataType.Integer: return new IntegerArrayMapper();
			case PrimitiveDataType.Long: return new LongArrayMapper();
			case PrimitiveDataType.Single: return new SingleArrayMapper();
			case PrimitiveDataType.Double: return new DoubleArrayMapper();
			case PrimitiveDataType.Currency: return new CurrencyArrayMapper();

			default: throw CompilerException.TypeMismatch();
		}
	}
}

public class IntegerArrayMapper : ArrayMarshaller
{
	public override void Map(object from, ref object? to)
	{
		Array? fromArray = null;

		if (from is ArrayVariable arrayVariable)
			fromArray = arrayVariable.Array;
		else if (from is Array array)
			fromArray = array;

		Span<byte> fromArrayData;

		if (fromArray != null)
		{
			fromArray.EnsurePacked();
			fromArrayData = fromArray.PackedData;
		}
		else if (from is short[] data)
			fromArrayData = MemoryMarshal.Cast<short, byte>(data.AsSpan());
		else
			throw RuntimeException.TypeMismatch();

		Array? targetArray = null;

		if (to is ArrayVariable targetVariable)
			targetArray = targetVariable.Array;
		else if (to is Array toArray)
			targetArray = toArray;

		Span<byte> targetArrayData = default;

		if (targetArray != null)
		{
			targetArray.EnsurePacked();
			targetArrayData = targetArray.PackedData;
		}
		else if (to is short[] toNativeArray)
			targetArrayData = MemoryMarshal.Cast<short, byte>(toNativeArray.AsSpan());

		if (targetArrayData != Span<byte>.Empty)
			fromArrayData.CopyTo(targetArrayData);
		else
		{
			var typedData = MemoryMarshal.Cast<byte, short>(fromArrayData);

			to = typedData.ToArray();

			return;
		}
	}
}

public class LongArrayMapper : ArrayMarshaller
{
	public override void Map(object from, ref object? to)
	{
		Array? fromArray = null;

		if (from is ArrayVariable arrayVariable)
			fromArray = arrayVariable.Array;
		else if (from is Array array)
			fromArray = array;

		Span<byte> fromArrayData;

		if (fromArray != null)
		{
			fromArray.EnsurePacked();
			fromArrayData = fromArray.PackedData;
		}
		else if (from is short[] data)
			fromArrayData = MemoryMarshal.Cast<short, byte>(data.AsSpan());
		else
			throw RuntimeException.TypeMismatch();

		Array? targetArray = null;

		if (to is ArrayVariable targetVariable)
			targetArray = targetVariable.Array;
		else if (to is Array toArray)
			targetArray = toArray;

		Span<byte> targetArrayData = default;

		if (targetArray != null)
		{
			targetArray.EnsurePacked();
			targetArrayData = targetArray.PackedData;
		}
		else if (to is short[] toNativeArray)
			targetArrayData = MemoryMarshal.Cast<short, byte>(toNativeArray.AsSpan());

		if (targetArrayData != Span<byte>.Empty)
			fromArrayData.CopyTo(targetArrayData);
		else
		{
			var typedData = MemoryMarshal.Cast<byte, short>(fromArrayData);

			to = typedData.ToArray();

			return;
		}
	}
}

public class SingleArrayMapper : ArrayMarshaller
{
	public override void Map(object from, ref object? to)
	{
		Array? fromArray = null;

		if (from is ArrayVariable arrayVariable)
			fromArray = arrayVariable.Array;
		else if (from is Array array)
			fromArray = array;

		Span<byte> fromArrayData;

		if (fromArray != null)
		{
			fromArray.EnsurePacked();
			fromArrayData = fromArray.PackedData;
		}
		else if (from is short[] data)
			fromArrayData = MemoryMarshal.Cast<short, byte>(data.AsSpan());
		else
			throw RuntimeException.TypeMismatch();

		Array? targetArray = null;

		if (to is ArrayVariable targetVariable)
			targetArray = targetVariable.Array;
		else if (to is Array toArray)
			targetArray = toArray;

		Span<byte> targetArrayData = default;

		if (targetArray != null)
		{
			targetArray.EnsurePacked();
			targetArrayData = targetArray.PackedData;
		}
		else if (to is short[] toNativeArray)
			targetArrayData = MemoryMarshal.Cast<short, byte>(toNativeArray.AsSpan());

		if (targetArrayData != Span<byte>.Empty)
			fromArrayData.CopyTo(targetArrayData);
		else
		{
			var typedData = MemoryMarshal.Cast<byte, short>(fromArrayData);

			to = typedData.ToArray();

			return;
		}
	}
}

public class DoubleArrayMapper : ArrayMarshaller
{
	public override void Map(object from, ref object? to)
	{
		Array? fromArray = null;

		if (from is ArrayVariable arrayVariable)
			fromArray = arrayVariable.Array;
		else if (from is Array array)
			fromArray = array;

		Span<byte> fromArrayData;

		if (fromArray != null)
		{
			fromArray.EnsurePacked();
			fromArrayData = fromArray.PackedData;
		}
		else if (from is short[] data)
			fromArrayData = MemoryMarshal.Cast<short, byte>(data.AsSpan());
		else
			throw RuntimeException.TypeMismatch();

		Array? targetArray = null;

		if (to is ArrayVariable targetVariable)
			targetArray = targetVariable.Array;
		else if (to is Array toArray)
			targetArray = toArray;

		Span<byte> targetArrayData = default;

		if (targetArray != null)
		{
			targetArray.EnsurePacked();
			targetArrayData = targetArray.PackedData;
		}
		else if (to is short[] toNativeArray)
			targetArrayData = MemoryMarshal.Cast<short, byte>(toNativeArray.AsSpan());

		if (targetArrayData != Span<byte>.Empty)
			fromArrayData.CopyTo(targetArrayData);
		else
		{
			var typedData = MemoryMarshal.Cast<byte, short>(fromArrayData);

			to = typedData.ToArray();

			return;
		}
	}
}

public class CurrencyArrayMapper : ArrayMarshaller
{
	public override void Map(object from, ref object? to)
	{
		Array? fromArray = null;

		if (from is ArrayVariable arrayVariable)
			fromArray = arrayVariable.Array;
		else if (from is Array array)
			fromArray = array;

		Span<byte> fromArrayData;

		if (fromArray != null)
		{
			fromArray.EnsurePacked();
			fromArrayData = fromArray.PackedData;
		}
		else if (from is short[] data)
			fromArrayData = MemoryMarshal.Cast<short, byte>(data.AsSpan());
		else
			throw RuntimeException.TypeMismatch();

		Array? targetArray = null;

		if (to is ArrayVariable targetVariable)
			targetArray = targetVariable.Array;
		else if (to is Array toArray)
			targetArray = toArray;

		Span<byte> targetArrayData = default;

		if (targetArray != null)
		{
			targetArray.EnsurePacked();
			targetArrayData = targetArray.PackedData;
		}
		else if (to is short[] toNativeArray)
			targetArrayData = MemoryMarshal.Cast<short, byte>(toNativeArray.AsSpan());

		if (targetArrayData != Span<byte>.Empty)
			fromArrayData.CopyTo(targetArrayData);
		else
		{
			var typedData = MemoryMarshal.Cast<byte, short>(fromArrayData);

			to = typedData.ToArray();

			return;
		}
	}
}

public class UserDataTypeArrayMapper : ArrayMarshaller
{
	public static UserDataTypeArrayMapper Construct(UserDataType userDataType, Type nativeElementType)
		=> new UserDataTypeArrayMapper(userDataType, nativeElementType);

	UserDataType _userDataType;
	UserDataTypeMarshaller _mapper;

	UserDataTypeArrayMapper(UserDataType userDataType, Type nativeElementType)
	{
		_userDataType = userDataType;
		_mapper = UserDataTypeMarshaller.Construct(userDataType, nativeElementType);
	}

	public override void Map(object from, ref object? to)
	{
		Array? fromArray = null;

		if (from is ArrayVariable arrayVariable)
			fromArray = arrayVariable.Array;
		else if (from is Array array)
			fromArray = array;

		Span<byte> fromArrayData;

		if (fromArray != null)
		{
			fromArray.EnsurePacked();
			fromArrayData = fromArray.PackedData;
		}
		else if (from is short[] data)
			fromArrayData = MemoryMarshal.Cast<short, byte>(data.AsSpan());
		else
			throw RuntimeException.TypeMismatch();

		Array? targetArray = null;

		if (to is ArrayVariable targetVariable)
			targetArray = targetVariable.Array;
		else if (to is Array toArray)
			targetArray = toArray;

		Span<byte> targetArrayData = default;

		if (targetArray != null)
		{
			targetArray.EnsurePacked();
			targetArrayData = targetArray.PackedData;
		}
		else if (to is short[] toNativeArray)
			targetArrayData = MemoryMarshal.Cast<short, byte>(toNativeArray.AsSpan());

		if (targetArrayData != Span<byte>.Empty)
			fromArrayData.CopyTo(targetArrayData);
		else
		{
			var typedData = MemoryMarshal.Cast<byte, short>(fromArrayData);

			to = typedData.ToArray();

			return;
		}
	}
}
