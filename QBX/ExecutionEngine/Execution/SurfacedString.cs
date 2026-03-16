using System;

using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class SurfacedString
{
	public WeakReference<StackFrame>? StackFrame;
	public WeakReference<ArrayVariable>? Array;
	public int Index;

	public StringVariable? Get()
	{
		if (StackFrame != null)
		{
			if (StackFrame.TryGetTarget(out var stackFrame))
				return stackFrame.Variables[Index] as StringVariable;
		}

		if (Array != null)
		{
			if (Array.TryGetTarget(out var array))
			{
				array.Array.EnsureUnpacked();

				if (Index < array.Array.Elements.Length)
					return array.Array.GetElement(Index) as StringVariable;
			}
		}

		return null;
	}
}
