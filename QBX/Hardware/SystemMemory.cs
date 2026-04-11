using System;
using System.Runtime.InteropServices;

namespace QBX.Hardware;

public class SystemMemory : IMemory
{
	byte[] _ram = new byte[640 * 1024];

	public int Length => _ram.Length;

	public KeyboardStatus KeyboardStatus { get; }

	Machine _machine;

	public SystemMemory(Machine machine)
	{
		KeyboardStatus = new KeyboardStatus(this);
		KeyboardStatus.Byte3.EnhancedKeyboard = true;

		_machine = machine;
	}

	public void UpdateDynamicData(int rangeStart, int rangeEnd)
	{
		// A compromise: Some dynamic data properly requires constant
		// updating. Instead of burning the CPU cycles, we just pretend
		// that it has been updated by allowing callers to notify us
		// when they're about to do an operation that could depend on
		// up-to-date information.

		// System timer
		const int SystemTimerAddress = 0x46C;

		if ((rangeStart < SystemTimerAddress + 4)
		 && (rangeEnd >= SystemTimerAddress))
		{
			var systemTimerBytes = AsSpan().Slice(SystemTimerAddress, 4);

			var systemTimerWord = MemoryMarshal.Cast<byte, int>(systemTimerBytes);

			systemTimerWord[0] = _machine.Timer.Timer0.Intervals;
		}
	}

	public Span<byte> AsSpan() => _ram;

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

	public bool TryGetReadOnlySpan(int offset, int length, out ReadOnlySpan<byte> span)
	{
		bool result = TryGetSpan(offset, length, out var readWriteSpan);

		span = readWriteSpan;
		return result;
	}

	public bool TryGetSpan(int offset, int length, out Span<byte> span)
	{
		if ((offset >= 0)
		 && (offset < _ram.Length)
		 && (offset + length <= _ram.Length))
		{
			span = _ram.AsSpan().Slice(offset, length);
			return true;
		}

		span = Span<byte>.Empty;
		return false;
	}
}

