using System.IO;

namespace QBX.OperatingSystem;

internal class DriveInfoWrapper(DriveInfo actual) : IDriveInfo
{
	public string Name => actual.Name;
	public bool IsReady => actual.IsReady;
	public string RootDirectoryPath => actual.RootDirectory.FullName;
	public DriveType DriveType => actual.DriveType;
	public string DriveFormat => actual.DriveFormat;
	public long AvailableFreeSpace => actual.AvailableFreeSpace;
	public long TotalFreeSpace => actual.TotalFreeSpace;
	public long TotalSize => actual.TotalSize;
	public string VolumeLabel => actual.VolumeLabel;
}
