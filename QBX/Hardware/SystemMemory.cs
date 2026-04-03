using System;

namespace QBX.Hardware;

public class SystemMemory : IMemory
{
	byte[] _ram = new byte[640 * 1024];

	public int Length => _ram.Length;

	public KeyboardStatus KeyboardStatus { get; }

	public SystemMemory()
	{
		KeyboardStatus = new KeyboardStatus(this);
		KeyboardStatus.Byte3.EnhancedKeyboard = true;
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

