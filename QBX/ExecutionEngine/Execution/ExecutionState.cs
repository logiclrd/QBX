using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class ExecutionState : IReadOnlyExecutionState, IExecutionControls
{
	public IEnumerable<StackFrame> Stack => _stack;
	public RuntimeException? CurrentError => _currentError;
	public bool IsTerminated => _isTerminated;

	public event Func<StackFrame, bool>? CheckWatchpoints;

	Stack<StackFrame> _stack = new Stack<StackFrame>();
	RuntimeException? _currentError = null;
	bool _isTerminated;

	int _stepOverNesting;

	object _sync = new object();
	volatile bool _break = false;
	volatile bool _breakOnReturn = false;
	volatile bool _step = false;
	volatile int _waiting = 0;
	volatile int _currentWait = 1;

	// Controls:
	public void ContinueExecution()
	{
		if (_isTerminated)
			throw new InvalidOperationException("Cannot continue execution after it has been terminated");

		lock (_sync)
		{
			_break = false;
			_step = false;
			_breakOnReturn = false;

			Debug.WriteLine("DEBUGGER: break=false step=false breakonreturn=false pulse");

			Monitor.PulseAll(_sync);
		}
	}

	public void StepOverNextRoutine()
	{
		if (_isTerminated)
			throw new InvalidOperationException("Cannot continue execution after it has been terminated");

		lock (_sync)
		{
			_stepOverNesting = 0;
			_breakOnReturn = true;

			DebugOut("DEBUGGER: breakonreturn=true pulse");

			Monitor.PulseAll(_sync);
		}
	}

	public void ExecuteOneStatement()
	{
		if (_isTerminated)
			throw new InvalidOperationException("Cannot continue execution after it has been terminated");

		lock (_sync)
		{
			_break = false;
			_step = true;

			DebugOut("DEBUGGER: step=true pulse");

			Monitor.PulseAll(_sync);
		}
	}

	public void Break()
	{
		_break = true;
	}

	public void Terminate()
	{
		_isTerminated = true;

		// Can't call ContinueExecution because it would just throw
		lock (_sync)
		{
			_break = false;
			_step = false;
			_breakOnReturn = false;

			DebugOut("DEBUGGER: terminate");

			Monitor.PulseAll(_sync);
		}
	}

	// Controller:
	public void StartExecution(StackFrame rootFrame)
	{
		_stack.Push(rootFrame);
	}

	public void EnterRoutine(Routine routine, StackFrame stackFrame)
	{
		if (_isTerminated)
			throw new TerminatedException();

		_stack.Push(stackFrame);

		if (_breakOnReturn)
		{
			_stepOverNesting++;
			DebugOut("PROGRAM: step over nesting " + _stepOverNesting);
		}
	}

	public void NextStatement(CodeModel.Statements.Statement? statement)
	{
		DebugOut("PROGRAM: beginning statement");

		if (_isTerminated)
			throw new TerminatedException();

		if (!_stack.TryPeek(out var currentStackFrame))
			throw new Exception("Cannot call NextStatement when the ExecutionState is terminated");

		currentStackFrame.CurrentStatement = statement;

		if ((statement != null) && statement.IsBreakpoint)
			_break = true;
		else if (CheckWatchpoints?.Invoke(currentStackFrame) ?? false)
			_break = true;

		if (_break)
		{
			DebugOut("PROGRAM: break");
			WaitToContinue();
		}
	}

	public void Error(RuntimeException error)
	{
		_currentError = error;
		_break = true;

		DebugOut("PROGRAM: error pause");
		WaitToContinue();

		_currentError = null;
	}

	public void ExitRoutine()
	{
		if (_isTerminated)
			throw new TerminatedException();

		_stack.Pop();

		if (_breakOnReturn)
		{
			_stepOverNesting--;

			DebugOut("PROGRAM: step over nesting " + _stepOverNesting);

			if (_stepOverNesting <= 0)
			{
				DebugOut("PROGRAM: breakonreturn=false break=true");

				_breakOnReturn = false;
				_break = true;
			}
		}
	}

	public void EndExecution()
	{
		lock (_sync)
		{
			DebugOut("PROGRAM: ended");

			_isTerminated = true;

			DebugOut("PROGRAM: pulse");

			Monitor.PulseAll(_sync);
		}
	}

	public void WaitToContinue()
	{
		lock (_sync)
		{
			DebugOut("PROGRAM: waiting");

			_waiting = _currentWait;

			DebugOut("PROGRAM: pulse");

			Monitor.PulseAll(_sync);

			while (_break)
			{
				DebugOut("PROGRAM: going to sleep");
				Monitor.Wait(_sync);
				DebugOut("PROGRAM: woke up");
			}

			_waiting = 0;

			DebugOut("PROGRAM: resuming");
		}

		if (_step)
		{
			_break = true;
			_step = false;
		}
	}

	public void WaitForInterruption()
	{
		lock (_sync)
		{
			DebugOut("DEBUGGER: waiting");

			_currentWait++;

			DebugOut("DEBUGGER: => _waiting is zero, setting _currentWait to " + _currentWait);

			if (_currentWait == int.MaxValue) // ha ha!
				_currentWait = 1;

			while ((_waiting != _currentWait) && !_isTerminated)
			{
				DebugOut("DEBUGGER: going to sleep");
				Monitor.Wait(_sync);
				DebugOut("DEBUGGER: woke up");
			}

			DebugOut("DEBUGGER: resuming waiting=" + _waiting + " isterminated=" + _isTerminated);
		}
	}

	[Conditional("TRACEDEBUGCONTROL")]
	void DebugOut(string str) => Debug.WriteLine(str);
}
