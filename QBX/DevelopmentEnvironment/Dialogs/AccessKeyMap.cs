using System.Collections.Generic;
using QBX.Firmware.Fonts;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class AccessKeyMap : Dictionary<byte, Widget>
{
	public AccessKeyMap(IEnumerable<Widget> widgets)
	{
		foreach (var widget in widgets)
		{
			foreach (var childWidget in widget.EnumerateAllWidgets())
			{
				char ch = childWidget.AccessKeyCharacter;

				if (ch != '\0')
				{
					byte accessKey = CP437Encoding.GetByteSemantic(ch);

					this[accessKey] = widget;
				}
			}
		}
	}
}
