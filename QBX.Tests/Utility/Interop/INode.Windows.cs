using Microsoft.Win32.SafeHandles;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QBX.Tests.Utility.Interop;

public class FileIndex : INode<FileIndex>
{
	public ulong VolumeSerialNumber;
	public ulong FileIdLow;
	public ulong FileIdHigh;

	public override bool IsSameVolumeAndFileAs(FileIndex other)
	{
		return
			(VolumeSerialNumber == other.VolumeSerialNumber) &&
			(FileIdLow == other.FileIdLow) &&
			(FileIdHigh == other.FileIdHigh);
	}
}

public partial class FileIndexProvider : INodeProvider<FileIndex>
{
	[Flags]
	enum FileAccess
	{
		FILE_READ_ATTRIBUTES = 0x80,
	}

	[Flags]
	enum ShareMode
	{
		FILE_SHARE_READ = 1,
		FILE_SHARE_WRITE = 2,
		FILE_SHARE_DELETE = 4,
	}

	enum CreationDisposition
	{
		OPEN_EXISTING = 3,
	}

	[Flags]
	enum FlagsAndAttributes
	{
		FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
	}

	enum FileInfoByHandleClass
	{
		FileIdInfo = 18,
	}

	[InlineArray(length: 16)]
	struct FILE_ID_128
	{
		byte _element0;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct FILE_ID_INFO
	{
		public ulong VolumeSerialNumber;
		public FILE_ID_128 FileId;
	}

	[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
	static extern SafeFileHandle CreateFileW(string lpFileName, FileAccess dwDesiredAccess, ShareMode dwShareMode,
		IntPtr lpSecurityAttributes, CreationDisposition dwCreationDisposition, FlagsAndAttributes dwFlagsAndAttributes,
		IntPtr hTemplateFile);

	[DllImport("kernel32")]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool GetFileInformationByHandleEx(SafeFileHandle handle,
		FileInfoByHandleClass FileInformationClass, ref FILE_ID_INFO lpFileInformation,
		int dwBufferSize);

	[DllImport("kernel32", SetLastError = true)]
	static extern void CloseHandle(SafeFileHandle hObject);

	public override bool TryGetINode(String path, out FileIndex fileIndex)
	{
		var handle = CreateFileW(
			path,
			FileAccess.FILE_READ_ATTRIBUTES,
			ShareMode.FILE_SHARE_READ | ShareMode.FILE_SHARE_WRITE | ShareMode.FILE_SHARE_DELETE,
			lpSecurityAttributes: IntPtr.Zero,
			CreationDisposition.OPEN_EXISTING,
			FlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS,
			hTemplateFile: IntPtr.Zero);

		fileIndex = new FileIndex();

		if (!handle.IsInvalid)
		{
			try
			{
				var fileIdInfo = new FILE_ID_INFO();

				bool success = GetFileInformationByHandleEx(
					handle,
					FileInfoByHandleClass.FileIdInfo,
					ref fileIdInfo,
					dwBufferSize: 24);

				fileIndex.VolumeSerialNumber = fileIdInfo.VolumeSerialNumber;

				var fileIdWords = MemoryMarshal.Cast<byte, ulong>(fileIdInfo.FileId);

				fileIndex.FileIdLow = fileIdWords[0];
				fileIndex.FileIdHigh = fileIdWords[1];

				return success;
			}
			finally
			{
				CloseHandle(handle);
			}
		}

		return false;
	}
}
