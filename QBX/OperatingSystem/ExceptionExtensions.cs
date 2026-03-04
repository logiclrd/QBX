using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

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
			case OutOfMemoryException: return DOSError.NotEnoughMemory;
		}

		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var errno = (ErrNo)exception.HResult;

			switch (errno)
			{
				case ErrNo.EPERM: return DOSError.AccessDenied; //Operation not permitted
				case ErrNo.EACCES: return DOSError.AccessDenied; //Permission denied
				case ErrNo.ENOENT: return DOSError.FileNotFound; //No such file or directory
				case ErrNo.ESRCH: return DOSError.FileNotFound; //No such process
				case ErrNo.E2BIG: return DOSError.TooManyNames; //Argument list too long
				case ErrNo.EBADF: return DOSError.InvalidHandle; //Bad file descriptor
				case ErrNo.ENOMEM: return DOSError.NotEnoughMemory; //Cannot allocate memory
				case ErrNo.EEXIST: return DOSError.FileExists; //File exists
				case ErrNo.ENODEV: return DOSError.FileNotFound; //No such device
				case ErrNo.ENOTDIR: return DOSError.PathNotFound; //Not a directory
				case ErrNo.EISDIR: return DOSError.FileNotFound; //Is a directory
				case ErrNo.EINVAL: return DOSError.InvalidParameter; //Invalid argument
				case ErrNo.ENFILE: return DOSError.TooManyOpenFiles; //Too many open files in system
				case ErrNo.EMFILE: return DOSError.TooManyOpenFiles; //Too many open files
				case ErrNo.ENOTTY: return DOSError.InvalidFunction; //Inappropriate ioctl for device
				case ErrNo.ENOSPC: return DOSError.HandleDiskFull; //No space left on device
				case ErrNo.ESPIPE: return DOSError.InvalidFunction; //Illegal seek
				case ErrNo.EROFS: return DOSError.AccessDenied; //Read-only file system
				case ErrNo.EDOM: return DOSError.InvalidParameter; //Numerical argument out of domain
				case ErrNo.ENOTEMPTY: return DOSError.AccessDenied; //Directory not empty
				case ErrNo.ENOMSG: return DOSError.FileNotFound; //No message of desired type
				case ErrNo.ENOSR: return DOSError.TooManyOpenFiles; //Out of streams resources
				case ErrNo.EUSERS: return DOSError.TooManySessions; //Too many users
				case ErrNo.ENOTSOCK: return DOSError.InvalidFunction; //Socket operation on non-socket
				case ErrNo.EPROTOTYPE: return DOSError.InvalidFunction; //Protocol wrong type for socket
				case ErrNo.ENOPROTOOPT: return DOSError.InvalidFunction; //Protocol not available
				case ErrNo.EPROTONOSUPPORT: return DOSError.InvalidFunction; //Protocol not supported
				case ErrNo.ESOCKTNOSUPPORT: return DOSError.InvalidFunction; //Socket type not supported
				case ErrNo.EOPNOTSUPP: return DOSError.InvalidFunction; //Operation not supported
				case ErrNo.EPFNOSUPPORT: return DOSError.InvalidFunction; //Protocol family not supported
				case ErrNo.EAFNOSUPPORT: return DOSError.InvalidFunction; //Address family not supported by protocol
				case ErrNo.EADDRINUSE: return DOSError.FileExists; //Address already in use
				case ErrNo.ENOBUFS: return DOSError.NotEnoughMemory; //No buffer space available
				case ErrNo.EDQUOT: return DOSError.HandleDiskFull; //Disk quota exceeded
			}
		}

		return DOSError.GeneralFailure;
	}
}
