using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QBX.Utility;

public static class ReadOnlyListExtensions
{
	[ThreadStatic]
	static Dictionary<Type, bool>? _typeIsBitwiseEquatable;

	static bool IsBitwiseEquatable<T>()
		=> IsBitwiseEquatable(typeof(T));

	static bool IsBitwiseEquatable(Type t)
	{
		_typeIsBitwiseEquatable ??= new Dictionary<Type, bool>();

		if (!_typeIsBitwiseEquatable.TryGetValue(t, out var bitwiseEquatable))
		{
			if (t.IsPrimitive)
				bitwiseEquatable = true;
			else
			{
				var equalsMethod = t.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance, binder: null, [typeof(object)], modifiers: null)!;

				var defaultBaseType = t.IsValueType ? typeof(ValueType) : typeof(object);

				var iequatableType = typeof(IEquatable<>).MakeGenericType(t);

				bool implementsIEquatable = iequatableType.IsAssignableFrom(t);

				bitwiseEquatable = (equalsMethod.DeclaringType == defaultBaseType);
			}

			_typeIsBitwiseEquatable[t] = bitwiseEquatable;
		}

		return bitwiseEquatable;
	}

	public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
	{
		if (list is IList<T> mutableList)
			return mutableList.IndexOf(item);
		else if (item == null)
		{
			for (int i = 0; i < list.Count; i++)
				if (list[i] == null)
					return i;

			return -1;
		}
		else if (!IsBitwiseEquatable<T>())
		{
			for (int i = 0; i < list.Count; i++)
				if (item.Equals(list[i]))
					return i;

			return -1;
		}
		else
			return IndexOfUsingBitwiseProcessor(list, item);
	}

	static int IndexOfUsingBitwiseProcessor<T>(IReadOnlyList<T> list, T item)
	{
		// Keep this isolated so that BitwiseIndexOfProcessor<T> only gets instantiated
		// when we know we need this path.
		return BitwiseIndexOfProcessor<T>.IndexOf(list, item);
	}

	class BitwiseIndexOfProcessor<T>
	{
		public static Func<IReadOnlyList<T>, T, int> IndexOf;

		static int TSizeWhenUnmanaged()
		{
			return (int)typeof(BitwiseIndexOfProcessor<T>)
				.GetMethod(nameof(USize), BindingFlags.NonPublic | BindingFlags.Static)!
				.MakeGenericMethod(typeof(T))
				.Invoke(null, null)!;
		}

		static unsafe int USize<U>() where U : unmanaged => sizeof(U);

		static BitwiseIndexOfProcessor()
		{
			string methodName;

			if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()
			 && TSizeWhenUnmanaged() <= 16)
				methodName = nameof(IndexOfBitwiseUnmanagedValueType);
			else if (typeof(T).IsValueType)
				methodName = nameof(IndexOfBitwiseManagedValueType);
			else
				methodName = nameof(IndexOfBitwiseReferenceType);

			var method = typeof(BitwiseIndexOfProcessor<T>)
				.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

			method = method?.MakeGenericMethod(typeof(T));

			if (method == null)
				throw new Exception("Internal error");

			IndexOf = method.CreateDelegate<Func<IReadOnlyList<T>, T, int>>();
		}

		static int IndexOfBitwiseUnmanagedValueType<U>(IReadOnlyList<U> list, U item)
			where U : unmanaged
		{
			// Compare elements explicitly, as generics don't allow us to '==' elements.
			const int BatchSize = 8;

			Span<U> batchBuffer = stackalloc U[BatchSize];

			for (int i = 0; i < list.Count; i += BatchSize)
			{
				int thisBatchSize = Math.Min(BatchSize, list.Count - i);

				for (int j = 0; j < thisBatchSize; j++)
					batchBuffer[j] = list[i + j];

				int batchIndex = batchBuffer.IndexOf(item);

				if (batchIndex >= 0)
					return i + batchIndex;
			}

			return -1;
		}

		static int IndexOfBitwiseManagedValueType<U>(IReadOnlyList<U> list, U item)
			where U : struct
		{
			// Compare elements explicitly, as generics don't allow us to '==' elements.
			var itemSpan = MemoryMarshal.CreateReadOnlySpan(ref item, 1);

			for (int i = 0; i < list.Count; i++)
			{
				var element = list[i]; // Copies the value to local storage
				var elementSpan = MemoryMarshal.CreateReadOnlySpan(ref element, 1);

				if (itemSpan.SequenceEqual(elementSpan))
					return i;
			}

			return -1;
		}

		static int IndexOfBitwiseReferenceType<U>(IReadOnlyList<U> list, U item)
			where U : class
		{
			// Guaranteed to always test reference equality.

			for (int i = 0; i < list.Count; i++)
				if (list[i] == item)
					return i;

			return -1;
		}
	}
}

