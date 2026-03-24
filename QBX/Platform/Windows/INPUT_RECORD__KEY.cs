using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct INPUT_RECORD__KEY
{
	public readonly InputEventTypes EventType = InputEventTypes.KEY_EVENT;
	public KEY_INPUT_RECORD KeyEvent;

	public INPUT_RECORD__KEY() { }
}
