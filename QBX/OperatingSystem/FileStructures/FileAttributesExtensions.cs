namespace QBX.OperatingSystem.FileStructures;

public static class FileAttributesExtensions
{
	public static byte ToDOSFileAttributesByte(this System.IO.FileAttributes bits)
		=> (byte)bits.ToDOSFileAttributes();

	public static FileAttributes ToDOSFileAttributes(this System.IO.FileAttributes bits)
	{
		var result = default(FileAttributes);

		if ((bits & System.IO.FileAttributes.ReadOnly) != 0)
			result |= FileAttributes.ReadOnly;
		if ((bits & System.IO.FileAttributes.Hidden) != 0)
			result |= FileAttributes.Hidden;
		if ((bits & System.IO.FileAttributes.System) != 0)
			result |= FileAttributes.System;
		if ((bits & System.IO.FileAttributes.Directory) != 0)
			result |= FileAttributes.Directory;
		if ((bits & System.IO.FileAttributes.Archive) != 0)
			result |= FileAttributes.Archive;

		return result;
	}

	public static System.IO.FileAttributes ToSystemFileAttributes(this FileAttributes bits)
	{
		var result = default(System.IO.FileAttributes);

		if ((bits & FileAttributes.ReadOnly) != 0)
			result |= System.IO.FileAttributes.ReadOnly;
		if ((bits & FileAttributes.Hidden) != 0)
			result |= System.IO.FileAttributes.Hidden;
		if ((bits & FileAttributes.System) != 0)
			result |= System.IO.FileAttributes.System;
		if ((bits & FileAttributes.Directory) != 0)
			result |= System.IO.FileAttributes.Directory;
		if ((bits & FileAttributes.Archive) != 0)
			result |= System.IO.FileAttributes.Archive;

		return result;
	}
}
