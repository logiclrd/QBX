using System.Globalization;
using System.IO;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.OperatingSystem.Memory;

namespace QBX.OperatingSystem.Globalization;

public class CountryInfo
{
	public DateFormat DateFormat;
	public StringValue CurrencySymbol = StringValue.CreateFixedLength(5);
	public StringValue ThousandsSeparator = StringValue.CreateFixedLength(2);
	public StringValue DecimalSeparator = StringValue.CreateFixedLength(2);
	public StringValue DateSeparator = StringValue.CreateFixedLength(2);
	public StringValue TimeSeparator = StringValue.CreateFixedLength(2);
	public CurrencyFormat CurrencyFormat;
	public int CurrencyPlaces;
	public TimeFormat TimeFormat;
	public SegmentedAddress CaseMappingRoutineAddress;
	public StringValue DataSeparator = StringValue.CreateFixedLength(2);
	public byte[] Reserved = new byte[10];

	public void Import(CultureInfo cultureInfo)
	{
		switch (cultureInfo.DateTimeFormat.ShortDatePattern[0])
		{
			case 'm': case 'M': DateFormat = DateFormat.USA; break;
			case 'd': case 'D': DateFormat = DateFormat.Europe; break;
			case 'y': case 'Y': DateFormat = DateFormat.Japan; break;
		}

		CurrencySymbol.Set(cultureInfo.NumberFormat.CurrencySymbol);
		ThousandsSeparator.Set(cultureInfo.NumberFormat.NumberGroupSeparator);
		DecimalSeparator.Set(cultureInfo.NumberFormat.NumberDecimalSeparator);
		DateSeparator.Set(cultureInfo.DateTimeFormat.DateSeparator);
		TimeSeparator.Set(cultureInfo.DateTimeFormat.TimeSeparator);

		// The values happen to match up, what a coincidence :-)
		CurrencyFormat = (CurrencyFormat)cultureInfo.NumberFormat.CurrencyPositivePattern;

		CurrencyPlaces = cultureInfo.NumberFormat.CurrencyDecimalDigits;
		TimeFormat = cultureInfo.DateTimeFormat.ShortDatePattern.Contains("H") ? TimeFormat._24Hour : TimeFormat._12Hour;
		DataSeparator.Set(cultureInfo.TextInfo.ListSeparator);
	}

	public void Serialize(SystemMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, 36);

		var writer = new BinaryWriter(stream);

		writer.Write((ushort)DateFormat);
		writer.Write(CurrencySymbol.AsSpan());
		writer.Write(ThousandsSeparator.AsSpan());
		writer.Write(DecimalSeparator.AsSpan());
		writer.Write(DateSeparator.AsSpan());
		writer.Write(TimeSeparator.AsSpan());
		writer.Write((byte)CurrencyFormat);
		writer.Write((byte)CurrencyPlaces);
		writer.Write((byte)TimeFormat);
		writer.Write(CaseMappingRoutineAddress.Offset);
		writer.Write(CaseMappingRoutineAddress.Segment);
		writer.Write(DataSeparator.AsSpan());
		writer.Write(Reserved);
	}

	public void Deserialize(SystemMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, 36);

		var reader = new BinaryReader(stream);

		DateFormat = (DateFormat)reader.ReadUInt16();
		reader.ReadExactly(CurrencySymbol.AsSpan());
		reader.ReadExactly(ThousandsSeparator.AsSpan());
		reader.ReadExactly(DecimalSeparator.AsSpan());
		reader.ReadExactly(DateSeparator.AsSpan());
		reader.ReadExactly(TimeSeparator.AsSpan());
		CurrencyFormat = (CurrencyFormat)reader.ReadByte();
		CurrencyPlaces = reader.ReadByte();
		TimeFormat = (TimeFormat)reader.ReadByte();
		CaseMappingRoutineAddress.Offset = reader.ReadUInt16();
		CaseMappingRoutineAddress.Segment = reader.ReadUInt16();
		reader.ReadExactly(DataSeparator.AsSpan());
		reader.ReadExactly(Reserved);
	}
}
