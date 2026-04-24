using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResetPaletteStatement(CodeModel.Statements.PaletteStatement source) : Executable(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		context.Machine.VideoFirmware.ResetPalette();
	}
}
