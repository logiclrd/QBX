using System;

using Microsoft.Win32.SafeHandles;

namespace QBX.OperatingSystem;

public interface IFileDateTimeProvider
{
	DateTime GetLastWriteTime(SafeFileHandle handle);
	void SetLastWriteTime(SafeFileHandle handle, DateTime lastWriteTime);

	DateTime GetLastAccessTime(SafeFileHandle handle);
	void SetLastAccessTime(SafeFileHandle handle, DateTime lastAccessTime);

	DateTime GetCreationTime(SafeFileHandle handle);
	void SetCreationTime(SafeFileHandle handle, DateTime creationTime);
}
