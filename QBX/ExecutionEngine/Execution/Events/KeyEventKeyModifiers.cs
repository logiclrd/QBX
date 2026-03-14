using System;

namespace QBX.ExecutionEngine.Execution.Events;

[Flags]
public enum KeyEventKeyModifiers : byte
{
	None = 0,

	Shift = 3,
	Control = 4,
	Alt = 8,
	NumLock = 32,
	CapsLock = 64,
	Extended = 128,
}

