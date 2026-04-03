using QBX.Firmware;

namespace QBX.Hardware;

public class KeyboardStatus(SystemMemory systemMemory)
{
	// As documented here: https://flint.cs.yale.edu/feng/cos/resources/BIOS/Resources/assembly/kbflags.html

	public const int Byte0Address = 1047;
	public const int Byte1Address = 1048;
	public const int Byte2Address = 1121;
	public const int Byte3Address = 1120;

	public abstract class ByteData(SystemMemory memory)
	{
		public abstract int Address { get; }

		public byte Get() => memory[Address];
		public void Set(int newValue) => memory[Address] = unchecked((byte)newValue);

		public static implicit operator byte(ByteData data) => data.Get();
	}

	public class Byte0Data(SystemMemory memory) : ByteData(memory)
	{
		public override int Address => Byte0Address;

		public const int RightShiftBit = 1;
		public const int LeftShiftBit = 2;
		public const int ControlBit = 4;
		public const int AltBit = 8;
		public const int ScrollLockBit = 16;
		public const int NumLockBit = 32;
		public const int CapsLockBit = 64;
		public const int InsertBit = 128;

		public bool RightShift { get => IsBitSet(this, RightShiftBit); set => SetBit(this, RightShiftBit, value); }
		public bool LeftShift { get => IsBitSet(this, LeftShiftBit); set => SetBit(this, LeftShiftBit, value); }
		public bool Control { get => IsBitSet(this, ControlBit); set => SetBit(this, ControlBit, value); }
		public bool Alt { get => IsBitSet(this, AltBit); set => SetBit(this, AltBit, value); }
		public bool ScrollLock { get => IsBitSet(this, ScrollLockBit); set => SetBit(this, ScrollLockBit, value); }
		public bool NumLock { get => IsBitSet(this, NumLockBit); set => SetBit(this, NumLockBit, value); }
		public bool CapsLock { get => IsBitSet(this, CapsLockBit); set => SetBit(this, CapsLockBit, value); }
		public bool Insert { get => IsBitSet(this, InsertBit); set => SetBit(this, InsertBit, value); }
	}

	public class Byte1Data(SystemMemory memory) : ByteData(memory)
	{
		public override int Address => Byte1Address;

		public const int LeftControlBit = 1;
		public const int LeftAltBit = 2;
		public const int SystemKeyBit = 4;
		public const int SuspendKeyChangedBit = 8;
		public const int ScrollLockKeyPressedBit = 16;
		public const int NumLockKeyPressedBit = 32;
		public const int CapsLockKeyPressedBit = 64;
		public const int InsertKeyPressedBit = 128;

		public bool LeftControl { get => IsBitSet(this, LeftControlBit); set => SetBit(this, LeftControlBit, value); }
		public bool LeftAlt { get => IsBitSet(this, LeftAltBit); set => SetBit(this, LeftAltBit, value); }
		public bool SystemKey { get => IsBitSet(this, SystemKeyBit); set => SetBit(this, SystemKeyBit, value); }
		public bool SuspendKeyChanged { get => IsBitSet(this, SuspendKeyChangedBit); set => SetBit(this, SuspendKeyChangedBit, value); }
		public bool ScrollLockKeyPressed { get => IsBitSet(this, ScrollLockKeyPressedBit); set => SetBit(this, ScrollLockKeyPressedBit, value); }
		public bool NumLockKeyPressed { get => IsBitSet(this, NumLockKeyPressedBit); set => SetBit(this, NumLockKeyPressedBit, value); }
		public bool CapsLockKeyPressed { get => IsBitSet(this, CapsLockKeyPressedBit); set => SetBit(this, CapsLockKeyPressedBit, value); }
		public bool InsertKeyPressed { get => IsBitSet(this, InsertKeyPressedBit); set => SetBit(this, InsertKeyPressedBit, value); }
	}

	public class Byte2Data(SystemMemory memory) : ByteData(memory)
	{
		public override int Address => Byte2Address;

		public const int ScrollLockIndicatorBit = 1;
		public const int NumLockIndicatorBit = 2;
		public const int CapsLockIndicatorBit = 4;
		public const int CircusSystemIndicatorBit = 8; // ?
		public const int ACKReceivedBit = 16;
		public const int ResendReceivedBit = 32;
		public const int ModeIndicatorUpdateBit = 64;
		public const int KeyboardTransmitErrorBit = 128;

		public bool ScrollLockIndicator { get => IsBitSet(this, ScrollLockIndicatorBit); set => SetBit(this, ScrollLockIndicatorBit, value); }
		public bool NumLockIndicator { get => IsBitSet(this, NumLockIndicatorBit); set => SetBit(this, NumLockIndicatorBit, value); }
		public bool CapsLockIndicator { get => IsBitSet(this, CapsLockIndicatorBit); set => SetBit(this, CapsLockIndicatorBit, value); }
		public bool CircusSystemIndicator { get => IsBitSet(this, CircusSystemIndicatorBit); set => SetBit(this, CircusSystemIndicatorBit, value); }
		public bool ACKReceived { get => IsBitSet(this, ACKReceivedBit); set => SetBit(this, ACKReceivedBit, value); }
		public bool ResendReceived { get => IsBitSet(this, ResendReceivedBit); set => SetBit(this, ResendReceivedBit, value); }
		public bool ModeIndicatorUpdate { get => IsBitSet(this, ModeIndicatorUpdateBit); set => SetBit(this, ModeIndicatorUpdateBit, value); }
		public bool KeyboardTransmitError { get => IsBitSet(this, KeyboardTransmitErrorBit); set => SetBit(this, KeyboardTransmitErrorBit, value); }
	}

	public class Byte3Data(SystemMemory memory) : ByteData(memory)
	{
		public override int Address => Byte3Address;

		public const int LastWasE0HiddenBit = 1;
		public const int LastWasE1HiddenBit = 2;
		public const int RightControlBit = 4;
		public const int RightAltBit = 8;
		public const int EnhancedKeyboardBit = 16;
		public const int ForceNumLockIfRdBit = 32;
		public const int LastCharWasFirstIDCharBit = 64;
		public const int ReadIDInProgressBit = 128;

		public bool LastWasE0Hidden { get => IsBitSet(this, LastWasE0HiddenBit); set => SetBit(this, LastWasE0HiddenBit, value); }
		public bool LastWasE1Hidden { get => IsBitSet(this, LastWasE1HiddenBit); set => SetBit(this, LastWasE1HiddenBit, value); }
		public bool RightControl { get => IsBitSet(this, RightControlBit); set => SetBit(this, RightControlBit, value); }
		public bool RightAlt { get => IsBitSet(this, RightAltBit); set => SetBit(this, RightAltBit, value); }
		public bool EnhancedKeyboard { get => IsBitSet(this, EnhancedKeyboardBit); set => SetBit(this, EnhancedKeyboardBit, value); }
		public bool ForceNumLockIfRd { get => IsBitSet(this, ForceNumLockIfRdBit); set => SetBit(this, ForceNumLockIfRdBit, value); }
		public bool LastCharWasFirstIDChar { get => IsBitSet(this, LastCharWasFirstIDCharBit); set => SetBit(this, LastCharWasFirstIDCharBit, value); }
		public bool ReadIDInProgress { get => IsBitSet(this, ReadIDInProgressBit); set => SetBit(this, ReadIDInProgressBit, value); }
	}

	public Byte0Data Byte0 = new Byte0Data(systemMemory);
	public Byte1Data Byte1 = new Byte1Data(systemMemory);
	public Byte2Data Byte2 = new Byte2Data(systemMemory);
	public Byte3Data Byte3 = new Byte3Data(systemMemory);

	static bool IsBitSet(ByteData data, int bit) => (data & bit) != 0;

	static void SetBit(ByteData data, int bit, bool value)
	{
		int bits = data;

		if (value)
			bits |= bit;
		else
			bits &= ~bit;

		data.Set(bits);
	}

	public bool RightShift => Byte0.RightShift;
	public bool LeftShift => Byte0.LeftShift;
	public bool Control => Byte0.Control;
	public bool Alt => Byte0.Alt;
	public bool AltGr => Byte3.RightAlt;
	public bool ScrollLock => Byte0.ScrollLock;
	public bool NumLock => Byte0.NumLock;
	public bool CapsLock => Byte0.CapsLock;
	public bool Insert => Byte0.Insert;

	public void UpdateKeyModifiers(RawKeyEventData evt)
	{
		switch (evt.RawScanCode)
		{
			case SDL3.SDL.Scancode.LShift: Byte0.LeftShift = !evt.IsRelease; break;
			case SDL3.SDL.Scancode.RShift: Byte0.RightShift = !evt.IsRelease; break;
			case SDL3.SDL.Scancode.LCtrl: Byte1.LeftControl = !evt.IsRelease; Byte0.Control = Byte1.LeftControl || Byte3.RightControl; break;
			case SDL3.SDL.Scancode.RCtrl: Byte3.RightControl = !evt.IsRelease; Byte0.Control = Byte1.LeftControl || Byte3.RightControl; break;
			case SDL3.SDL.Scancode.LAlt: Byte1.LeftAlt = !evt.IsRelease; Byte0.Alt = Byte1.LeftAlt || Byte3.RightAlt; break;
			case SDL3.SDL.Scancode.RAlt: Byte3.RightAlt = !evt.IsRelease; Byte0.Alt = Byte1.LeftAlt || Byte3.RightAlt; break;
			case SDL3.SDL.Scancode.Capslock: Byte1.CapsLockKeyPressed = !evt.IsRelease; break;
			case SDL3.SDL.Scancode.NumLockClear: Byte1.NumLockKeyPressed = !evt.IsRelease; break;
			case SDL3.SDL.Scancode.Scrolllock: Byte1.ScrollLockKeyPressed = !evt.IsRelease; break;
			case SDL3.SDL.Scancode.Insert: if (!evt.IsRelease) Byte0.Insert = !Byte0.Insert; break;
		}
	}

	public KeyModifiers GetKeyModifiers()
	{
		byte byte0 = systemMemory[Byte0Address];

		return new KeyModifiers(
			ctrlKey: (byte0 & Byte0Data.ControlBit) != 0,
			altKey: (byte0 & Byte0Data.AltBit) != 0,
			altGrKey: Byte3.RightAlt,
			shiftKey: (byte0 & (Byte0Data.LeftShiftBit | Byte0Data.RightShiftBit)) != 0,
			capsLock: (byte0 & Byte0Data.CapsLockBit) != 0,
			numLock: (byte0 & Byte0Data.NumLockBit) != 0,
			scrollLock: (byte0 & Byte0Data.ScrollLockBit) != 0);
	}
}

