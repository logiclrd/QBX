using System;
using System.IO;

using Microsoft.Win32.SafeHandles;

namespace QBX.OperatingSystem;

class DefaultFileDateTimeProvider : IFileDateTimeProvider
{
	public DateTime GetCreationTime(SafeFileHandle handle) => File.GetCreationTime(handle);
	public DateTime GetLastAccessTime(SafeFileHandle handle) => File.GetLastAccessTime(handle);
	public DateTime GetLastWriteTime(SafeFileHandle handle) => File.GetLastWriteTime(handle);

	public void SetCreationTime(SafeFileHandle handle, DateTime creationTime) => File.SetCreationTime(handle, creationTime);
	public void SetLastAccessTime(SafeFileHandle handle, DateTime lastAccessTime) => File.SetLastAccessTime(handle, lastAccessTime);
	public void SetLastWriteTime(SafeFileHandle handle, DateTime lastWriteTime) => File.SetLastWriteTime(handle, lastWriteTime);
}
