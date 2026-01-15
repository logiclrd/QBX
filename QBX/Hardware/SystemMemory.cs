namespace QBX.Hardware;

public class SystemMemory : IMemory
{
	byte[] _ram = new byte[640 * 1024];

	public int Length => _ram.Length;

	public const int KeyboardStatusAddress = 1047;

	bool KeyboardStatusBitSet(int bit) => (_ram[KeyboardStatusAddress] & bit) != 0;

	public const int KeyboardStatus_RightShiftBit = 1;
	public const int KeyboardStatus_LeftShiftBit = 2;
	public const int KeyboardStatus_ControlBit = 4;
	public const int KeyboardStatus_AltBit = 8;
	public const int KeyboardStatus_ScrollLockBit = 16;
	public const int KeyboardStatus_NumLockBit = 32;
	public const int KeyboardStatus_CapsLockBit = 64;
	public const int KeyboardStatus_InsertBit =  28;

	public bool KeyboardStatus_RightShift => KeyboardStatusBitSet(KeyboardStatus_RightShiftBit);
	public bool KeyboardStatus_LeftShift => KeyboardStatusBitSet(KeyboardStatus_LeftShiftBit);
	public bool KeyboardStatus_Control => KeyboardStatusBitSet(KeyboardStatus_ControlBit);
	public bool KeyboardStatus_Alt => KeyboardStatusBitSet(KeyboardStatus_AltBit);
	public bool KeyboardStatus_ScrollLock => KeyboardStatusBitSet(KeyboardStatus_ScrollLockBit);
	public bool KeyboardStatus_NumLock => KeyboardStatusBitSet(KeyboardStatus_NumLockBit);
	public bool KeyboardStatus_CapsLock => KeyboardStatusBitSet(KeyboardStatus_CapsLockBit);
	public bool KeyboardStatus_Insert => KeyboardStatusBitSet(KeyboardStatus_InsertBit);

	public KeyModifiers GetKeyModifiers()
	{
		int bits = _ram[KeyboardStatusAddress];

		return new KeyModifiers(
			ctrlKey: (bits & KeyboardStatus_ControlBit) != 0,
			altKey: (bits & KeyboardStatus_AltBit) != 0,
			shiftKey: (bits & (KeyboardStatus_LeftShiftBit | KeyboardStatus_RightShiftBit)) != 0,
			capsLock: (bits & KeyboardStatus_CapsLockBit) != 0,
			numLock: (bits & KeyboardStatus_NumLockBit) != 0);
	}

	public byte this[int address]
	{
		get => ((address >= 0) && (address < _ram.Length)) ? _ram[address] : (byte)0;
		set
		{
			if ((address < 0) || (address >= _ram.Length))
				return;

			_ram[address] = value;
		}
	}
}

