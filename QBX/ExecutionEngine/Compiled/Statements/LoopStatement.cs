using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class LoopStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public LoopType Type;

	public static LoopStatement ConstructUnconditionalLoop(Sequence body, CodeModel.Statements.Statement source)
	{
		return new UnconditionalLoopStatement(body, source);
	}

	public static LoopStatement ConstructPreConditionLoop(Evaluable condition, Sequence body, CodeModel.Statements.Statement conditionSource)
	{
		return ConditionalLoopStatement.ConstructPreConditionLoop(condition, body, conditionSource);
	}

	public static LoopStatement ConstructPostConditionLoop(Sequence body, Evaluable condition, CodeModel.Statements.Statement conditionSource, CodeModel.Statements.Statement loopSource)
	{
		return ConditionalLoopStatement.ConstructPostConditionLoop(body, condition, conditionSource, loopSource);
	}

	class UnconditionalLoopStatement(Sequence body, CodeModel.Statements.Statement source)
		: LoopStatement(source)
	{
		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				while (true)
				{
					System.Threading.Thread.Yield();
					context.Dispatch(body, stackFrame);
				}
			}
			catch (ExitDo) when (Type == LoopType.Do)
			{
			}
		}
	}
}

public abstract class ConditionalLoopStatement : LoopStatement
{
	public Sequence Body;

	Sequence? _firstSequence;

	protected Sequence _conditionWrapper;

	public new static LoopStatement ConstructPreConditionLoop(Evaluable condition, Sequence body, CodeModel.Statements.Statement conditionSource)
	{
		var conditionStatement = new LoopConditionStatement(condition, conditionSource);

		return new PreConditionLoopStatement(conditionStatement, body, conditionSource);
	}

	public new static LoopStatement ConstructPostConditionLoop(Sequence body, Evaluable condition, CodeModel.Statements.Statement conditionSource, CodeModel.Statements.Statement loopSource)
	{
		var conditionStatement = new LoopConditionStatement(condition, conditionSource);

		return new PostConditionLoopStatement(conditionStatement, body, loopSource);
	}

	protected ConditionalLoopStatement(Executable conditionStatement, Sequence body, CodeModel.Statements.Statement source)
		: base(source)
	{
		Body = body;

		_firstSequence = GetSequenceByIndex(0);

		_conditionWrapper = new Sequence();
		_conditionWrapper.Append(conditionStatement);
	}

	public override int GetSequenceCount() => 2;

	public override bool SelfSequenceDispatch => true;

	public override void Dispatch(ExecutionContext context, StackFrame stackFrame, int sequenceIndex, ref StatementPath? goTo)
	{
		int statementIndex = 0;

		if (goTo != null)
		{
			statementIndex = goTo.Pop();

			if (goTo.Count == 0)
				goTo = null;
		}

		if (sequenceIndex == 1)
			statementIndex += _firstSequence!.Count;

		DispatchImplementation(statementIndex, context, stackFrame);
	}

	protected abstract void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame);

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		DispatchImplementation(0, context, stackFrame);
	}

	class LoopConditionStatement(Evaluable condition, CodeModel.Statements.Statement? source)
		: Executable(source)
	{
		public bool ConditionValue;

		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			ConditionValue = condition.Evaluate(context, stackFrame).CoerceToInt() != 0;
		}
	}

	class PreConditionLoopStatement(LoopConditionStatement conditionStatement, Sequence body, CodeModel.Statements.Statement source)
		: ConditionalLoopStatement(conditionStatement, body, source)
	{
		public override bool CanBreak { get => false; set { } }

		public override Sequence? GetSequenceByIndex(int sequenceIndex)
		{
			if (sequenceIndex == 0)
				return _conditionWrapper;
			if (sequenceIndex == 1)
				return body;

			throw new IndexOutOfRangeException();
		}

		public override int IndexOfSequence(Sequence sequence)
		{
			if (sequence == _conditionWrapper)
				return 0;
			if (sequence == body)
				return 1;

			throw new Exception("Internal error: Sequence is not owned by this statement");
		}

		protected override void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				while (true)
				{
					System.Threading.Thread.Yield();

					if (statementIndex == 0)
					{
						context.Dispatch(conditionStatement, stackFrame);

						if (!conditionStatement.ConditionValue)
							break;
					}
					else
						statementIndex--;

					for (int i = statementIndex; i < body!.Count; i++)
						context.Dispatch(Body[i], stackFrame);
				}
			}
			catch (ExitDo) when (Type == LoopType.Do)
			{
			}
		}
	}

	class PostConditionLoopStatement(LoopConditionStatement conditionStatement, Sequence body, CodeModel.Statements.Statement source)
		: ConditionalLoopStatement(conditionStatement, body, source)
	{
		public override Sequence? GetSequenceByIndex(int sequenceIndex)
		{
			if (sequenceIndex == 0)
				return body;
			if (sequenceIndex == 1)
				return _conditionWrapper;

			throw new IndexOutOfRangeException();
		}

		public override int IndexOfSequence(Sequence sequence)
		{
			if (sequence == body)
				return 0;
			if (sequence == _conditionWrapper)
				return 1;

			throw new Exception("Internal error: Sequence is not owned by this statement");
		}

		protected override void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				while (true)
				{
					System.Threading.Thread.Yield();

					for (int i = statementIndex; i < body!.Count; i++)
						context.Dispatch(Body[i], stackFrame);

					context.Dispatch(conditionStatement, stackFrame);

					if (!conditionStatement.ConditionValue)
						break;
				}
			}
			catch (ExitDo) when (Type == LoopType.Do)
			{
			}
		}
	}
}
