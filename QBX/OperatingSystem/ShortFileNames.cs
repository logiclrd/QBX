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

	public static bool TryMap(string path, string shortName)
	{
		if (System.OperatingSystem.IsWindows())
			return TryMapWindows(path, shortName);
		else
			return TryMapEmulated(path, shortName);
	}

	public static string GetFullPath(string relativePath)
	{
		if (System.OperatingSystem.IsWindows())
			return GetFullPathWindows(relativePath);
		else
			return GetFullPathEmulated(relativePath);
	}

	public static string Unmap(string shortPath)
	{
		if (System.OperatingSystem.IsWindows())
			return UnmapWindows(shortPath);
		else
			return UnmapEmulated(shortPath);
	}

	public static void Forget(string longPath)
	{
		if (System.OperatingSystem.IsWindows())
			ForgetWindows(longPath);
		else
			ForgetEmulated(longPath);
	}
}
