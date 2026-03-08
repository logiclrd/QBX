using QBX.ExecutionEngine.Execution;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CapturingPrintEmitter(Machine machine, StringValue captureTarget)
	: VisualPrintEmitter(new CapturingTextLibrary(machine, captureTarget))
{
}
