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

public class ForStatement
{
	public static IExecutable Construct(
		int iteratorVariableIndex,
		PrimitiveDataType iteratorVariableType,
		IEvaluable fromExpression,
		IEvaluable toExpression,
		IEvaluable? stepExpression,
		ISequence body)
	{
		switch (iteratorVariableType)
		{
			case PrimitiveDataType.Integer:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Integer);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Integer);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Integer);

				return
					new IntegerForStatement()
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
					};
			}
			case PrimitiveDataType.Long:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Long);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Long);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Long);

				return
					new LongForStatement()
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
					};
			}
			case PrimitiveDataType.Single:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Single);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Single);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Single);

				return
					new SingleForStatement()
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
					};
			}
			case PrimitiveDataType.Double:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Double);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Double);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Double);

				return
					new DoubleForStatement()
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
					};
			}
			case PrimitiveDataType.Currency:
			{
				fromExpression = Conversion.Construct(fromExpression, PrimitiveDataType.Currency);
				toExpression = Conversion.Construct(toExpression, PrimitiveDataType.Currency);
				stepExpression = Conversion.Construct(stepExpression, PrimitiveDataType.Currency);

				return
					new CurrencyForStatement()
					{
						IteratorVariableIndex = iteratorVariableIndex,
						FromExpression = fromExpression,
						ToExpression = toExpression,
						StepExpression = stepExpression,
						Body = body,
					};
			}

			default: throw new Exception("Unrecognized iterator variable type " + iteratorVariableType);
		}
	}
}

public class IntegerForStatement : IExecutable
{
	public int IteratorVariableIndex;
	public IEvaluable? FromExpression;
	public IEvaluable? ToExpression;
	public IEvaluable? StepExpression;
	public ISequence? Body;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		var iteratorVariable = context.CurrentFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("IntegerForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("IntegerForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context) ?? throw new Exception("IntegerForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context) ?? throw new Exception("IntegerForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context);

		short fromValue = ((IntegerVariable)fromVariable).Value;
		short toValue = ((IntegerVariable)toVariable).Value;
		short stepValue = (stepVariable as IntegerVariable)?.Value ?? 1;

		bool proceed = (fromValue == toValue);

		if (!proceed)
		{
			if (fromValue < toValue)
				proceed = (stepValue > 0);
			else
				proceed = (stepValue < 0);
		}

		if (proceed)
			context.Execute(new ForLoop(FromExpression?.SourceStatement, fromValue, toValue, stepValue, iteratorVariable, Body), stepInto);
	}

	class ForLoop(CodeModel.Statements.Statement? blame, short from, short to, short step, Variable iterator, ISequence body) : IExecutable
	{
		short _nextValue = from;

		public void Execute(ExecutionContext context, bool stepInto)
		{
			iterator.SetData(_nextValue);

			try
			{
				_nextValue += step;
			}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(blame);
			}

			bool finishLoop = false;

			if (step > 0)
				finishLoop = (_nextValue > to);
			else if (step < 0)
				finishLoop = (_nextValue < to);

			if (!finishLoop)
				context.CurrentFrame.NextStatement = this;

			context.PushScope();

			context.CurrentFrame.CurrentSequence = body;
		}
	}
}

public class LongForStatement : IExecutable
{
	public int IteratorVariableIndex;
	public IEvaluable? FromExpression;
	public IEvaluable? ToExpression;
	public IEvaluable? StepExpression;
	public ISequence? Body;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		var iteratorVariable = context.CurrentFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("LongForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("LongForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context) ?? throw new Exception("LongForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context) ?? throw new Exception("LongForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context);

		int fromValue = ((LongVariable)fromVariable).Value;
		int toValue = ((LongVariable)toVariable).Value;
		int stepValue = (stepVariable as LongVariable)?.Value ?? 1;

		bool proceed = (fromValue == toValue);

		if (!proceed)
		{
			if (fromValue < toValue)
				proceed = (stepValue > 0);
			else
				proceed = (stepValue < 0);
		}

		if (proceed)
			context.Execute(new ForLoop(FromExpression?.SourceStatement, fromValue, toValue, stepValue, iteratorVariable, Body), stepInto);
	}

	class ForLoop(CodeModel.Statements.Statement? blame, int from, int to, int step, Variable iterator, ISequence body) : IExecutable
	{
		int _nextValue = from;

		public void Execute(ExecutionContext context, bool stepInto)
		{
			iterator.SetData(_nextValue);

			try
			{
				_nextValue += step;
			}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(blame);
			}

			bool finishLoop = false;

			if (step > 0)
				finishLoop = (_nextValue > to);
			else if (step < 0)
				finishLoop = (_nextValue < to);

			if (!finishLoop)
				context.CurrentFrame.NextStatement = this;

			context.PushScope();

			context.CurrentFrame.CurrentSequence = body;
		}
	}
}

public class SingleForStatement : IExecutable
{
	public int IteratorVariableIndex;
	public IEvaluable? FromExpression;
	public IEvaluable? ToExpression;
	public IEvaluable? StepExpression;
	public ISequence? Body;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		var iteratorVariable = context.CurrentFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("SingleForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("SingleForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context) ?? throw new Exception("SingleForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context) ?? throw new Exception("SingleForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context);

		float fromValue = ((SingleVariable)fromVariable).Value;
		float toValue = ((SingleVariable)toVariable).Value;
		float stepValue = (stepVariable as SingleVariable)?.Value ?? 1;

		bool proceed = (fromValue == toValue);

		if (!proceed)
		{
			if (fromValue < toValue)
				proceed = (stepValue > 0);
			else
				proceed = (stepValue < 0);
		}

		if (proceed)
			context.Execute(new ForLoop(FromExpression?.SourceStatement, fromValue, toValue, stepValue, iteratorVariable, Body), stepInto);
	}

	class ForLoop(CodeModel.Statements.Statement? blame, float from, float to, float step, Variable iterator, ISequence body) : IExecutable
	{
		float _nextValue = from;

		public void Execute(ExecutionContext context, bool stepInto)
		{
			iterator.SetData(_nextValue);

			try
						{
			_nextValue += step;
						}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(blame);
						}

			bool finishLoop = false;

			if (step > 0)
				finishLoop = (_nextValue > to);
			else if (step < 0)
				finishLoop = (_nextValue < to);

			if (!finishLoop)
				context.CurrentFrame.NextStatement = this;

			context.PushScope();

			context.CurrentFrame.CurrentSequence = body;
		}
	}
}

public class DoubleForStatement : IExecutable
{
	public int IteratorVariableIndex;
	public IEvaluable? FromExpression;
	public IEvaluable? ToExpression;
	public IEvaluable? StepExpression;
	public ISequence? Body;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		var iteratorVariable = context.CurrentFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("DoubleForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("DoubleForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context) ?? throw new Exception("DoubleForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context) ?? throw new Exception("DoubleForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context);

		double fromValue = ((DoubleVariable)fromVariable).Value;
		double toValue = ((DoubleVariable)toVariable).Value;
		double stepValue = (stepVariable as DoubleVariable)?.Value ?? 1;

		bool proceed = (fromValue == toValue);

		if (!proceed)
		{
			if (fromValue < toValue)
				proceed = (stepValue > 0);
			else
				proceed = (stepValue < 0);
		}

		if (proceed)
			context.Execute(new ForLoop(FromExpression?.SourceStatement, fromValue, toValue, stepValue, iteratorVariable, Body), stepInto);
	}

	class ForLoop(CodeModel.Statements.Statement? blame, double from, double to, double step, Variable iterator, ISequence body) : IExecutable
	{
		double _nextValue = from;

		public void Execute(ExecutionContext context, bool stepInto)
		{
			iterator.SetData(_nextValue);

			try
							{
			_nextValue += step;
							}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(blame);
							}

			bool finishLoop = false;

			if (step > 0)
				finishLoop = (_nextValue > to);
			else if (step < 0)
				finishLoop = (_nextValue < to);

			if (!finishLoop)
				context.CurrentFrame.NextStatement = this;

			context.PushScope();

			context.CurrentFrame.CurrentSequence = body;
		}
	}
}

public class CurrencyForStatement : IExecutable
{
	public int IteratorVariableIndex;
	public IEvaluable? FromExpression;
	public IEvaluable? ToExpression;
	public IEvaluable? StepExpression;
	public ISequence? Body;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		var iteratorVariable = context.CurrentFrame.Variables[IteratorVariableIndex];

		if (iteratorVariable == null)
			throw new Exception("CurrencyForStatement with no IteratorVariable");
		if (Body == null)
			throw new Exception("CurrencyForStatement with no Body");

		var fromVariable = FromExpression?.Evaluate(context) ?? throw new Exception("CurrencyForStatement with no FromExpression");
		var toVariable = ToExpression?.Evaluate(context) ?? throw new Exception("CurrencyForStatement with no ToExpression");
		var stepVariable = StepExpression?.Evaluate(context);

		decimal fromValue = ((CurrencyVariable)fromVariable).Value;
		decimal toValue = ((CurrencyVariable)toVariable).Value;
		decimal stepValue = (stepVariable as CurrencyVariable)?.Value ?? 1;

		bool proceed = (fromValue == toValue);

		if (!proceed)
		{
			if (fromValue < toValue)
				proceed = (stepValue > 0);
			else
				proceed = (stepValue < 0);
		}

		if (proceed)
			context.Execute(new ForLoop(FromExpression?.SourceStatement, fromValue, toValue, stepValue, iteratorVariable, Body), stepInto);
	}

	class ForLoop(CodeModel.Statements.Statement? blame, decimal from, decimal to, decimal step, Variable iterator, ISequence body) : IExecutable
	{
		decimal _nextValue = from;

		public void Execute(ExecutionContext context, bool stepInto)
		{
			iterator.SetData(_nextValue);

			try
			{
				_nextValue += step;
			}
			catch (OverflowException)
			{
				throw RuntimeException.Overflow(blame);
			}

			if (!_nextValue.IsInCurrencyRange())
				throw RuntimeException.Overflow(blame);

			bool finishLoop = false;

			if (step > 0)
				finishLoop = (_nextValue > to);
			else if (step < 0)
				finishLoop = (_nextValue < to);

			if (!finishLoop)
				context.CurrentFrame.NextStatement = this;

			context.PushScope();

			context.CurrentFrame.CurrentSequence = body;
		}
	}
}
