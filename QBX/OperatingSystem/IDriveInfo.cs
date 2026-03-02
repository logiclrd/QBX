using System.IO;

namespace QBX.OperatingSystem;

public interface IDriveInfo
{
	string Name { get; }
	bool IsReady { get; }
	string RootDirectoryPath { get; }
	DriveType DriveType { get; }
	string DriveFormat { get; }
	long AvailableFreeSpace { get; }
	long TotalFreeSpace { get; }
	long TotalSize { get; }
	string VolumeLabel { get; }
}
