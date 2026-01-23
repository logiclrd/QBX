using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution;

public class ArraySubscripts
{
	public List<ArraySubscript> Subscripts { get; } = new List<ArraySubscript>();

	public int Dimensions => Subscripts.Count;

	public ArraySubscript this[int index]
	{
		get => Subscripts[index];
		set => Subscripts[index] = value;
	}

	public int ElementCount => Subscripts.Select(subscript => subscript.ElementCount).Product();

	public int GetElementIndex(int[] subscriptValues, IList<Evaluable>? subscriptExpressions = null)
	{
		int index = 0;

		// By processing the subscripts in reverse order, a multidimensional array is effectively an array
		// of arrays of all the leading subscripts. When redimensioning with preservation, the elements
		// can simply be copied because their layout doesn't change, only the number of repetitions
		// changes.

		for (int i = Subscripts.Count - 1; i >= 0; i--)
		{
			int lowerBound = Subscripts[i].LowerBound;
			int upperBound = Subscripts[i].UpperBound;

			int subscript = subscriptValues[i];

			if ((subscript < lowerBound) || (subscript > upperBound))
				throw RuntimeException.SubscriptOutOfRange(subscriptExpressions?[i]?.Source);

			index = index * Subscripts[i].ElementCount + subscriptValues[i] - Subscripts[i].LowerBound;
		}

		return index;
	}

	public int GetSubarraySize()
	{
		// Data is laid out in the array so that each successive dimension can be interpreted
		// as an array of the array described by the preceding dimensions.
		//
		// (1 TO 10) = 10 consecutive integers
		// (1 TO 10, 1 TO 5) = 5 consecutive copies of (1 TO 10)
		// (1 TO 10, 1 TO 5, 1 TO 8) = 8 consecutive copies of (1 TO 10, 1 TO 5)
		//
		// This allows for REDIM PRESERVE, with its restriction that only the upper bound of
		// the last dimension can be changed, to be implemented as a raw copy of the elements
		// without having to reorganize the array.
		//
		// This copy requires knowing the size of that second-to-last subarray, e.g.
		// (1 TO 10, 1 TO 5) in the example above.

		int size = 1;

		for (int i = Subscripts.Count - 2; i >= 0; i--)
			size *= Subscripts[i].ElementCount;

		return size;
	}

	[ThreadStatic]
	static int[]? s_subscriptValueBuffer;

	public int GetElementIndex(Variable[] subscriptValues, IList<Evaluable> subscriptExpressions)
	{
		if ((s_subscriptValueBuffer == null) || (s_subscriptValueBuffer.Length < subscriptValues.Length))
			s_subscriptValueBuffer = new int[subscriptValues.Length * 2];

		for (int i = 0; i < subscriptValues.Length; i++)
			s_subscriptValueBuffer[i] = subscriptValues[i].CoerceToInt(context: subscriptExpressions[i]);

		return GetElementIndex(s_subscriptValueBuffer, subscriptExpressions);
	}
}
