using System.IO;

namespace QBX.Firmware;

public class MouseButtonCounter
{
	public int Count;
	public int LastX, LastY;

	public void SerializeTo(BinaryWriter writer)
	{
		writer.Write(Count);
		writer.Write(LastX);
		writer.Write(LastY);
	}

	public void DeserializeFrom(BinaryReader reader)
	{
		Count = reader.ReadInt32();
		LastX = reader.ReadInt32();
		LastY = reader.ReadInt32();
	}
}
