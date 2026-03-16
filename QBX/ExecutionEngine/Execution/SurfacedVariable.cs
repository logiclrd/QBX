using System;

using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class SurfacedVariable
{
	public WeakReference<StackFrame>? StackFrame;
	public WeakReference<ArrayVariable>? Array;
	public int Index;

	public bool IsValid
	{
		get
		{
			if (StackFrame != null)
			{
				if (StackFrame.TryGetTarget(out var stackFrame))
					return Index < stackFrame.Variables.Length;
			}
			else if (Array != null)
			{
				if (Array.TryGetTarget(out var array))
					return Index < array.Array.Elements.Length;
			}

			return false;
		}
	}

	public Variable? Get()
	{
		if (StackFrame != null)
		{
			if (StackFrame.TryGetTarget(out var stackFrame))
				return stackFrame.Variables[Index];
		}

		if (Array != null)
		{
			if (Array.TryGetTarget(out var array))
			{
				array.Array.EnsureUnpacked();

				if (Index < array.Array.Elements.Length)
					return array.Array.GetElement(Index);
			}
		}

		return null;
	}
}
