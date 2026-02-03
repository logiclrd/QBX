using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

using static QBX.Hardware.GraphicsArray;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ClearStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? StringSpaceExpression;
	public Evaluable? MaximumMemoryAddressExpression;
	public Evaluable? StackSpaceExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		// CLEAR [, , stack&]
		//
		// The first two arguments are no longer used, but historically are:
		// - String Space
		// - Maximum Memory Address (i.e., amount of memory available to QB)
		//
		// The third argument, stack space size, is also meaningless to us.
		// What we do here: zero out all variables, deallocate all dynamic
		// arrays, clear the GOSUB return stack, close all open files.
		//
		// NB: This statement is only valid in the main module and thus
		// only applies when there is only the root stack frame.

		context.Reset();
	}
}
