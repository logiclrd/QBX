using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QBX.Tests.Utility.Interop;

public class INode : INode<INode>
{
	public UInt64 DeviceID;
	public UInt64 INodeNumber;

	public override bool IsSameVolumeAndFileAs(INode other)
	{
		return
			(DeviceID == other.DeviceID) &&
			(INodeNumber == other.INodeNumber);
	}
}

public class LinuxINodeProvider : INodeProvider<INode>
{
	[StructLayout(LayoutKind.Sequential)]
	struct timespec
	{
		public long tv_sec;
		public long tv_nsec;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct stat_
	{
		public ulong st_dev;    /* Device ID */
		public ulong st_ino;    /* Inode number */
		public ulong st_nlink;  /* Number of hard links */
		public uint st_mode;    /* File type and permissions */
		public uint st_uid;    /* User ID of owner */
		public uint st_gid;    /* Group ID of owner */
		public uint __pad0;
		public ulong st_rdev;   /* Device ID (if special file) */
		public long st_size;   /* Total size, in bytes */
		public long st_blksize; /* Block size for filesystem I/O */
		public long st_blocks;  /* Number of 512B blocks allocated */
		public timespec st_atime;  /* Time of last access (seconds) */
		public timespec st_mtime;  /* Time of last modification (seconds) */
		public timespec st_ctime;  /* Time of last status change (seconds) */
	}

	// Import the stat function from the C standard library (libc)
	[DllImport("c", SetLastError = true, CharSet = CharSet.Ansi)]
	static extern int stat(string path, out stat_ buf);

	public override bool TryGetINode(string path, out INode inode)
	{
		inode = new INode();

		if (stat(path, out var stat_structure) == 0)
		{
			inode.DeviceID = stat_structure.st_dev;
			inode.INodeNumber = stat_structure.st_ino;
		}

		return false;
	}
}

public class FreeBSDINodeProvider : INodeProvider<INode>
{
	[StructLayout(LayoutKind.Sequential)]
	struct timespec
	{
		public long tv_sec;
		public long tv_nsec;
	}

	[InlineArray(length: 10)]
	struct stat_spare
	{
		ulong _element0;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct stat_
	{
		public ulong st_dev;
		public ulong st_ino;
		public ulong st_nlink;
		public ushort st_mode;
		public ushort st_padding0;
		public uint st_uid;
		public uint st_gid;
		public uint st_padding1;
		public ulong st_rdev;
		public timespec st_atim;
		public timespec st_mtim;
		public timespec st_ctim;
		public timespec st_birthtim;
		public ulong st_size;
		public ulong st_blocks;
		public uint st_blksize;
		public uint st_flags;
		public ulong st_gen;
		public stat_spare st_spare;
	}

	[DllImport("c", SetLastError = true, CharSet = CharSet.Ansi)]
	static extern int stat(string path, out stat_ buf);

	public override bool TryGetINode(string path, out INode inode)
	{
		inode = new INode();

		if (stat(path, out var stat_structure) == 0)
		{
			inode.DeviceID = stat_structure.st_dev;
			inode.INodeNumber = stat_structure.st_ino;
		}

		return false;
	}
}

public class OSXINodeProvider : INodeProvider<INode>
{
	[StructLayout(LayoutKind.Sequential)]
	struct timespec
	{
		public long tv_sec;
		public long tv_nsec;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct stat_
	{
		public uint st_dev;
		public ushort st_mode;
		public ushort st_nlink;
		public ulong st_ino; // 64-bit inode
		public uint st_uid;
		public uint st_gid;
		public uint st_rdev;
		public timespec st_atimespec;
		public timespec st_mtimespec;
		public timespec st_ctimespec;
		public ulong st_size;
		public ulong st_blocks;
		public uint st_blksize;
		public uint st_flags;
		public uint st_gen;
		public uint st_lspare;
		public long st_qspare1;
		public long st_qspare2;
	}

	[DllImport("c", SetLastError = true, CharSet = CharSet.Ansi)]
	static extern int stat(string path, out stat_ buf);

	public override bool TryGetINode(string path, out INode inode)
	{
		inode = new INode();

		if (stat(path, out var stat_structure) == 0)
		{
			inode.DeviceID = stat_structure.st_dev;
			inode.INodeNumber = stat_structure.st_ino;
		}

		return false;
	}
}
