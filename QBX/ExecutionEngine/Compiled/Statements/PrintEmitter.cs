using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware.Fonts;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class PrintEmitter
{
	public abstract int CursorX { get; set; }
	public abstract int Width { get; }

	static readonly CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	[ThreadStatic]
	static byte[]? s_buffer;

	public virtual void Emit(ReadOnlySpan<char> chars)
	{
		int numBytes = s_cp437.GetByteCount(chars);

		if ((s_buffer == null) || (s_buffer.Length < numBytes))
			s_buffer = new byte[Math.Max(128, numBytes * 2)];

		numBytes = s_cp437.GetBytes(chars, s_buffer);

		Emit(s_buffer.AsSpan().Slice(0, numBytes));
	}

	public virtual void Emit(byte ch)
	{
		if (s_buffer == null)
			s_buffer = new byte[128];

		s_buffer[0] = ch;

		Emit(s_buffer.AsSpan().Slice(0, 1));
	}

	public virtual void Emit(char ch)
	{
		if (s_buffer == null)
			s_buffer = new byte[128];

		s_buffer[0] = CP437Encoding.GetByteSemantic(ch);

		Emit(s_buffer.AsSpan().Slice(0, 1));
	}

	public abstract void Emit(Span<byte> str);

	public virtual void EmitNewLine()
	{
		if (s_buffer == null)
			s_buffer = new byte[128];

		s_buffer[0] = 13;
		s_buffer[1] = 10;

		Emit(s_buffer.AsSpan().Slice(0, 2));
	}

	public virtual void Flush() { }

	public const string Zone = "              "; // 14 characters

	public void NextZone()
	{
		int currentZoneStart = Zone.Length * (CursorX / Zone.Length);
		int nextZoneStart = currentZoneStart + Zone.Length;

		if (nextZoneStart >= Width)
			EmitNewLine();
		else
		{
			int offset = CursorX - currentZoneStart;

			Emit(Zone.AsSpan().Slice(offset));
		}
	}

	public void Emit(Variable value)
	{
		value.ReadPinnedData();

		switch (value)
		{
			case IntegerVariable integerValue: Emit(integerValue.Value); break;
			case LongVariable longValue: Emit(longValue.Value); break;
			case SingleVariable singleValue: Emit(singleValue.Value); break;
			case DoubleVariable doubleValue: Emit(doubleValue.Value); break;
			case CurrencyVariable currencyValue: Emit(currencyValue.Value); break;
			case StringVariable stringValue: Emit(stringValue.ValueSpan); break;
		}
	}

	public void Emit(string str) => Emit(str.AsSpan());
	public void Emit(StringValue str) => Emit(str.AsSpan());

	public void Emit(short integerValue)
	{
		if (integerValue >= 0)
			Emit((byte)' ');

		Emit(integerValue.ToString());

		Emit((byte)' ');
	}

	public void Emit(int longValue)
	{
		if (longValue >= 0)
			Emit((byte)' ');

		Emit(longValue.ToString());

		Emit((byte)' ');
	}

	public void Emit(float singleValue)
	{
		if (!(singleValue < 0))
			Emit((byte)' ');

		Emit(NumberFormatter.Format(singleValue, qualify: false));
		Emit((byte)' ');
	}

	public void Emit(double doubleValue)
	{
		if (!(doubleValue < 0))
			Emit((byte)' ');

		Emit(NumberFormatter.Format(doubleValue, qualify: false));
		Emit((byte)' ');
	}

	public void Emit(decimal currencyValue)
	{
		if (!currencyValue.IsInCurrencyRange())
			throw new Exception("Internal error: Currency value is out of range");

		string formatted = currencyValue.ToString("###############.####");

		if (formatted == "")
		{
			Emit((byte)' ');
			Emit((byte)'0');
			Emit((byte)' ');
		}
		else
		{
			Emit((byte)' ');
			Emit(formatted);
			Emit((byte)' ');
		}
	}
}
