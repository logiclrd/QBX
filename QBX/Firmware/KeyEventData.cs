using QBX.Hardware;

namespace QBX.Firmware;

public class KeyEventData(RawKeyEventData rawKeyEventData, char textCharacter, ScanCode scanCode, KeyModifiers modifiers, bool isRight, bool isKeyPad, bool isEphemeral)
{
	public RawKeyEventData RawKeyEventData => rawKeyEventData;

	public KeyModifiers Modifiers => modifiers;

	public char TextCharacter => textCharacter;
	public ScanCode ScanCode => scanCode;
	public bool IsRight => isRight;
	public bool IsKeyPad => isKeyPad;
	public bool IsEphemeral => isEphemeral;
}
