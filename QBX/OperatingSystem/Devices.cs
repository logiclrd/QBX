using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using QBX.OperatingSystem.FileDescriptors;

namespace QBX.OperatingSystem;

public class Devices
{
	Dictionary<string, FileDescriptor> _devices = new Dictionary<string, FileDescriptor>(StringComparer.InvariantCultureIgnoreCase);

	public readonly ClockFileDescriptor Clock;
	public readonly ConsoleFileDescriptor Console;
	public readonly NullFileDescriptor Null;

	public bool TryGetDeviceByName(string name, [NotNullWhen(true)] out FileDescriptor? device)
		=> _devices.TryGetValue(name, out device);

	public Devices(DOS owner)
	{
		Clock = new ClockFileDescriptor();
		Console = new ConsoleFileDescriptor(owner);
		Null = new NullFileDescriptor();

		_devices["CLOCK$"] = Clock;
		_devices["CON"] = Console;
		_devices["NUL"] = Null;
	}
}
