using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution.Variables;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution;

public class ArraySubscripts
{
	public List<ArraySubscript> Subscripts { get; } = new List<ArraySubscript>();

	public int ElementCount => Subscripts.Select(subscript => subscript.ElementCount).Product();

	public int GetElementIndex(int[] subscriptValues)
	{
		int index = 0;

		for (int i = 0; i < Subscripts.Count; i++)
			index = index * Subscripts[i].ElementCount + subscriptValues[i] - Subscripts[i].LowerBound;

		return index;
	}

	[ThreadStatic]
	static int[]? s_subscriptValueBuffer;

	public int GetElementIndex(Variable[] subscriptValues)
	{
		if ((s_subscriptValueBuffer == null) || (s_subscriptValueBuffer.Length < subscriptValues.Length))
			s_subscriptValueBuffer = new int[subscriptValues.Length * 2];

		for (int i = 0; i < subscriptValues.Length; i++)
			s_subscriptValueBuffer[i] = subscriptValues[i].CoerceToInt();

		return GetElementIndex(s_subscriptValueBuffer);
	}
}
