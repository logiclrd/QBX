using System;
using System.IO;

namespace QBX.OperatingSystem;

internal class DefaultDriveInfoProvider : IDriveInfoProvider
{
	public IDriveInfo GetDrive(String path)
	{
		return new DriveInfoWrapper(new DriveInfo(path));
	}

	public IDriveInfo[] GetDrives()
	{
		var actuals = DriveInfo.GetDrives();

		var wrappers = new IDriveInfo[actuals.Length];

		for (int i=0; i < actuals.Length; i++)
			wrappers[i] = new DriveInfoWrapper(actuals[i]);

		return wrappers;
	}
}
