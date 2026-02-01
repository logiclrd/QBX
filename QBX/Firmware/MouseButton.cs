using System.IO;

namespace QBX.Firmware;

public class MouseButton(MouseDriver owner)
{
	public bool IsPressed;
	public MouseButtonCounter ClickCounter = new MouseButtonCounter();
	public MouseButtonCounter ReleaseCounter = new MouseButtonCounter();

	public void SerializeTo(BinaryWriter writer)
	{
		writer.Write(IsPressed);
		ClickCounter.SerializeTo(writer);
		ReleaseCounter.SerializeTo(writer);
	}

	public void DeserializeFrom(BinaryReader reader)
	{
		IsPressed = reader.ReadBoolean();
		ClickCounter.DeserializeFrom(reader);
		ReleaseCounter.DeserializeFrom(reader);
	}

	public void Set(bool isPressed)
	{
		if (isPressed)
			Down();
		else
			Up();
	}

	public void Down()
	{
		if (!IsPressed)
		{
			IsPressed = true;

			ClickCounter.Count++;
			ClickCounter.LastX = owner.PointerX;
			ClickCounter.LastY = owner.PointerY;
		}
	}

	public void Up()
	{
		if (IsPressed)
		{
			IsPressed = false;

			ReleaseCounter.Count++;
			ReleaseCounter.LastX = owner.PointerX;
			ReleaseCounter.LastY = owner.PointerY;
		}
	}
}
