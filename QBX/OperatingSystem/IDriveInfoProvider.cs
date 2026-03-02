namespace QBX.OperatingSystem;

public interface IDriveInfoProvider
{
	IDriveInfo[] GetDrives();
	IDriveInfo GetDrive(string path);
}
