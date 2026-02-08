namespace QBX.OperatingSystem;

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
}
