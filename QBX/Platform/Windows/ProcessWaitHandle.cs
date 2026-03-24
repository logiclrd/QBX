using System;
using System.Threading;

using Microsoft.Win32.SafeHandles;

namespace QBX.Platform.Windows;

public class ProcessWaitHandle : WaitHandle
{
	public ProcessWaitHandle(IntPtr hProcess)
	{
		SafeWaitHandle = new SafeWaitHandle(hProcess, ownsHandle: false);
	}
}
