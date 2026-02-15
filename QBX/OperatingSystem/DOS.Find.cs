using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;

using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.OperatingSystem.FileStructures;

using FileAttributes = QBX.OperatingSystem.FileStructures.FileAttributes;

namespace QBX.OperatingSystem;

public partial class DOS
{
	// DOS would store FCB-based searches in the FCB itself. We can't do that; there's
	// no way to represent a pointer from the SystemMemory abstraction back to the CLR
	// world, and even if there were, there aren't enough bits to space in the FCB
	// structure for a 64-bit pointer. So, the best we can do is to keep searches
	// active independently with a way to match them up. Since there's no way to know
	// if the emulated code abandons a search, we place an upper limit on how many
	// we'll remember. We'll probably never hit it, but just in case.

	class ActiveSearch(int searchID, IEnumerator<FileSystemInfo> search, FileAttributes searchAttributes, ReadOnlySpan<byte> searchPatternBytes)
	{
		public readonly int SearchID = searchID;
		public readonly IEnumerator<FileSystemInfo> Search = search;
		public readonly LinkedListNode<int> SearchSequence = new LinkedListNode<int>(searchID);
		public readonly FileAttributes SearchAttributes = searchAttributes;
		public readonly byte[] SearchPattern = searchPatternBytes.Slice(0, 11).ToArray();
	}

	int _nextSearchID = 1;
	Dictionary<int, ActiveSearch> _activeSearches = new();
	LinkedList<int> _activeSearchRecency = new();

	const int MaxActiveSearches = 128;

	void AddActiveSearch(int searchID, IEnumerator<FileSystemInfo> search, FileAttributes searchAttributes, ReadOnlySpan<byte> searchPatternBytes)
	{
		while (_activeSearches.Count > MaxActiveSearches)
			DiscardOldestSearch();

		var activeSearch = new ActiveSearch(searchID, search, searchAttributes, searchPatternBytes);

		_activeSearches[searchID] = activeSearch;
		_activeSearchRecency.AddLast(activeSearch.SearchSequence);
	}

	void DiscardOldestSearch()
	{
		if (_activeSearchRecency.First is LinkedListNode<int> firstSequence)
		{
			int oldestSearchID = firstSequence.Value;

			_activeSearches.Remove(oldestSearchID);
			_activeSearchRecency.RemoveFirst();
		}
	}

	void RecordSearchActivity(ActiveSearch search)
	{
		_activeSearchRecency.Remove(search.SearchSequence);
		_activeSearchRecency.AddLast(search.SearchSequence);
	}

	void RemoveFinishedSearch(ActiveSearch search)
	{
		_activeSearches.Remove(search.SearchID);
		_activeSearchRecency.Remove(search.SearchSequence);
	}

	string NormalizeFileSearchPattern(ref string part, int partLength)
	{
		if (part.Length > partLength)
			part = part.Substring(0, partLength);

		int asterisk = part.IndexOf('*');

		if (asterisk >= 0)
			part = part.Substring(0, asterisk) + new string('?', partLength - asterisk);

		string collapsed = part;

		if ((collapsed.Length == partLength) && (collapsed[collapsed.Length - 1] == '?'))
			collapsed = collapsed.TrimEnd('?') + '*';

		return collapsed;
	}

	void FormatName8_3(string name, BinaryWriter writer)
	{
		int dot = name.IndexOf('.');

		if (dot < 0)
		{
			for (int i = 0; i < 8; i++)
				writer.Write((i < name.Length) ? CP437Encoding.GetByteSemantic(name[i]) : (byte)0);
			for (int i = 8; i < 11; i++)
				writer.Write((byte)0);
		}
		else
		{
			int extensionStart = dot + 1;

			for (int i = 0; i < 8; i++)
				writer.Write((i < dot) ? CP437Encoding.GetByteSemantic(name[i]) : (byte)0);
			for (int i = 0; i < 3; i++)
				writer.Write((extensionStart + i < name.Length) ? CP437Encoding.GetByteSemantic(name[extensionStart + i]) : (byte)0);
		}
	}

	void FormatAsDirEntry(FileSystemInfo info, string shortName, BinaryWriter writer)
	{
		var timestamp = info.LastWriteTime;

		FormatName8_3(shortName, writer);
		writer.Write(info.Attributes.ToDOSFileAttributesByte());
		writer.Write(stackalloc byte[10]);
		writer.Write(new FileTime().Set(timestamp).Raw);
		writer.Write(new FileDate().Set(timestamp).Raw);
		writer.Write((ushort)0xFFFF); // starting cluster

		if (info is FileInfo fileInfo)
			writer.Write((int)fileInfo.Length);
		else
			writer.Write((int)0);
	}

	delegate void FindResultFormatter(FileSystemInfo fileInfo, string shortName, FileAttributes searchAttributes, ReadOnlySpan<byte> searchPattern, int searchID);

	void FormatFindResultAsDirEntry(FileSystemInfo info, string shortName, FileAttributes searchAttributes, ReadOnlySpan<byte> searchPattern, int searchID)
	{
		// DIRENTRY    STRUC
		//     deName         db '????????'   ;name
		//     deExtension    db '???'        ;extension
		//     deAttributes   db ?            ;attributes
		//     deReserved     db 10 dup(?)    ;reserved
		//     deTime         dw ?            ;time
		//     deDate         dw ?            ;date
		//     deStartCluster dw ?            ;starting cluster
		//     deFileSize     dd ?            ;file size
		// DIRENTRY    ENDS

		var stream = new SystemMemoryStream(Machine.MemoryBus, DataTransferAddress, 32);
		var writer = new BinaryWriter(stream);

		FormatAsDirEntry(info, shortName, writer);
	}

	void FormatFindResultAsDirEntryEx(FileSystemInfo info, string shortName, FileAttributes searchAttributes, ReadOnlySpan<byte> searchPattern, int searchID)
	{
		// EXTHEADER STRUC
		//     ehSignature     db 0ffh         ;extended signature
		//     ehReserved      db 5 dup(0)     ;reserved
		//     ehSearchAttrs   db ?            ;attribute byte
		// EXTHEADER ENDS
		// ;followed by DIRENTRY

		var stream = new SystemMemoryStream(Machine.MemoryBus, DataTransferAddress, 7 + 32);
		var writer = new BinaryWriter(stream);

		writer.Write((byte)0xFF);
		writer.Write(stackalloc byte[5]);
		writer.Write(unchecked((byte)searchAttributes));

		FormatAsDirEntry(info, shortName, writer);
	}

	void FormatFindResultAsFileInfo(FileSystemInfo info, string shortName, FileAttributes searchAttributes, ReadOnlySpan<byte> searchPattern, int searchID)
	{
		var dosFileInfo = new DOSFileInfo();

		dosFileInfo.Reserved_SearchAttributes = searchAttributes;
		searchPattern.Slice(0, Math.Min(11, searchPattern.Length)).CopyTo(dosFileInfo.Reserved_SearchPattern);
		dosFileInfo.Reserved_SearchID = searchID;

		var timestamp = info.LastWriteTime;

		dosFileInfo.Attributes = info.Attributes.ToDOSFileAttributes();
		dosFileInfo.FileTime.Set(timestamp);
		dosFileInfo.FileDate.Set(timestamp);

		if (info is FileInfo fileInfo)
			dosFileInfo.Size = (uint)fileInfo.Length;

		dosFileInfo.FileName.Set(shortName);

		dosFileInfo.Serialize(Machine.MemoryBus, DataTransferAddress);
	}

	public bool FindFirst(FileControlBlock fcb)
	{
		string fileNamePattern = fcb.GetFileName();

		string fileNamePart = Path.GetFileNameWithoutExtension(fileNamePattern);
		string extensionPart = Path.GetExtension(fileNamePattern);

		string collapsedFileNamePart = NormalizeFileSearchPattern(ref fileNamePart, 8);
		string collapsedExtensionPart = NormalizeFileSearchPattern(ref extensionPart, 3);

		fileNamePattern = fileNamePart + "." + extensionPart;

		fcb.SetFileName(fileNamePattern);

		string collapsedFileNamePattern = collapsedFileNamePart + "." + collapsedExtensionPart;
		string rawFileNamePattern = collapsedFileNamePart + collapsedExtensionPart;

		bool success;
		IEnumerator<FileSystemInfo> search;
		FileAttributes searchAttributes = default;
		byte[] searchPatternBytes = s_cp437.GetBytes(rawFileNamePattern);

		int searchID = _nextSearchID;

		if (fcb is ExtendedFileControlBlock fcbEx)
		{
			searchAttributes = fcbEx.Attributes;
			success = FindFirst(collapsedFileNamePattern, searchAttributes, searchPatternBytes, searchID, FormatFindResultAsDirEntryEx, out search);
		}
		else
			success = FindFirst(collapsedFileNamePattern, default, searchPatternBytes, searchID, FormatFindResultAsDirEntry, out search);

		if (success)
		{
			_nextSearchID++;

			fcb.SearchID = searchID;

			AddActiveSearch(fcb.SearchID, search, searchAttributes, searchPatternBytes);
		}

		return success;
	}

	ActiveSearch? _activeSearch = null;

	public bool FindFirstCentralized(string fileNamePattern, FileAttributes attributes)
	{
		return TranslateError(() =>
		{
			string fileNamePart = Path.GetFileNameWithoutExtension(fileNamePattern);
			string extensionPart = Path.GetExtension(fileNamePattern);

			string collapsedFileNamePart = NormalizeFileSearchPattern(ref fileNamePart, 8);
			string collapsedExtensionPart = NormalizeFileSearchPattern(ref extensionPart, 3);

			string collapsedFileNamePattern = collapsedFileNamePart + "." + collapsedExtensionPart;
			string rawFileNamePattern = collapsedFileNamePart + collapsedExtensionPart;

			byte[] searchPatternBytes = s_cp437.GetBytes(rawFileNamePattern);

			int searchID = _nextSearchID;

			var result = FindFirst(collapsedFileNamePattern, attributes, searchPatternBytes, searchID, FormatFindResultAsFileInfo, out var search);

			if (result)
			{
				_nextSearchID++;

				_activeSearch = new ActiveSearch(searchID, search, attributes, searchPatternBytes);
			}
			else
			{
				_activeSearch = null;
				LastError = DOSError.FileNotFound;
			}

			return result;
		});
	}

	readonly IEnumerable<FileSystemInfo> _dummyArray = Array.Empty<FileSystemInfo>();

	IEnumerator<FileSystemInfo> DummySearch() => _dummyArray.GetEnumerator();

	class VolumeLabelFileSystemInfo(DriveInfo driveInfo) : FileSystemInfo
	{
		public override bool Exists => true;

		public override string Name => driveInfo.VolumeLabel;

		public override void Delete() { }
	}

	bool FindFirst(string fileNamePattern, FileAttributes attributes, ReadOnlySpan<byte> searchPattern, int searchID, FindResultFormatter formatResult, out IEnumerator<FileSystemInfo> search)
	{
		string containerPath = Path.GetDirectoryName(fileNamePattern) ?? ".";

		fileNamePattern = Path.GetFileName(fileNamePattern);

		var directoryInfo = new DirectoryInfo(Environment.CurrentDirectory);

		if (!directoryInfo.Exists)
		{
			LastError = DOSError.PathNotFound;
			search = DummySearch();
			return false;
		}

		if ((attributes & FileAttributes.Volume) != 0)
		{
			// TODO: if in the root, return a result with just the volume label, and only if it matches the pattern
			if (directoryInfo.FullName == directoryInfo.Root.FullName)
			{
				var driveInfo = new DriveInfo(directoryInfo.FullName);

				if (!string.IsNullOrWhiteSpace(driveInfo.VolumeLabel)
				 && FileSystemName.MatchesSimpleExpression(fileNamePattern, driveInfo.VolumeLabel))
				{
					IEnumerable<FileSystemInfo> enumerable = [new VolumeLabelFileSystemInfo(driveInfo)];

					search = enumerable.GetEnumerator();
					return true;
				}
			}

			LastError = DOSError.FileNotFound;
			search = DummySearch();
			return false;
		}
		else
		{
			var searchPrototype = directoryInfo.EnumerateFileSystemInfos(fileNamePattern);

			if ((attributes & FileAttributes.Directory) == 0)
				searchPrototype = searchPrototype.OfType<FileInfo>();
			if ((attributes & FileAttributes.Hidden) == 0)
				searchPrototype = searchPrototype.Where(entry => ((entry.Attributes & System.IO.FileAttributes.ReadOnly) == 0));

			search = searchPrototype.GetEnumerator();

			return FindNext(search, attributes, searchPattern, searchID, formatResult);
		}
	}

	public bool FindNext(FileControlBlock fcb)
	{
		return TranslateError(() =>
		{
			if (!_activeSearches.TryGetValue(fcb.SearchID, out var activeSearch))
				return false;

			bool result;

			if (fcb is ExtendedFileControlBlock fcbEx)
				result = FindNext(activeSearch.Search, activeSearch.SearchAttributes, activeSearch.SearchPattern, activeSearch.SearchID, FormatFindResultAsDirEntryEx);
			else
				result = FindNext(activeSearch.Search, activeSearch.SearchAttributes, activeSearch.SearchPattern, activeSearch.SearchID, FormatFindResultAsDirEntry);

			if (result)
				RecordSearchActivity(activeSearch);
			else
				RemoveFinishedSearch(activeSearch);

			return result;
		});
	}

	public bool FindNextCentralized()
	{
		return TranslateError(() =>
		{
			if (_activeSearch == null)
			{
				LastError = DOSError.NoMoreFiles;
				return false;
			}

			bool result = FindNext(
				_activeSearch.Search,
				_activeSearch.SearchAttributes, _activeSearch.SearchPattern, _activeSearch.SearchID,
				FormatFindResultAsFileInfo);

			if (!result)
				_activeSearch = null;

			return result;
		});
	}

	bool FindNext(IEnumerator<FileSystemInfo> search, FileAttributes searchAttributes, ReadOnlySpan<byte> searchPattern, int searchID, FindResultFormatter formatResult)
	{
		string shortPath;

		do
		{
			if (!search.MoveNext())
			{
				LastError = DOSError.NoMoreFiles;
				return false;
			}
		} while (!ShortFileNames.TryMap(search.Current.FullName, out shortPath));

		formatResult(
			search.Current, Path.GetFileName(shortPath),
			searchAttributes, searchPattern, searchID);

		return true;
	}
}
