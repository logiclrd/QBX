using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PrintEmitter(ExecutionContext context)
{
	VisualLibrary _visual = context.VisualLibrary;

	public void NextLine()
	{
		_visual.NewLine();
	}

	public const string Zone = "              "; // 14 characters

	public void NextZone()
	{
		int currentZoneStart = Zone.Length * (_visual.CursorX / Zone.Length);
		int nextZoneStart = currentZoneStart + Zone.Length;

		if (nextZoneStart >= _visual.Width)
			_visual.NewLine();
		else
		{
			int offset = _visual.CursorX - currentZoneStart;

			_visual.WriteText(Zone.AsSpan().Slice(offset));
		}
	}

	public void Emit(Variable value)
	{
		switch (value)
		{
			case IntegerVariable integerValue: Emit(integerValue.Value); break;
			case LongVariable longValue: Emit(longValue.Value); break;
			case SingleVariable singleValue: Emit(singleValue.Value); break;
			case DoubleVariable doubleValue: Emit(doubleValue.Value); break;
			case CurrencyVariable currencyValue: Emit(currencyValue.Value); break;
			case StringVariable stringValue: Emit(stringValue.Value); break;
		}
	}

	public void Emit(char ch) => _visual.WriteText(ch);
	public void Emit(string str) => _visual.WriteText(str);

	public void Emit(short integerValue)
	{
		if (integerValue < 0)
			Emit('-');
		else
			Emit(' ');

		Emit(integerValue.ToString());

		Emit(' ');
	}

	public void Emit(int longValue)
	{
		if (longValue < 0)
			Emit('-');
		else
			Emit(' ');

		Emit(longValue.ToString());

		Emit(' ');
	}

	public void Emit(float singleValue)
	{
		if (singleValue >= 0)
			Emit(' ');

		Emit(NumberFormatter.Format(singleValue, qualify: false));
		Emit(' ');
	}

	public void Emit(double doubleValue)
	{
		if (doubleValue >= 0)
			Emit(' ');

		Emit(NumberFormatter.Format(doubleValue, qualify: false));
		Emit(' ');
	}

	public void Emit(decimal currencyValue)
	{
		if (!currencyValue.IsInCurrencyRange())
			throw new Exception("Internal error: Currency value is out of range");

		string formatted = currencyValue.ToString("###############.####");

		if (formatted == "")
			Emit(" 0 ");
		else
		{
			Emit(' ');
			Emit(formatted);
			Emit(' ');
		}
	}
}
