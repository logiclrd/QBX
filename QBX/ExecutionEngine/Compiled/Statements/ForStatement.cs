using System;

using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

// FOR loop quirk: If either of the range expressions evaluate to
// floating-point values outside of the range of an integer iterator
// variable, the loop simply does nothing. (Contrast with having
// a range expression fail to evaluate because the numeric value
// is out-of-range, which is an error.)

public abstract class ForStatement : Executable
{
	public static Executable Construct(
		int iteratorVariableIndex,
		PrimitiveDataType iteratorVariableType,
		Evaluable fromExpression,
		Evaluable toExpression,
		Evaluable? stepExpression,
		Sequence body,
		CodeModel.Statements.ForStatement? sourceForStatement,
		CodeModel.Statements.NextStatement? sourceNextStatement)
	{
		switch (iteratorVariableType)
		{
			case PrimitiveDataType.Integer:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Integer);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Integer);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Integer);

				return
					new IntegerForStatement(sourceForStatement, sourceNextStatement)
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
						SourceNextStatement = sourceNextStatement,
					};
			}
			case PrimitiveDataType.Long:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Long);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Long);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Long);

				return
					new LongForStatement(sourceForStatement, sourceNextStatement)
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
						SourceNextStatement = sourceNextStatement,
					};
			}
			case PrimitiveDataType.Single:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Single);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Single);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Single);

				return
					new SingleForStatement(sourceForStatement, sourceNextStatement)
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
						SourceNextStatement = sourceNextStatement,
					};
			}
			case PrimitiveDataType.Double:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Double);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Double);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Double);

				return
					new DoubleForStatement(sourceForStatement, sourceNextStatement)
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
						SourceNextStatement = sourceNextStatement,
					};
			}
			case PrimitiveDataType.Currency:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Currency);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Currency);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Currency);

				return
					new CurrencyForStatement(sourceForStatement, sourceNextStatement)
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
						SourceNextStatement = sourceNextStatement,
					};
			}

			default: throw new Exception("Unrecognized iterator variable type " + iteratorVariableType);
		}
	}

	public override int IndexOfSequence(Sequence sequence)
	{
		if (sequence == this.Body)
			return 0;
		if (sequence == _nextWrapper)
			return 1;

		throw new Exception("Internal error: Sequence is not owned by this statement");
	}

	public Sequence? Body;
	Sequence? _nextWrapper;

	protected long _nextDispatcherIndex;

	public ForStatement(CodeModel.Statements.Statement? sourceForStatement, CodeModel.Statements.Statement? sourceNextStatement)
		: base(sourceForStatement)
	{
		var nextDispatcher = new NextDispatcherStatement(sourceNextStatement);

		_nextWrapper = new Sequence();
		_nextWrapper.OwnerExecutable = this;
		_nextWrapper.Append(nextDispatcher);

		_nextDispatcherIndex = nextDispatcher.DispatcherIndex;
	}

	public override int GetSequenceCount() => 2;

	public override Sequence? GetSequenceByIndex(int sequenceIndex)
	{
		if (sequenceIndex == 0)
			return Body;
		if (sequenceIndex == 1)
			return _nextWrapper;

		throw new IndexOutOfRangeException();
	}

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
			statementIndex += Body!.Count;

		DispatchImplementation(statementIndex, context, stackFrame);
	}

	protected abstract void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame);

	class NextDispatcherStatement(CodeModel.Statements.Statement? source) : Executable(source)
	{
		static long s_nextNextIndex;

		long _nextIndex = s_nextNextIndex++;

		public long DispatcherIndex => _nextIndex;

		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			stackFrame.NextStatements[_nextIndex].Execute(context, stackFrame);
		}
	}
}

public class IntegerForStatement(CodeModel.Statements.ForStatement? sourceForStatement, CodeModel.Statements.NextStatement? sourceNextStatement) : ForStatement(sourceForStatement, sourceNextStatement)
{
	public int IteratorVariableIndex;
	public Evaluable? FromExpression;
	public Evaluable? ToExpression;
	public Evaluable? StepExpression;
	public CodeModel.Statements.NextStatement? SourceNextStatement;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("IntegerForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("IntegerForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context, stackFrame) ?? throw new Exception("IntegerForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context, stackFrame) ?? throw new Exception("IntegerForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context, stackFrame);

		short from = ((IntegerVariable)fromVariable).Value;
		short to = ((IntegerVariable)toVariable).Value;
		short step = (stepVariable as IntegerVariable)?.Value ?? 1;

		bool proceed = (from == to);

		if (!proceed)
		{
			if (from < to)
				proceed = (step > 0);
			else
				proceed = (step < 0);
		}

		if (proceed)
		{
			var nextStatement = new NextStatement(from, to, step, SourceNextStatement);

			stackFrame.NextStatements[_nextDispatcherIndex] = nextStatement;

			DispatchImplementation(0, context, stackFrame);
		}
	}

	protected override void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame)
	{
		var nextStatement = (NextStatement)stackFrame.NextStatements[_nextDispatcherIndex];

		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		while (!nextStatement.FinishLoop)
		{
			iteratorVariable.SetData(nextStatement.NextValue);

			for (int i = statementIndex; i < Body!.Count; i++)
				context.Dispatch(Body[i], stackFrame);

			statementIndex = 0;

			context.Dispatch(nextStatement, stackFrame);
		}
	}

	class NextStatement(short from, short to, short step, CodeModel.Statements.NextStatement? sourceNextStatement)
		: Executable(sourceNextStatement)
	{
		public bool FinishLoop = false;
		public short NextValue = from;

		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				NextValue += step;
			}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(Source);
			}

			if (step > 0)
				FinishLoop = (NextValue > to);
			else if (step < 0)
				FinishLoop = (NextValue < to);
		}
	}
}

public class LongForStatement(CodeModel.Statements.ForStatement? sourceForStatement, CodeModel.Statements.NextStatement? sourceNextStatement) : ForStatement(sourceForStatement, sourceNextStatement)
{
	public int IteratorVariableIndex;
	public Evaluable? FromExpression;
	public Evaluable? ToExpression;
	public Evaluable? StepExpression;
	public CodeModel.Statements.NextStatement? SourceNextStatement;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("LongForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("LongForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context, stackFrame) ?? throw new Exception("LongForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context, stackFrame) ?? throw new Exception("LongForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context, stackFrame);

		int from = ((LongVariable)fromVariable).Value;
		int to = ((LongVariable)toVariable).Value;
		int step = (stepVariable as LongVariable)?.Value ?? 1;

		bool proceed = (from == to);

		if (!proceed)
		{
			if (from < to)
				proceed = (step > 0);
			else
				proceed = (step < 0);
		}

		if (proceed)
		{
			var nextStatement = new NextStatement(from, to, step, SourceNextStatement);

			stackFrame.NextStatements[_nextDispatcherIndex] = nextStatement;

			DispatchImplementation(0, context, stackFrame);
		}
	}

	protected override void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame)
	{
		var nextStatement = (NextStatement)stackFrame.NextStatements[_nextDispatcherIndex];

		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		while (!nextStatement.FinishLoop)
		{
			iteratorVariable.SetData(nextStatement.NextValue);

			for (int i = statementIndex; i < Body!.Count; i++)
				context.Dispatch(Body[i], stackFrame);

			statementIndex = 0;

			context.Dispatch(nextStatement, stackFrame);
		}
	}

	class NextStatement(int from, int to, int step, CodeModel.Statements.NextStatement? sourceNextStatement)
		: Executable(sourceNextStatement)
	{
		public bool FinishLoop = false;
		public int NextValue = from;

		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				NextValue += step;
			}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(Source);
			}

			if (step > 0)
				FinishLoop = (NextValue > to);
			else if (step < 0)
				FinishLoop = (NextValue < to);
		}
	}
}

public class SingleForStatement(CodeModel.Statements.ForStatement? sourceForStatement, CodeModel.Statements.NextStatement? sourceNextStatement) : ForStatement(sourceForStatement, sourceNextStatement)
{
	public int IteratorVariableIndex;
	public Evaluable? FromExpression;
	public Evaluable? ToExpression;
	public Evaluable? StepExpression;
	public CodeModel.Statements.NextStatement? SourceNextStatement;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("SingleForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("SingleForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context, stackFrame) ?? throw new Exception("SingleForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context, stackFrame) ?? throw new Exception("SingleForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context, stackFrame);

		float from = ((SingleVariable)fromVariable).Value;
		float to = ((SingleVariable)toVariable).Value;
		float step = (stepVariable as SingleVariable)?.Value ?? 1;

		bool proceed = (from == to);

		if (!proceed)
		{
			if (from < to)
				proceed = (step > 0);
			else
				proceed = (step < 0);
		}

		if (proceed)
		{
			var nextStatement = new NextStatement(from, to, step, SourceNextStatement);

			stackFrame.NextStatements[_nextDispatcherIndex] = nextStatement;

			DispatchImplementation(0, context, stackFrame);
		}
	}

	protected override void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame)
	{
		var nextStatement = (NextStatement)stackFrame.NextStatements[_nextDispatcherIndex];

		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		while (!nextStatement.FinishLoop)
		{
			iteratorVariable.SetData(nextStatement.NextValue);

			for (int i = statementIndex; i < Body!.Count; i++)
				context.Dispatch(Body[i], stackFrame);

			statementIndex = 0;

			context.Dispatch(nextStatement, stackFrame);
		}
	}

	class NextStatement(float from, float to, float step, CodeModel.Statements.NextStatement? sourceNextStatement)
		: Executable(sourceNextStatement)
	{
		public bool FinishLoop = false;
		public float NextValue = from;

		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				NextValue += step;
			}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(Source);
			}

			if (step > 0)
				FinishLoop = (NextValue > to);
			else if (step < 0)
				FinishLoop = (NextValue < to);
		}
	}
}

public class DoubleForStatement(CodeModel.Statements.ForStatement? sourceForStatement, CodeModel.Statements.NextStatement? sourceNextStatement) : ForStatement(sourceForStatement, sourceNextStatement)
{
	public int IteratorVariableIndex;
	public Evaluable? FromExpression;
	public Evaluable? ToExpression;
	public Evaluable? StepExpression;
	public CodeModel.Statements.NextStatement? SourceNextStatement;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("DoubleForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("DoubleForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context, stackFrame) ?? throw new Exception("DoubleForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context, stackFrame) ?? throw new Exception("DoubleForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context, stackFrame);

		double from = ((DoubleVariable)fromVariable).Value;
		double to = ((DoubleVariable)toVariable).Value;
		double step = (stepVariable as DoubleVariable)?.Value ?? 1;

		bool proceed = (from == to);

		if (!proceed)
		{
			if (from < to)
				proceed = (step > 0);
			else
				proceed = (step < 0);
		}

		if (proceed)
		{
			var nextStatement = new NextStatement(from, to, step, SourceNextStatement);

			stackFrame.NextStatements[_nextDispatcherIndex] = nextStatement;

			DispatchImplementation(0, context, stackFrame);
		}
	}

	protected override void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame)
	{
		var nextStatement = (NextStatement)stackFrame.NextStatements[_nextDispatcherIndex];

		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		while (!nextStatement.FinishLoop)
		{
			iteratorVariable.SetData(nextStatement.NextValue);

			for (int i = statementIndex; i < Body!.Count; i++)
				context.Dispatch(Body[i], stackFrame);

			statementIndex = 0;

			context.Dispatch(nextStatement, stackFrame);
		}
	}

	class NextStatement(double from, double to, double step, CodeModel.Statements.NextStatement? sourceNextStatement)
		: Executable(sourceNextStatement)
	{
		public bool FinishLoop = false;
		public double NextValue = from;

		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				NextValue += step;
			}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(Source);
			}

			if (step > 0)
				FinishLoop = (NextValue > to);
			else if (step < 0)
				FinishLoop = (NextValue < to);
		}
	}
}

public class CurrencyForStatement(CodeModel.Statements.ForStatement? sourceForStatement, CodeModel.Statements.NextStatement? sourceNextStatement) : ForStatement(sourceForStatement, sourceNextStatement)
{
	public int IteratorVariableIndex;
	public Evaluable? FromExpression;
	public Evaluable? ToExpression;
	public Evaluable? StepExpression;
	public CodeModel.Statements.NextStatement? SourceNextStatement;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("CurrencyForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("CurrencyForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context, stackFrame) ?? throw new Exception("CurrencyForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context, stackFrame) ?? throw new Exception("CurrencyForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context, stackFrame);

		decimal from = ((CurrencyVariable)fromVariable).Value;
		decimal to = ((CurrencyVariable)toVariable).Value;
		decimal step = (stepVariable as CurrencyVariable)?.Value ?? 1;

		bool proceed = (from == to);

		if (!proceed)
		{
			if (from < to)
				proceed = (step > 0);
			else
				proceed = (step < 0);
		}

		if (proceed)
		{
			var nextStatement = new NextStatement(from, to, step, SourceNextStatement);

			stackFrame.NextStatements[_nextDispatcherIndex] = nextStatement;

			DispatchImplementation(0, context, stackFrame);
		}
	}

	protected override void DispatchImplementation(int statementIndex, ExecutionContext context, StackFrame stackFrame)
	{
		var nextStatement = (NextStatement)stackFrame.NextStatements[_nextDispatcherIndex];

		var iteratorVariable = stackFrame.Variables[IteratorVariableIndex];

		while (!nextStatement.FinishLoop)
		{
			iteratorVariable.SetData(nextStatement.NextValue);

			for (int i = statementIndex; i < Body!.Count; i++)
				context.Dispatch(Body[i], stackFrame);

			statementIndex = 0;

			context.Dispatch(nextStatement, stackFrame);
		}
	}

	class NextStatement(decimal from, decimal to, decimal step, CodeModel.Statements.NextStatement? sourceNextStatement)
		: Executable(sourceNextStatement)
	{
		public bool FinishLoop = false;
		public decimal NextValue = from;

		public override void Execute(ExecutionContext context, StackFrame stackFrame)
		{
			try
			{
				NextValue += step;
			}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(Source);
			}

			if (!NextValue.IsInCurrencyRange())
				throw RuntimeException.Overflow(Source);

			if (step > 0)
				FinishLoop = (NextValue > to);
			else if (step < 0)
				FinishLoop = (NextValue < to);
		}
	}
}
