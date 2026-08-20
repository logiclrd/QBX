using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResetPaletteStatement(CodeModel.Statements.PaletteStatement source) : Executable(source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		context.Machine.VideoFirmware.ResetPalette();
	}
}
