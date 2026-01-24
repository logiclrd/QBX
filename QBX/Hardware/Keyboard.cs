using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using SDL3;

namespace QBX.Hardware;

public class Keyboard(Machine machine)
{
	object _sync = new();
	Queue<KeyEvent> _inputQueue = new Queue<KeyEvent>();

	void UpdateModifiers(SDL.KeyboardEvent evt)
	{
		var mods = SDL.GetModState();

		int keyboardStatus = machine.SystemMemory[SystemMemory.KeyboardStatusAddress];

		if ((mods & SDL.Keymod.Ctrl) != 0)
			keyboardStatus |= SystemMemory.KeyboardStatus_ControlBit;
		else
			keyboardStatus &= ~SystemMemory.KeyboardStatus_ControlBit;

		if ((mods & SDL.Keymod.Alt) != 0)
			keyboardStatus |= SystemMemory.KeyboardStatus_AltBit;
		else
			keyboardStatus &= ~SystemMemory.KeyboardStatus_AltBit;

		if ((mods & SDL.Keymod.Shift) == 0)
			keyboardStatus &= ~(SystemMemory.KeyboardStatus_LeftShiftBit | SystemMemory.KeyboardStatus_RightShiftBit);
		else
		{
			int shiftBit =
				evt.Scancode switch
				{
					SDL.Scancode.LShift => SystemMemory.KeyboardStatus_LeftShiftBit,
					SDL.Scancode.RShift => SystemMemory.KeyboardStatus_RightShiftBit,

					_ => 0
				};

			if (evt.Down)
				keyboardStatus |= shiftBit;
			else
				keyboardStatus &= ~shiftBit;
		}

		if ((mods & SDL.Keymod.Scroll) != 0)
			keyboardStatus |= SystemMemory.KeyboardStatus_ScrollLockBit;
		else
			keyboardStatus &= ~SystemMemory.KeyboardStatus_ScrollLockBit;

		if ((mods & SDL.Keymod.Num) != 0)
			keyboardStatus |= SystemMemory.KeyboardStatus_NumLockBit;
		else
			keyboardStatus &= ~SystemMemory.KeyboardStatus_NumLockBit;

		if ((mods & SDL.Keymod.Caps) != 0)
			keyboardStatus |= SystemMemory.KeyboardStatus_CapsLockBit;
		else
			keyboardStatus &= ~SystemMemory.KeyboardStatus_CapsLockBit;

		if (evt.Down && (evt.Scancode == SDL.Scancode.Insert))
			keyboardStatus ^= SystemMemory.KeyboardStatus_InsertBit;

		machine.SystemMemory[SystemMemory.KeyboardStatusAddress] =
			unchecked((byte)keyboardStatus);
	}

	bool? _suppressNextIfIsRelease;
	ScanCode? _suppressNextIfHasScanCode;

	public void SuppressNextEventIf(bool? isRelease, ScanCode? hasScanCode)
	{
		_suppressNextIfIsRelease = isRelease;
		_suppressNextIfHasScanCode = hasScanCode;
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
				UpdateModifiers(evt);
				break;
		}

		var keyEvent = new KeyEvent(evt.Scancode, machine.SystemMemory.GetKeyModifiers(), isRelease: !evt.Down);

		if (_suppressNextIfIsRelease.HasValue || _suppressNextIfHasScanCode.HasValue)
		{
			bool suppress = true;

			if (_suppressNextIfIsRelease.HasValue && (keyEvent.IsRelease != _suppressNextIfIsRelease))
				suppress = false;
			if (_suppressNextIfHasScanCode.HasValue && (keyEvent.ScanCode != _suppressNextIfHasScanCode))
				suppress = false;

			_suppressNextIfIsRelease = null;
			_suppressNextIfHasScanCode = null;

			if (suppress)
				return;
		}

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
			while (machine.KeepRunning && (_inputQueue.Count == 0))
				Monitor.Wait(_sync);

			return machine.KeepRunning;
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

	public bool WaitForNewInput(int timeoutMilliseconds = Timeout.Infinite)
	{
		if (timeoutMilliseconds != Timeout.Infinite)
			return WaitForNewInput(TimeSpan.FromMilliseconds(timeoutMilliseconds));

		int countOnEntry = _inputQueue.Count;

		lock (_sync)
		{
			while (_inputQueue.Count == countOnEntry)
				Monitor.Wait(_sync);

			return true;
		}
	}

	public bool WaitForNewInput(TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;

		int countOnEntry = _inputQueue.Count;

		lock (_sync)
		{
			while (_inputQueue.Count == countOnEntry)
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

	public void OutPort(int portNumber, byte data)
	{
		// TODO: https://wiki.osdev.org/I8042_PS/2_Controller#Data_Port
	}

	public byte InPort(int portNumber, out bool handled)
	{
		handled = false;
		return 0;
		// TODO
	}
}
