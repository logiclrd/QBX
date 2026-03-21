namespace QBX.Hardware;

public class KeyModifiers
{
	public bool CtrlKey => _ctrlKey;
	public bool AltKey => _altKey;
	public bool ShiftKey => _shiftKey;

	public bool CapsLock => _capsLock;
	public bool NumLock => _numLock;

	protected bool _ctrlKey;
	protected bool _altKey;
	protected bool _shiftKey;

	protected bool _capsLock;
	protected bool _numLock;

	public KeyModifiers() { }

	public KeyModifiers(bool ctrlKey, bool altKey, bool shiftKey, bool capsLock, bool numLock)
	{
		_ctrlKey = ctrlKey;
		_altKey = altKey;
		_shiftKey = shiftKey;

		_capsLock = capsLock;
		_numLock = numLock;
	}

	public KeyModifiers Clone() => new KeyModifiers(_ctrlKey, _altKey, _shiftKey, _capsLock, _numLock);

	public byte Pack()
	{
		return (byte)(
			(_ctrlKey ? 1 : 0) |
			(_altKey ? 2 : 0) |
			(_shiftKey ? 4 : 0) |
			(_capsLock ? 8 : 0) |
			(_numLock ? 16 : 0));
	}

	public static KeyModifiers Unpack(byte value)
	{
		bool ctrlKey = (value & 1) != 0;
		bool altKey = (value & 2) != 0;
		bool shiftKey = (value & 4) != 0;
		bool capsLock = (value & 8) != 0;
		bool numLock = (value & 16) != 0;

		return new KeyModifiers(ctrlKey, altKey, shiftKey, capsLock, numLock);
	}
}
