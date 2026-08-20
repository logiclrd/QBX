using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public abstract class Executable
{
	public Sequence? Sequence;
	public int Index;

	public CodeModel.Statements.Statement? Source;

	public int LineNumberForErrorReporting;

	public Executable(CodeModel.Statements.Statement? source)
	{
		Source = source?.TrueSource ?? source;
		LineNumberForErrorReporting = Source?.LineNumberForErrorReporting ?? 0;
	}

	public StatementPath GetPathToStatement(int offset = 0)
	{
		var path = new StatementPath();

		GetPathToStatement(path, offset);

		return path;
	}

	public void GetPathToStatement(StatementPath path, int offset = 0)
	{
		if (Sequence == null)
			throw new Exception("Internal error: Executable does not have a reference to its containing Sequence");

		path.Push(Index + offset);

		if (Sequence is not Routine)
			Sequence.GetPathToStatement(path);
	}

	public virtual int GetSequenceCount() => 0;
	public virtual Sequence? GetSequenceByIndex(int sequenceIndex)
		=> throw new Exception("Internal error: Trying to GetSequenceByIndex on a statement that contains no sequences.");
	public virtual int IndexOfSequence(Sequence sequence) => -1;

	public virtual bool CanBreak { get; set; } = true;

	public virtual bool IsLabel => false;

	static int s_gcIntervalCounter;

	public void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		s_gcIntervalCounter++;

		if (s_gcIntervalCounter > 100000)
		{
			s_gcIntervalCounter = 0;

			GC.Collect(0, GCCollectionMode.Optimized, blocking: true);
			GC.WaitForPendingFinalizers();
		}

		ExecuteImplementation(context, stackFrame);
	}

	protected abstract void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame);

	public virtual bool SelfSequenceDispatch => false;

	public virtual void Dispatch(ExecutionContext context, StackFrame stackFrame, int sequenceIndex, ref StatementPath? goTo)
	{
		throw new Exception("Internal error: Executable.Dispatch called with no implementation");
	}
}
