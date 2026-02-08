using System;
using System.Runtime.InteropServices;
using System.Text;

namespace QBX.OperatingSystem;

public partial class ShortFileNames
{
	public static bool TryMap(string path, out string shortPath)
	{
		if (System.OperatingSystem.IsWindows())
			return TryMapWindows(path, out shortPath);
		else
			return TryMapEmulated(path, out shortPath);
	}
}
