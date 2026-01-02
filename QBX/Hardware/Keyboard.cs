using SDL3;

namespace QBX.Hardware;

public class Keyboard
{
	object _sync = new();
	Queue<KeyEvent> _inputQueue = new Queue<KeyEvent>();

	public KeyModifiers Modifiers => _modifiers;

	MutableKeyModifiers _modifiers = new MutableKeyModifiers();

	void UpdateModifiers()
	{
		var mods = SDL.GetModState();

		_modifiers.CtrlKey = (mods & SDL.Keymod.Ctrl) != 0;
		_modifiers.AltKey = (mods & SDL.Keymod.Alt) != 0;
		_modifiers.ShiftKey = (mods & SDL.Keymod.Shift) != 0;

		_modifiers.CapsLock = (mods & SDL.Keymod.Caps) != 0;
		_modifiers.NumLock = (mods & SDL.Keymod.Num) != 0;
	}

	public void HandleEvent(SDL.KeyboardEvent evt)
	{
		switch (evt.Scancode)
		{
			case SDL.Scancode.LCtrl:
			case SDL.Scancode.RCtrl:
			case SDL.Scancode.LShift:
			case SDL.Scancode.RShift:
			case SDL.Scancode.LAlt:
			case SDL.Scancode.RAlt:
			case SDL.Scancode.Capslock:
			case SDL.Scancode.NumLockClear:
				UpdateModifiers();
				break;
		}

		var keyEvent = new KeyEvent(evt.Scancode, _modifiers.Clone(), isRelease: !evt.Down);

		lock (_sync)
		{
			_inputQueue.Enqueue(keyEvent);
			Monitor.PulseAll(_sync);
		}
	}

	public bool WaitForInput(int timeoutMilliseconds = Timeout.Infinite)
	{
		if (timeoutMilliseconds != Timeout.Infinite)
			return WaitForInput(TimeSpan.FromMilliseconds(timeoutMilliseconds));

		lock (_sync)
		{
			while (_inputQueue.Count == 0)
				Monitor.Wait(_sync);

			return true;
		}
	}

	public bool WaitForInput(TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;

		lock (_sync)
		{
			while (_inputQueue.Count == 0)
			{
				var remainingTime = deadline - DateTime.UtcNow;

				if (remainingTime <= TimeSpan.Zero)
					return false;

				Monitor.Wait(_sync, remainingTime);
			}

			return true;
		}
	}


	public bool WaitForInput(CancellationToken cancellationToken)
	{
		void NotifyWaitLoop()
		{
			lock (_sync)
				Monitor.PulseAll(_sync);
		}

		using (cancellationToken.Register(NotifyWaitLoop))
		lock (_sync)
		{
			while (_inputQueue.Count == 0)
			{
				Monitor.Wait(_sync);

				if (cancellationToken.IsCancellationRequested)
					return false;
			}

			return true;
		}
	}

	public KeyEvent? GetNextEvent()
	{
		lock (_sync)
		{
			_inputQueue.TryDequeue(out var evt);

			return evt;
		}
	}
}
