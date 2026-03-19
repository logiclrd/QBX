using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using QBX.Firmware.Fonts;

namespace QBX.OperatingSystem;

public partial class ShortFileNames
{
	// INT 21h and thus the QB code atop it only supports 8.3 path components.
	// On Windows, there's an automatic "short filenames" layer that stores a
	// persistent equivalent short path for every file whose name doesn't
	// already meet the requirements. But, if we're on a system that doesn't
	// have that layer, we're going to need to fake it.

	public void LoadEmulatedMappings(TextReader storage)
	{
		while (true)
		{
			if (!(storage.ReadLine() is string shortPath))
				break;
			if (!(storage.ReadLine() is string longPath))
				break;

			s_longToShort[longPath] = shortPath;
			s_shortToLong[shortPath] = longPath;
		}
	}

	public void SaveEmulatedMappings(TextWriter storage)
	{
		foreach (var mapping in s_shortToLong)
		{
			storage.WriteLine(mapping.Key);
			storage.WriteLine(mapping.Value);
		}
	}

	static Dictionary<string, string> s_longToShort = new Dictionary<string, string>();
	static Dictionary<string, string> s_shortToLong = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	static void ForgetEmulated(string longPath)
	{
		if (s_longToShort.TryGetValue(longPath, out var shortPath))
		{
			s_longToShort.Remove(longPath);
			s_shortToLong.Remove(shortPath);
		}
	}

	static string GetFullPathEmulated(string shortRelativePath)
	{
		if (ShortPath.TryGetDriveLetter(shortRelativePath, out var driveLetter))
		{
			if ((driveLetter == 'C')
			 && ((shortRelativePath.Length < 3) || !ShortPath.DirectorySeparators.Contains(shortRelativePath[2])))
				shortRelativePath = ".\\" + shortRelativePath.Substring(2);
			else
				return GetCanonicalPath(shortRelativePath);
		}

		string longCurrentDirectory = Environment.CurrentDirectory;

		string shortRelativePathNonNormalized;

		if (TryMapEmulated(longCurrentDirectory, out var shortCurrentDirectory))
			shortRelativePathNonNormalized = CombinePath(shortCurrentDirectory, shortRelativePath);
		else
			shortRelativePathNonNormalized = CombinePath(longCurrentDirectory, shortRelativePath);

		return GetCanonicalPath(shortRelativePathNonNormalized);
	}

	static string CombinePath(string basePath, string relativePath)
	{
		if (ShortPath.HasDriveLetter(relativePath))
			return relativePath;

		if ((relativePath.Length > 0) && ShortPath.DirectorySeparators.Contains(relativePath[0]))
		{
			if (ShortPath.TryGetDriveLetter(basePath, out char driveLetter))
				return driveLetter + ":" + relativePath;
			else
				return relativePath;
		}

		return ShortPath.Join(
			basePath.TrimEnd(ShortPath.DirectorySeparators),
			relativePath.TrimStart(ShortPath.DirectorySeparators));
	}

	static readonly string DirectorySeparatorString = Path.DirectorySeparatorChar.ToString();

	static string UnmapEmulated(string possibleShortPath)
	{
		if (string.IsNullOrEmpty(Path.GetPathRoot("C:\\"))
		 && ((possibleShortPath == "C:\\") || (possibleShortPath == "C://")
		  || (possibleShortPath == "c:\\") || (possibleShortPath == "c://")))
			return DirectorySeparatorString;

		possibleShortPath = GetFullPathEmulated(possibleShortPath);

		if (s_shortToLong.TryGetValue(possibleShortPath, out var longPath))
			return longPath;
		else
		{
			var container = ShortPath.GetDirectoryName(possibleShortPath);

			if (!string.IsNullOrEmpty(container))
			{
				string fileName = ShortPath.GetFileName(possibleShortPath);

				string containerLongPath = UnmapEmulated(container);

				longPath = Path.Join(containerLongPath, fileName);

				if ((fileName.Length > 0)
				 && !File.Exists(longPath)
				 && !Directory.Exists(longPath)
				 && Directory.Exists(containerLongPath))
				{
					// The user could be specifying a valid short filename for a path that we've never
					// tried to map before, and this lookup needs to be case-insensitive. The mappings
					// are nontrivial. The only guarantee is that the short filename will start with
					// the same character (modulo case) as the long filename. So, we have to try
					// mapping every file whose first character matches.

					char firstCharacter = fileName[0];

					byte firstByte = CP437Encoding.GetByteSemantic(firstCharacter);
					byte firstByteUpper = ShortPath.ToUpper(firstByte);

					char firstCharacterUpper = CP437Encoding.GetCharSemantic(firstByteUpper);

					foreach (var fileSystemInfo in new DirectoryInfo(containerLongPath).EnumerateFileSystemInfos())
					{
						char ch = fileSystemInfo.Name[0];

						if ((ch == firstCharacter)
						 || (ch == firstCharacterUpper))
						{
							if (TryMapEmulated(fileSystemInfo.FullName, out var newShortPath))
							{
								if (ShortPath.EqualsCaseInsensitive(possibleShortPath, newShortPath))
								{
									longPath = fileSystemInfo.FullName;
									break;
								}
							}
						}
					}
				}

				return longPath;
			}
			else
				return possibleShortPath;
		}
	}

	static bool TryMapEmulated(string path, out string shortPath)
	{
		if (s_longToShort.TryGetValue(Path.GetFullPath(path), out var existingShortPath)
		 && (existingShortPath != null))
		{
			shortPath = existingShortPath;
			return true;
		}

		var builder = new StringBuilder();

		int startIndex = 0;

		if (ShortPath.HasDriveLetter(path)
		 && ((path.Length == 2) || ShortPath.IsDirectorySeparator(path[2])))
		{
			builder.Append(path.AsSpan().Slice(0, 2));

			if (path.Length == 2)
				startIndex = 2;
			else
			{
				startIndex = 4;

				while ((startIndex < path.Length) && ShortPath.IsDirectorySeparator(path[startIndex]))
					startIndex++;
			}
		}
		else
		{
			builder.Append("C:" + ShortPath.DirectorySeparators[0]);

			while ((startIndex < path.Length)
			    && ((path[startIndex] == Path.DirectorySeparatorChar) || (path[startIndex] == Path.AltDirectorySeparatorChar)))
				startIndex++;
		}

		for (int i = startIndex; i < path.Length; i++)
		{
			if (!ShortPath.IsDirectorySeparator(path[i]))
				builder.Append(path[i]);
			else
			{
				if (builder.Length > 0)
					MapLastComponent(builder, longPath: path.Substring(0, i));

				builder.Append(ShortPath.DirectorySeparators[0]);
			}
		}

		MapLastComponent(builder, longPath: path);

		shortPath = builder.ToString();
		return true;
	}

	static bool TryMapEmulated(string path, string shortPath)
	{
		var builder = new StringBuilder();

		int startIndex = 0;

		if ((path.Length >= 2)
		 && char.IsAsciiLetter(path[0])
		 && (path[1] == ShortPath.VolumeSeparatorChar)
		 && ((path.Length == 2) || ShortPath.IsDirectorySeparator(path[2])))
		{
			builder.Append(path.AsSpan().Slice(0, 2));

			if (path.Length == 2)
				startIndex = 2;
			else
			{
				startIndex = 4;

				while ((startIndex < path.Length) && ShortPath.IsDirectorySeparator(path[startIndex]))
					startIndex++;
			}
		}
		else
		{
			builder.Append("C:" + ShortPath.DirectorySeparators[0]);

			while ((startIndex < path.Length)
			    && ((path[startIndex] == Path.DirectorySeparatorChar) || (path[startIndex] == Path.AltDirectorySeparatorChar)))
				startIndex++;
		}

		for (int i = startIndex; i < path.Length; i++)
		{
			if (!ShortPath.IsDirectorySeparator(path[i]))
				builder.Append(path[i]);
			else
			{
				if (builder.Length > 0)
					MapLastComponent(builder, longPath: path.Substring(0, i));

				builder.Append(ShortPath.DirectorySeparators[0]);
			}
		}

		MapLastComponent(builder, longPath: path, shortName: shortPath);

		shortPath = builder.ToString();
		return true;
	}

	static void MapLastComponent(StringBuilder builder, string longPath, string? shortName = null)
	{
		int componentLength = 0;

		while ((componentLength < builder.Length
		    && !ShortPath.IsDirectorySeparator(builder[builder.Length - componentLength - 1])))
			componentLength++;

		if (componentLength > 0)
		{
			string component = shortName != null
				? shortName
				: builder.ToString(builder.Length - componentLength, componentLength);

			string normalizedComponent = component;

			bool componentIsValidShortPathComponent = (shortName == null) ? IsValidShortPathComponent(ref normalizedComponent) : false;

			if (!componentIsValidShortPathComponent
			 || (shortName != null)
			 || (normalizedComponent != component))
			{
				int containerLength = builder.Length - componentLength;

				while ((containerLength > 0) && ShortPath.IsDirectorySeparator(builder[containerLength - 1]))
					containerLength--;

				string containerShortPath = "";
				string containerLongPath = "";

				if (containerLength > 0)
				{
					containerShortPath = builder.ToString(0, containerLength);

					if (s_shortToLong.TryGetValue(containerShortPath, out var existingContainerLongPath))
						containerLongPath = existingContainerLongPath;
					else
						containerLongPath = containerShortPath;
				}

				var longName = component;

				longPath ??= Path.Join(containerLongPath, longName);

				string shortPath;

				if (componentIsValidShortPathComponent)
				{
					shortName = normalizedComponent;

					shortPath = ShortPath.Join(containerShortPath, shortName);
					shortPath = GetCanonicalPath(shortPath);
				}
				else
				{
					shortName = "";
					shortPath = "";

					bool createMapping = false;

					if (normalizedComponent == component)
						createMapping = true;
					else
					{
						shortName = normalizedComponent;
						shortPath = ShortPath.Join(containerShortPath, shortName);

						if (Path.Exists(shortPath))
							createMapping = true;
					}

					if (createMapping)
					{
						CreateMapping(longName, containerShortPath, out shortName);

						shortPath = ShortPath.Join(containerShortPath, shortName);
					}

					shortPath = GetCanonicalPath(shortPath);
				}

				s_longToShort[longPath] = shortPath;
				s_shortToLong[shortPath] = longPath;

				builder.Remove(builder.Length - componentLength, componentLength);
				builder.Append(shortName);
			}
		}
	}

	static void CreateMapping(string longName, string containerShortPath, out string shortName)
	{
		int dot = longName.LastIndexOf('.');

		string namePart, extensionPart;

		if (dot >= 0)
		{
			var partBuilder = new StringBuilder(capacity: 8);

			string GetPart(int offset, int length, int maxLength, bool leadingDot)
			{
				partBuilder.Length = 0;

				if (length > maxLength)
					length = maxLength;

				for (int i=0; i < length; i++)
				{
					char ch = longName[offset + i];

					if ((ch != ' ') && ((ch != '.') || leadingDot))
					{
						leadingDot = false;

						partBuilder.Append(ch);
						if (partBuilder.Length == partBuilder.Capacity)
							break;
					}
				}

				return partBuilder.ToString();
			}

			int extensionLength = longName.Length - dot;

			if (extensionLength > 4) // ".EXT"
				extensionLength = 4;

			namePart = GetPart(0, 8, dot, leadingDot: false);
			extensionPart = GetPart(dot, 4, extensionLength, leadingDot: true);
		}
		else
		{
			namePart = longName;
			extensionPart = "";
		}

		namePart = namePart.ToUpperInvariant();
		extensionPart = extensionPart.ToUpperInvariant();

		// Will map up to four with FIRSTS~n.EXT, as long as we have at least 3 characters.
		if (namePart.Length >= 3)
		{
			if (TryFindFreeMapping(namePart, maximumIndex: 4, extensionPart, containerShortPath, out shortName))
				return;
		}

		// When more than four are present, switch to checksum strategy.
		string checksumBasedPrefix = GetNameChecksumPrefix(longName);

		if (!TryFindFreeMapping(checksumBasedPrefix, maximumIndex: int.MaxValue, extensionPart, containerShortPath, out shortName))
			throw new Exception("Sanity failure");
	}

	const int ERROR_FILE_SYSTEM_LIMITATION = 0x299;

	static bool TryFindFreeMapping(string prefix, int maximumIndex, string extension, string containerShortPath, out string shortName)
	{
		for (int i = 1; i <= maximumIndex; i++)
		{
			string indexChars = "~" + i;

			if (prefix.Length + indexChars.Length > 8)
			{
				if (indexChars.Length >= 8)
					throw new Win32Exception(ERROR_FILE_SYSTEM_LIMITATION);

				prefix = prefix.Substring(0, 8 - indexChars.Length);
			}

			string candidate = prefix + indexChars + extension;

			string candidatePath = ShortPath.Join(containerShortPath, candidate);

			if (!s_shortToLong.ContainsKey(candidatePath))
			{
				shortName = candidate;
				return true;
			}
		}

		shortName = "";
		return false;
	}

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	static bool IsValidShortPathComponent(ref string shortPath)
	{
		if (shortPath.Length > 12)
			return false;

		int firstDot = shortPath.IndexOf('.');
		int lastDot = shortPath.IndexOf('.');

		if (firstDot != lastDot)
			return false;

		int dot = lastDot;

		int nameLength, extensionLength;

		if (dot >= 0)
		{
			nameLength = dot - 1;
			extensionLength = shortPath.Length - dot - 1;
		}
		else
		{
			nameLength = shortPath.Length;
			extensionLength = 0;
		}

		if ((nameLength > 8) || (extensionLength > 3))
			return false;

		var shortPathASCII = s_cp437.GetBytes(shortPath);

		for (int i = 0; i < shortPathASCII.Length; i++)
		{
			byte byteValue = shortPathASCII[i];

			if (!ShortPath.IsValid(byteValue) && (byteValue != '.'))
				return false;

			shortPathASCII[i] = ShortPath.ToUpper(byteValue);
		}

		shortPath = s_cp437.GetString(shortPathASCII);

		return true;
	}

	static string GetNameChecksumPrefix(string longName)
	{
		string checksum = GetNameChecksum(longName);

		string longNameWithoutExtension = Path.GetFileNameWithoutExtension(longName);

		if (string.IsNullOrWhiteSpace(longNameWithoutExtension))
			longNameWithoutExtension = longName;

		string bareCharacters = longName.Replace(".", "");

		if (bareCharacters.Length > 2)
			bareCharacters = bareCharacters.Substring(0, 2);

		return bareCharacters.ToUpperInvariant() + checksum;
	}

	// Reverse-engineered by Thomas Galvin and described at
	// http://usn.pw/blog/gen/2015/06/09/filenames/, which
	// no longer exists but is indexed in the Wayback Machine.
	static string GetNameChecksum(string longName)
	{
		unchecked
		{
			ushort checksum = 0;

			for (int i = 0; i < longName.Length; i++)
				checksum = (ushort)(checksum * 0x25 + longName[i]);

			int temp = checksum * 314159269;

			if (temp < 0)
				temp = -temp;

			temp = (int)((ulong)temp - ((ulong)(temp * 1152921497L) >> 60) * 1000000007UL);
			checksum = (ushort)temp;

			// reverse nibble order
			checksum = (ushort)(
					((checksum & 0xf000) >> 12) |
					((checksum & 0x0f00) >> 4) |
					((checksum & 0x00f0) << 4) |
					((checksum & 0x000f) << 12));

			return checksum.ToString("X4");
		}
	}

	static string GetCanonicalPath(string path)
	{
		if (path.Length == 0)
			return path;

		bool prependRoot = ShortPath.DirectorySeparators.Contains(path[0]);

		var components = path.Split(ShortPath.DirectorySeparators, StringSplitOptions.RemoveEmptyEntries).ToList();

		for (int i=0; i < components.Count; i++)
		{
			if (components[i] == ".")
				components.RemoveAt(i);
			else if (components[i] == "..")
			{
				if ((i > 0) && !ShortPath.IsDriveLetter(components[i - 1]))
				{
					components.RemoveRange(i - 1, 2);
					i--;
				}
				else
					components.RemoveAt(i);
			}
		}

		if ((components.Count > 0)
		 && ShortPath.HasDriveLetter(components[0]))
			prependRoot = false;

		path = ShortPath.Join(CollectionsMarshal.AsSpan(components));

		if (prependRoot)
			path = ShortPath.DirectorySeparators[0] + path;

		return path;
	}
}
