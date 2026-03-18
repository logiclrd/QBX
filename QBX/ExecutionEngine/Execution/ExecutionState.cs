using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class ExecutionState : IReadOnlyExecutionState, IExecutionControls
{
	public IEnumerable<StackFrame> Stack => _stack;
	public RuntimeException? CurrentError => _currentError;
	public bool IsTerminated => _isTerminated;

	public event Action? EnterExecution;
	public event Action? ExitExecution;

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

		DebugOut("PROGRAM: EnterExecution");

		EnterExecution?.Invoke();
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

			DebugOut("PROGRAM: ExitExecution");

			ExitExecution?.Invoke();

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

			DebugOut("PROGRAM: ExitExecution");

			ExitExecution?.Invoke();

			DebugOut("PROGRAM: pulse");

			Monitor.PulseAll(_sync);

			while (_break)
			{
				DebugOut("PROGRAM: going to sleep");
				Monitor.Wait(_sync);
				DebugOut("PROGRAM: woke up");
			}

			DebugOut("PROGRAM: EnterExecution");

			EnterExecution?.Invoke();

			_waiting = 0;

			DebugOut("PROGRAM: resuming");
		}

		if (_step)
		{
			_break = true;
			_step = false;
		}
	}

	public void WaitForStartUp()
	{
		lock (_sync)
		{
			DebugOut("DEBUGGER: waiting for startup (assuming no stale wait on the program thread");

			while ((_waiting == 0) && !_isTerminated)
			{
				DebugOut("DEBUGGER: going to sleep");
				Monitor.Wait(_sync);
				DebugOut("DEBUGGER: woke up");
			}

			DebugOut("DEBUGGER: resuming waiting=" + _waiting + " isterminated=" + _isTerminated);
		}
	}

	public void WaitForInterruption()
	{
		lock (_sync)
		{
			DebugOut("DEBUGGER: waiting");

			_currentWait++;

			if (_currentWait == int.MaxValue) // ha ha!
				_currentWait = 1;

			DebugOut("DEBUGGER: => _waiting is " + _waiting + ", setting _currentWait to " + _currentWait);

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
	void DebugOut(string str)
	{
		if (DebugLog == null)
		{
			DebugLog = new List<(long, string)>();
			DebugLogByThreadID[Thread.CurrentThread.ManagedThreadId] = DebugLog;
		}

		DebugLog.Add((DateTime.Now.Ticks, str));
	}

	[ThreadStatic]
	static List<(long, string)>? DebugLog;

	static Dictionary<int, List<(long, string)>> DebugLogByThreadID = new();

	public static string GetMergedLog(int debuggerThreadID, int executionThreadID)
	{
		var combined = new List<(long, string)>();

		var debugger = DebugLogByThreadID[debuggerThreadID];
		var execution = DebugLogByThreadID[executionThreadID];

		int debuggerIndex = 0;
		int executionIndex = 0;

		while ((debuggerIndex < debugger.Count) && (executionIndex < execution.Count))
		{
			var debuggerLine = debugger[debuggerIndex];
			var executionLine = execution[executionIndex];

			if (debuggerLine.Item1 < executionLine.Item1)
			{
				combined.Add(debuggerLine);
				debuggerIndex++;
			}
			else
			{
				combined.Add(executionLine);
				executionIndex++;
			}
		}

		combined.AddRange(debugger.Skip(debuggerIndex));
		combined.AddRange(execution.Skip(executionIndex));

		var buffer = new StringWriter();

		var lastTime = new DateTime(combined[0].Item1);

		foreach (var item in combined)
		{
			var thisTime = new DateTime(item.Item1);

			if ((thisTime - lastTime).TotalMilliseconds > 50)
				buffer.WriteLine();

			lastTime = thisTime;

			buffer.WriteLine("{0}|{1}", item.Item1, item.Item2);
		}

		return buffer.ToString();
	}
}
