using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class RandomizeStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? ArgumentExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		object seedValue;

		if (ArgumentExpression == null)
		{
			context.VisualLibrary.WriteText("Random-number seed (-32768 to 32767)? ");

			while (true)
			{
				string line = context.VisualLibrary.ReadLine(context.Machine.Keyboard);

				if (short.TryParse(line, out var shortSeedValue))
				{
					seedValue = shortSeedValue;
					break;
				}

				context.VisualLibrary.NewLine();
				context.VisualLibrary.WriteText("Redo from start");
				context.VisualLibrary.WriteText("? ");
			}
		}
		else
		{
			var argumentValue = ArgumentExpression.Evaluate(context, stackFrame);

			switch (argumentValue)
			{
				case IntegerVariable integerValue: seedValue = integerValue.Value; break;
				case LongVariable longValue: seedValue = longValue.Value; break;
				case SingleVariable singleValue: seedValue = singleValue.Value; break;
				case DoubleVariable doubleValue: seedValue = doubleValue.Value; break;
				case CurrencyVariable currencyValue: seedValue = (double)currencyValue.Value; break;

				default: throw RuntimeException.TypeMismatch(Source);
			}
		}

		Reseed(seedValue);
	}

	public static void Reseed(object seedValue)
	{
		int intPart = 0;
		double fracPart = 0;

		switch (seedValue)
		{
			case IntegerVariable integerVariable: Reseed(integerVariable.Value); return;
			case LongVariable longVariable: Reseed(longVariable.Value); return;
			case SingleVariable singleVariable: Reseed(singleVariable.Value); return;
			case DoubleVariable doubleVariable: Reseed(doubleVariable.Value); return;
			case CurrencyVariable currencyVariable: Reseed(currencyVariable.Value); return;

			case short shortValue: intPart = shortValue; break;
			case int intValue: intPart = intValue; break;
			case float floatValue:
			{
				var noFracPart = float.Truncate(floatValue);
				fracPart = floatValue - noFracPart;

				while ((noFracPart < int.MinValue) || (noFracPart > int.MaxValue))
					noFracPart *= 0.5f;

				intPart = (int)noFracPart;

				break;
			}
			case double doubleValue:
			{
				var noFracPart = double.Truncate(doubleValue);
				fracPart = doubleValue - noFracPart;

				while ((noFracPart < int.MinValue) || (noFracPart > int.MaxValue))
					noFracPart *= 0.5f;

				intPart = (int)noFracPart;

				break;
			}
			case decimal currencyValue:
			{
				var noFracPart = decimal.Truncate(currencyValue);
				fracPart = decimal.ToDouble(currencyValue - decimal.Truncate(currencyValue));

				intPart = decimal.GetBits(noFracPart)[0];

				break;
			}
		}

		RandomNumberGenerator.Reseed(intPart, fracPart);
	}
}
