using System;
using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

public class GlobalHeapMemoryAllocation : IDisposable
{
	public int Size { get; }
	public IntPtr Address { get; private set; }

	public GlobalHeapMemoryAllocation(int size)
	{
		Size = size;
		Address = Marshal.AllocHGlobal(size);
	}

	public void Dispose()
	{
		if (Address != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(Address);
			Address = IntPtr.Zero;
		}
	}

	public static implicit operator IntPtr(GlobalHeapMemoryAllocation memoryAllocation)
		=> memoryAllocation.Address;
}

