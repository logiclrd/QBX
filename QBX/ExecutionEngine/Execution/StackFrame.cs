using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class StackFrame(Routine routine, Variable[] variables)
{
	public readonly Routine Routine = routine;
	public readonly Module Module = routine.Module;
	public readonly Variable[] Variables = variables;
	public CodeModel.Statements.Statement? CurrentStatement;

	public bool IsModuleFrame;

	public StackFrame ModuleFrame => Routine.Module.ModuleFrame ?? throw new Exception("Internal error: Module frame is not initialized");

	public bool IsHandlingError;
	public ErrorHandler? NewLocalErrorHandler;

	// FOR loops have this interesting property that they capture
	// state pertinent to the current stack frame. The from, to and
	// step values can be different for different concurrent
	// executions of the same FOR loop in different frames. So,
	// we need a way to separate out these things per-StackFrame.
	public Dictionary<long, Executable> NextStatements = new Dictionary<long, Executable>();

	Stack<StatementPath> _goSubStack = new Stack<StatementPath>();

	public void PushReturnPath(StatementPath returnPath)
	{
		_goSubStack.Push(returnPath);
	}

	public StatementPath PopReturnPath(CodeModel.Statements.Statement? context)
	{
		if (!_goSubStack.TryPop(out var path))
			throw RuntimeException.ReturnWithoutGoSub(context);

		return path;
	}

	public void Reset()
	{
		foreach (var variable in Variables)
			variable.Reset();

		_goSubStack.Clear();
	}
}
