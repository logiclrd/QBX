using System;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Statements;

class CapturingTextLibrary(Machine machine, StringValue captureBuffer) : VisualLibrary(machine)
{
	public override void ScrollText() { }
	public override void ScrollTextWindow(int x1, int y1, int x2, int y2, int numLines, int fillAttribute) { }
	protected override void ClearImplementation(int fromCharacterLine = 0, int toCharacterLine = -1) { }

	public override void WriteText(ReadOnlySpan<byte> buffer)
	{
		captureBuffer.Append(buffer);
	}

	protected override void DrawPointer() { }
	protected override void UndrawPointer() { }
}
