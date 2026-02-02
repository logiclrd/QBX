using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace QBX.Utility;

public class SystemDetector
{
	public bool IsLaptop()
	{
		return WMIIsLaptop() || LinuxIsLaptop() || OSXIsLaptop();
	}

	static HashSet<ushort> LaptopChassisTypes =
		new HashSet<ushort>()
		{
			8, // Laptop
			9, // Notebook
			10, // Portable
			14, // Sub-notebook
		};

	bool WMIIsLaptop()
	{
		if (!OperatingSystem.IsWindows())
			return false;

		try
		{
			var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure");

			foreach (var obj in searcher.Get())
			{
				ushort[]? chassisTypes = obj.GetPropertyValue("ChassisTypes") as ushort[];

				if (chassisTypes != null)
					return chassisTypes.Any(LaptopChassisTypes.Contains);
			}
		}
		catch { }

		return false;
	}

	public bool LinuxIsLaptop()
	{
		// Try laptop-detect, a Debian standard utility.
		try
		{
			const string LaptopDetectFullPath = "/usr/bin/laptop-detect";
			const string LaptopDetectNameOnly = "laptop-detect";

			string laptopDetect = File.Exists(LaptopDetectFullPath) ? LaptopDetectFullPath : LaptopDetectNameOnly;

			using (var process = Process.Start(laptopDetect))
			{
				process.WaitForExit();

				return (process.ExitCode == 0);
			}
		}
		catch { }

		// Check decoded DMI data for a chassis type.
		try
		{
			string chassisTypeString = File.ReadAllText("/sys/class/dmi/id/chassis_type");

			ushort chassisType = ushort.Parse(chassisTypeString);

			return LaptopChassisTypes.Contains(chassisType);
		}
		catch { }

		// Check for the presence of a "Lid" button.
		try
		{
			return Directory.Exists("/proc/acpi/button/lid");
		}
		catch { }

		return false;
	}

	public bool OSXIsLaptop()
	{
		try
		{
			var psi = new ProcessStartInfo();

			psi.FileName = "/usr/sbin/system_profiler";
			psi.Arguments = "SPHardwareDataType";
			psi.RedirectStandardOutput = true;

			using (var process = Process.Start(psi))
			{
				if (process != null)
				{
					while (true)
					{
						string? line = process.StandardOutput.ReadLine();

						if (line == null)
							break;

						if (line.Contains("Model Identifier:")
						 && line.Contains("Book"))
							return true;
					}
				}
			}
		}
		catch { }

		return false;
	}
}
