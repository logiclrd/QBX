using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Functions;
using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class LoopStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public LoopType Type;

	public static LoopStatement ConstructUnconditionalLoop(Sequence body, CodeModel.Statements.Statement source, bool detectDelayLoops)
	{
		return new UnconditionalLoopStatement(body, source, detectDelayLoops);
	}

	public static LoopStatement ConstructPreConditionLoop(Evaluable condition, Sequence body, CodeModel.Statements.Statement conditionSource, bool detectDelayLoops)
	{
		return ConditionalLoopStatement.ConstructPreConditionLoop(condition, body, conditionSource, detectDelayLoops);
	}

	public static LoopStatement ConstructPostConditionLoop(Sequence body, Evaluable condition, CodeModel.Statements.Statement conditionSource, CodeModel.Statements.Statement loopSource, bool detectDelayLoops)
	{
		return ConditionalLoopStatement.ConstructPostConditionLoop(body, condition, conditionSource, loopSource, detectDelayLoops);
	}

	class UnconditionalLoopStatement(Sequence body, CodeModel.Statements.Statement source, bool detectDelayLoops)
		: LoopStatement(source)
	{
		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				bool isEmptyLoop = detectDelayLoops && (body.Count == 0);

				while (true)
				{
					if (isEmptyLoop)
						System.Threading.Thread.Sleep(10);
					else
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

	public new static LoopStatement ConstructPreConditionLoop(Evaluable condition, Sequence body, CodeModel.Statements.Statement conditionSource, bool detectDelayLoops)
	{
		if (detectDelayLoops)
		{
			if (HasNoSideEffectsAndVariesOnlyOnTimer(condition) && (body.Count == 0))
				body.Append(new TimingLoopDelayStatement());
		}

		var conditionStatement = new LoopConditionStatement(condition, conditionSource);

		return new PreConditionLoopStatement(conditionStatement, body, conditionSource);
	}

	public new static LoopStatement ConstructPostConditionLoop(Sequence body, Evaluable condition, CodeModel.Statements.Statement conditionSource, CodeModel.Statements.Statement loopSource, bool detectDelayLoops)
	{
		if (detectDelayLoops)
		{
			if (HasNoSideEffectsAndVariesOnlyOnTimer(condition) && (body.Count == 0))
				body.Append(new TimingLoopDelayStatement());
		}

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
			ConditionValue = condition.EvaluateAndCoerceToInt(context, stackFrame) != 0;
		}
	}

	static bool HasNoSideEffectsAndVariesOnlyOnTimer(Evaluable? expression)
	{
		if (expression == null)
			return true;

		switch (expression)
		{
			case UnaryExpression unaryExpression:
				return HasNoSideEffectsAndVariesOnlyOnTimer(unaryExpression.Right);

			case BinaryExpression binaryExpression:
				return
					HasNoSideEffectsAndVariesOnlyOnTimer(binaryExpression.Left) &&
					HasNoSideEffectsAndVariesOnlyOnTimer(binaryExpression.Right);

			case Conversion conversion:
				return HasNoSideEffectsAndVariesOnlyOnTimer(conversion.Value);

			case IdentifierExpression:
				return true;

			case ArrayElementExpression arrayElementExpression:
				foreach (var subscriptExpression in arrayElementExpression.SubscriptExpressions)
					if (!HasNoSideEffectsAndVariesOnlyOnTimer(subscriptExpression))
						return false;

				return true;

			case FieldAccessExpression fieldAccessExpression:
				return HasNoSideEffectsAndVariesOnlyOnTimer(fieldAccessExpression.Expression);

			case LiteralValue:
				return true;

			// Functions
			case AbsFunction abs: return HasNoSideEffectsAndVariesOnlyOnTimer(abs.ArgumentExpression);
			case AscFunction asc: return HasNoSideEffectsAndVariesOnlyOnTimer(asc.Argument);
			case AtnFunction atn: return HasNoSideEffectsAndVariesOnlyOnTimer(atn.Argument);
			case CCurFunction ccur: return HasNoSideEffectsAndVariesOnlyOnTimer(ccur.Argument);
			case CDblFunction cdbl: return HasNoSideEffectsAndVariesOnlyOnTimer(cdbl.Argument);
			case ChrFunction chr: return HasNoSideEffectsAndVariesOnlyOnTimer(chr.Argument);
			case CIntFunction cint: return HasNoSideEffectsAndVariesOnlyOnTimer(cint.Argument);
			case CLngFunction clng: return HasNoSideEffectsAndVariesOnlyOnTimer(clng.Argument);
			case CosFunction cos: return HasNoSideEffectsAndVariesOnlyOnTimer(cos.Argument);
			case CSngFunction csng: return HasNoSideEffectsAndVariesOnlyOnTimer(csng.Argument);
			case FixFunction fix: return HasNoSideEffectsAndVariesOnlyOnTimer(fix.ArgumentExpression);
			case InStrFunction instr:
				return
					HasNoSideEffectsAndVariesOnlyOnTimer(instr.StringExpression) &&
					HasNoSideEffectsAndVariesOnlyOnTimer(instr.SearchForExpression) &&
					HasNoSideEffectsAndVariesOnlyOnTimer(instr.StartExpression);
			case IntFunction @int: return HasNoSideEffectsAndVariesOnlyOnTimer(@int.ArgumentExpression);
			case LCaseFunction lcase: return HasNoSideEffectsAndVariesOnlyOnTimer(lcase.ArgumentExpression);
			case LeftFunction left:
				return
					HasNoSideEffectsAndVariesOnlyOnTimer(left.StringExpression) &&
					HasNoSideEffectsAndVariesOnlyOnTimer(left.LengthExpression);
			case MidFunction mid:
				return
					HasNoSideEffectsAndVariesOnlyOnTimer(mid.StringExpression) &&
					HasNoSideEffectsAndVariesOnlyOnTimer(mid.StartExpression) &&
					HasNoSideEffectsAndVariesOnlyOnTimer(mid.LengthExpression);
			case RightFunction right:
				return
					HasNoSideEffectsAndVariesOnlyOnTimer(right.StringExpression) &&
					HasNoSideEffectsAndVariesOnlyOnTimer(right.LengthExpression);
			case SinFunction sin: return HasNoSideEffectsAndVariesOnlyOnTimer(sin.Argument);
			case SpaceFunction space: return HasNoSideEffectsAndVariesOnlyOnTimer(space.Argument);
			case StrFunction str: return HasNoSideEffectsAndVariesOnlyOnTimer(str.Argument);
			case TanFunction tan: return HasNoSideEffectsAndVariesOnlyOnTimer(tan.Argument);
			case UCaseFunction ucase: return HasNoSideEffectsAndVariesOnlyOnTimer(ucase.ArgumentExpression);
			case ValFunction val: return HasNoSideEffectsAndVariesOnlyOnTimer(val.Argument);

			case TimerFunction:
			case LBoundFunction:
			case UBoundFunction:
				return true;
		}

		return false;
	}

	class TimingLoopDelayStatement() : Executable(source: null)
	{
		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			var delay = context.Machine.Timer.TimeToNextTick;

			delay = delay - TimeSpan.FromMilliseconds(20);

			if (delay > TimeSpan.Zero)
				System.Threading.Thread.Sleep(delay);
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
