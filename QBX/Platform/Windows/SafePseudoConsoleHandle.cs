using System;
using System.Runtime.Versioning;

using Microsoft.Win32.SafeHandles;

using QBX.Utility;

namespace QBX.Platform.Windows;

[SupportedOSPlatform(PlatformNames.Windows)]
public class SafePseudoConsoleHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	public SafePseudoConsoleHandle()
		: base(ownsHandle: true)
	{
	}

	public SafePseudoConsoleHandle(IntPtr handle, bool ownsHandle)
		: base(ownsHandle)
	{
		SetHandle(handle);
	}

	protected override bool ReleaseHandle()
	{
		NativeMethods.ClosePseudoConsole(handle);
		return true;
	}
}
