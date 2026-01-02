namespace QBX.Hardware;

public class MutableKeyModifiers : KeyModifiers
{
	public new bool CtrlKey
	{
		get => _ctrlKey;
		set => _ctrlKey = value;
	}

	public new bool AltKey
	{
		get => _altKey;
		set => _altKey = value;
	}

	public new bool ShiftKey
	{
		get => _shiftKey;
		set => _shiftKey = value;
	}


	public new bool CapsLock
	{
		get => _capsLock;
		set => _capsLock = value;
	}

	public new bool NumLock
	{
		get => _numLock;
		set => _numLock = value;
	}
}
