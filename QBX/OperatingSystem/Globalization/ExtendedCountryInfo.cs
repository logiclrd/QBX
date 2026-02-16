using QBX.Hardware;
using System.Globalization;
using System.IO;

namespace QBX.OperatingSystem.Globalization;

public class ExtendedCountryInfo : CountryInfo
{
	// Format:
	//   Length  dw
	//   CountryCode dw
	//   CodePageID  dw
	//   COUNTRYINFO
	public new const int Size = 6 + CountryInfo.Size;

	public CountryCode CountryCode;
	public ushort CodePageID;

	public override void Import(CultureInfo cultureInfo)
	{
		CodePageID = (ushort)cultureInfo.TextInfo.OEMCodePage;
		CountryCode = cultureInfo.ToCountryCode();

		base.Import(cultureInfo);
	}

	public override void Serialize(IMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, 36);

		var writer = new BinaryWriter(stream);

		writer.Write((ushort)Size);
		writer.Write((ushort)CountryCode);
		writer.Write(CodePageID);

		base.Serialize(memory, address + 6);
	}

	public override void Deserialize(IMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, 36);

		var reader = new BinaryReader(stream);

		int size = reader.ReadUInt16();

		if (size < Size)
			return;

		CountryCode = (CountryCode)reader.ReadUInt16();
		CodePageID = reader.ReadUInt16();

		base.Serialize(memory, address + 6);
	}
}
