using SDL3;

namespace QBX.Firmware;

public class RawKeyEventData(SDL.Scancode rawScanCode, SDL.Keymod modifiers, bool isRelease)
{
	public SDL.Scancode RawScanCode => rawScanCode;
	public SDL.Keymod Modifiers => modifiers;
	public bool IsRelease => isRelease;
}
