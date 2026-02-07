using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using SDL3;

namespace QBX.Hardware;

public class Keyboard(Machine machine)
{
	object _sync = new();
	Queue<KeyEvent> _inputQueue = new Queue<KeyEvent>();
	Queue<KeyEvent> _divertedEvents = new Queue<KeyEvent>();
	bool _divertEvents = false;

	public event Action? Break;

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

		if (keyEvent.IsBreak)
			Break?.Invoke();

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
			if (_divertEvents)
				_divertedEvents.Enqueue(keyEvent);
			else
				_inputQueue.Enqueue(keyEvent);

			Monitor.PulseAll(_sync);
		}
	}

	void EatReleaseEvents(Queue<KeyEvent> queue)
	{
		while ((queue.Count > 0) && queue.Peek().IsRelease)
			queue.Dequeue();
	}

	public void DiscardQueueudInput()
	{
		lock (_sync)
		{
			if ((_waitCount > 0) && !_divertEvents) // Don't interfere with ongoing waits
				return;

			_inputQueue.Clear();
		}
	}

	public bool WaitForInput(int timeoutMilliseconds = Timeout.Infinite)
	{
		if (timeoutMilliseconds != Timeout.Infinite)
			return WaitForInput(TimeSpan.FromMilliseconds(timeoutMilliseconds));

		lock (_sync)
		{
			// Prevent entering the loop while other threads are in the process of being
			// interrupted, otherwise we might eat their interruption notification.
			if (_interruptCount > 0)
				return false;

			EatReleaseEvents(_inputQueue);

			Interlocked.Increment(ref _waitCount);

			try
			{
				while (machine.KeepRunning && (_inputQueue.Count == 0))
				{
					Monitor.Wait(_sync);

					if (_interruptCount > 0)
						return true;

					EatReleaseEvents(_inputQueue);
				}
			}
			finally
			{
				Interlocked.Decrement(ref _waitCount);
				if (_interruptCount > 0)
					Interlocked.Decrement(ref _interruptCount);
			}

			return machine.KeepRunning;
		}
	}

	public bool HasQueuedTangibleInput
	{
		get
		{
			lock (_sync)
				return _inputQueue.Any(item => !item.IsEphemeral);
		}
	}

	public bool WaitForInput(TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;

		lock (_sync)
		{
			// Prevent entering the loop while other threads are in the process of being
			// interrupted, otherwise we might eat their interruption notification.
			if (_interruptCount > 0)
				return false;

			EatReleaseEvents(_inputQueue);

			Interlocked.Increment(ref _waitCount);

			try
			{
				while (_inputQueue.Count == 0)
				{
					var remainingTime = deadline - DateTime.UtcNow;

					if (remainingTime <= TimeSpan.Zero)
						return false;

					Monitor.Wait(_sync, remainingTime);

					if (_interruptCount > 0)
						return false;

					EatReleaseEvents(_inputQueue);
				}
			}
			finally
			{
				Interlocked.Decrement(ref _waitCount);
				if (_interruptCount > 0)
					Interlocked.Decrement(ref _interruptCount);
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
			// Prevent entering the loop while other threads are in the process of being
			// interrupted, otherwise we might eat their interruption notification.
			if (_interruptCount > 0)
				return false;

			Interlocked.Increment(ref _waitCount);

			try
			{
				while (_inputQueue.Count == 0)
				{
					Monitor.Wait(_sync);

					if (_interruptCount > 0)
						return false;

					EatReleaseEvents(_inputQueue);

					if (cancellationToken.IsCancellationRequested)
						return false;
				}
			}
			finally
			{
				Interlocked.Decrement(ref _waitCount);
				if (_interruptCount > 0)
					Interlocked.Decrement(ref _interruptCount);
			}

			return true;
		}
	}

	public bool WaitForNewInput(int timeoutMilliseconds = Timeout.Infinite)
	{
		if (timeoutMilliseconds != Timeout.Infinite)
			return WaitForNewInput(TimeSpan.FromMilliseconds(timeoutMilliseconds));

		lock (_sync)
		{
			if (_divertEvents)
				throw new InvalidOperationException("WaitForNextInput does not support concurrent operation");

			// Prevent entering the loop while other threads are in the process of being
			// interrupted, otherwise we might eat their interruption notification.
			if (_interruptCount > 0)
				return false;

			_divertEvents = true;

			Interlocked.Increment(ref _waitCount);

			try
			{
				while (_divertedEvents.Count == 0)
				{
					Monitor.Wait(_sync);

					if (_interruptCount > 0)
						return false;

					EatReleaseEvents(_divertedEvents);
				}

				foreach (var newEvent in _divertedEvents)
					_inputQueue.Enqueue(newEvent);

				return true;
			}
			finally
			{
				Interlocked.Decrement(ref _waitCount);
				if (_interruptCount > 0)
					Interlocked.Decrement(ref _interruptCount);

				_divertEvents = false;
				_divertedEvents.Clear();
			}
		}
	}

	public bool WaitForNewInput(TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;

		lock (_sync)
		{
			if (_divertEvents)
				throw new InvalidOperationException("WaitForNextInput does not support concurrent operation");

			// Prevent entering the loop while other threads are in the process of being
			// interrupted, otherwise we might eat their interruption notification.
			if (_interruptCount > 0)
				return false;

			_divertEvents = true;

			Interlocked.Increment(ref _waitCount);

			try
			{
				while (_divertedEvents.Count == 0)
				{
					var remainingTime = deadline - DateTime.UtcNow;

					if (remainingTime <= TimeSpan.Zero)
						return false;

					Monitor.Wait(_sync, remainingTime);

					if (_interruptCount > 0)
						return false;

					EatReleaseEvents(_divertedEvents);
				}

				foreach (var newEvent in _divertedEvents)
					_inputQueue.Enqueue(newEvent);

				return true;
			}
			finally
			{
				Interlocked.Decrement(ref _waitCount);
				if (_interruptCount > 0)
					Interlocked.Decrement(ref _interruptCount);

				_divertEvents = false;
				_divertedEvents.Clear();
			}
		}
	}

	volatile int _waitCount;
	volatile int _interruptCount;

	public void InterruptWait()
	{
		lock (_sync)
		{
			if (_interruptCount > 0)
				return;

			_interruptCount = _waitCount;

			Monitor.PulseAll(_sync);
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
