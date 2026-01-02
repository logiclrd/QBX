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
}
