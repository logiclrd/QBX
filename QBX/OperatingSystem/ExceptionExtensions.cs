using System;
using System.ComponentModel;
using System.IO;

namespace QBX.OperatingSystem;

public static class ExceptionExtensions
{
	public static DOSError ToDOSError(this Exception exception)
	{
		if (exception is DOSException dosException)
			return dosException.Error;

		if (exception is Win32Exception win32Exception)
		{
			if (Enum.IsDefined(typeof(DOSError), win32Exception.NativeErrorCode))
				return (DOSError)win32Exception.NativeErrorCode;
		}

		if ((exception.HResult & 0xFFFF0000) == 0x80070000)
		{
			int errorCode = exception.HResult & 0xFFFF;

			if (Enum.IsDefined(typeof(DOSError), errorCode))
				return (DOSError)errorCode;
		}

		switch (exception)
		{
			case InvalidOperationException: return DOSError.InvalidFunction;
			case FileNotFoundException: return DOSError.FileNotFound;
			case DirectoryNotFoundException: return DOSError.PathNotFound;
			case UnauthorizedAccessException: return DOSError.AccessDenied;
			case FormatException: return DOSError.BadFormat;
			case AccessViolationException: return DOSError.ReadFault;
			case EndOfStreamException: return DOSError.HandleEOF;
			case NotSupportedException: return DOSError.NotSupported;
		}

		return DOSError.GeneralFailure;
	}
}
