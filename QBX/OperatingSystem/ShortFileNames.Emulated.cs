using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
		string longCurrentDirectory = Environment.CurrentDirectory;

		string shortRelativePathNonNormalized;

		if (TryMapEmulated(longCurrentDirectory, out var shortCurrentDirectory))
			shortRelativePathNonNormalized = Path.Combine(shortCurrentDirectory, shortRelativePath);
		else
			shortRelativePathNonNormalized = Path.Combine(longCurrentDirectory, shortRelativePath);

		return Path.GetFullPath(shortRelativePathNonNormalized); // handles '.' and '..' components, does not require that path exist
	}

	static readonly string DirectorySeparatorString = Path.DirectorySeparatorChar.ToString();

	static string UnmapEmulated(string possibleShortPath)
	{
		if ((Path.GetPathRoot("C:\\") is null) && ((possibleShortPath == "C:\\" || possibleShortPath == "C://")))
			return DirectorySeparatorString;

		possibleShortPath = GetFullPathEmulated(possibleShortPath);

		if (s_shortToLong.TryGetValue(GetCanonicalPath(possibleShortPath), out var longPath))
			return longPath;
		else
		{
			var container = Path.GetDirectoryName(possibleShortPath);

			if (!string.IsNullOrEmpty(container))
				return Path.Combine(UnmapEmulated(container), Path.GetFileName(possibleShortPath));
			else
				return possibleShortPath;
		}
	}

	static bool TryMapEmulated(string path, out string shortPath)
	{
		if (s_longToShort.TryGetValue(GetCanonicalPath(path), out var existingShortPath)
		 && (existingShortPath != null))
		{
			shortPath = existingShortPath;
			return true;
		}

		var builder = new StringBuilder();

		int startIndex = 0;

		if ((Path.GetPathRoot(path) is string root) && (root.Length >= 2) && (root[1] == Path.VolumeSeparatorChar))
		{
			builder.Append(root);
			startIndex = root.Length;
		}
		else
		{
			builder.Append("C:" + Path.DirectorySeparatorChar);

			while ((startIndex < path.Length)
			    && ((path[startIndex] == Path.DirectorySeparatorChar) || (path[startIndex] == Path.AltDirectorySeparatorChar)))
				startIndex++;
		}

		for (int i = startIndex; i < path.Length; i++)
		{
			if (path[i] == Path.DirectorySeparatorChar)
			{
				if (builder.Length > 0)
					MapLastComponent(builder);
			}

			builder.Append(path[i]);
		}

		MapLastComponent(builder);

		shortPath = builder.ToString();
		return true;
	}

	static bool TryMapEmulated(string path, string shortPath)
	{
		var builder = new StringBuilder();

		int startIndex = 0;

		if ((Path.GetPathRoot(path) is string root) && (root.Length >= 2) && (root[1] == Path.VolumeSeparatorChar))
		{
			builder.Append(root);
			startIndex = root.Length;
		}
		else
		{
			builder.Append("C:" + Path.DirectorySeparatorChar);

			while ((startIndex < path.Length)
			    && ((path[startIndex] == Path.DirectorySeparatorChar) || (path[startIndex] == Path.AltDirectorySeparatorChar)))
				startIndex++;
		}

		for (int i = startIndex; i < path.Length; i++)
		{
			if (path[i] == Path.VolumeSeparatorChar)
			{
				if (builder.Length > 0)
					MapLastComponent(builder);
			}

			builder.Append(path[i]);
		}

		MapLastComponent(builder, shortPath);

		shortPath = builder.ToString();
		return true;
	}

	static void MapLastComponent(StringBuilder builder)
	{
		int componentLength = 0;

		while ((componentLength < builder.Length)
		    && (builder[builder.Length - componentLength - 1] != Path.DirectorySeparatorChar))
			componentLength++;

		if (componentLength > 0)
		{
			string component = builder.ToString(builder.Length - componentLength, componentLength);

			string normalizedComponent = component;

			if (!IsValidShortPathComponent(ref normalizedComponent)
			 || (normalizedComponent != component))
			{
				int containerLength = builder.Length - componentLength;

				while ((containerLength > 0) && (builder[containerLength - 1] == Path.VolumeSeparatorChar))
					containerLength--;

				if (containerLength > 0)
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

				var longPath = Path.Combine(containerLongPath, longName);

				string shortName = "";
				string shortPath = "";

				bool createMapping = false;

				if (normalizedComponent == component)
					createMapping = true;
				else
				{
					shortName = normalizedComponent;
					shortPath = Path.Combine(containerShortPath, shortName);

					if (Path.Exists(shortPath))
						createMapping = true;
				}

				if (createMapping)
				{
					CreateMapping(longPath, containerShortPath, out shortName);

					shortPath = Path.Combine(containerShortPath, shortName);
				}

				shortPath = GetCanonicalPath(shortPath);

				s_longToShort[longPath] = shortPath;
				s_shortToLong[shortPath] = longPath;

				builder.Remove(builder.Length - componentLength, componentLength);
				builder.Append(shortName);
			}
		}
	}

	static void MapLastComponent(StringBuilder builder, string shortName)
	{
		int componentLength = 0;

		while ((componentLength < builder.Length)
		    && (builder[builder.Length - componentLength - 1] != Path.VolumeSeparatorChar))
			componentLength++;

		if (componentLength > 0)
		{
			string component = builder.ToString(builder.Length - componentLength, componentLength);

			int containerLength = builder.Length - componentLength;

			while ((containerLength > 0) && (builder[containerLength - 1] == Path.VolumeSeparatorChar))
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

			var longPath = containerLongPath + Path.VolumeSeparatorChar + longName;
			var shortPath = containerShortPath + Path.VolumeSeparatorChar + shortName;

			shortPath = GetCanonicalPath(shortPath);

			if (s_shortToLong.ContainsKey(shortPath))
				throw new DOSException(DOSError.FileExists);

			s_longToShort[longPath] = shortPath;
			s_shortToLong[shortPath] = longPath;

			builder.Remove(builder.Length - componentLength, componentLength);
			builder.Append(shortName);
		}
	}

	static void CreateMapping(string longName, string containerShortPath, out string shortName)
	{
		int dot = longName.LastIndexOf('.');

		string namePart, extensionPart;

		if (dot >= 0)
		{
			namePart = longName.Substring(0, dot).Replace(".", "");
			extensionPart = longName.Substring(dot);
		}
		else
		{
			namePart = longName;
			extensionPart = "";
		}

		namePart = namePart.ToUpperInvariant();

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

				prefix = prefix.Substring(8 - indexChars.Length);
			}

			string candidate = prefix + indexChars + extension;

			string candidatePath = containerShortPath + Path.VolumeSeparatorChar + candidate;

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

			if (!PathCharacter.IsValid(byteValue) && (byteValue != '.'))
				return false;

			shortPathASCII[i] = PathCharacter.ToUpper(byteValue);
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
		var components = path.Split(Path.VolumeSeparatorChar, StringSplitOptions.RemoveEmptyEntries);

		if (!Path.IsPathRooted(path))
			return Path.VolumeSeparatorChar + string.Join(Path.VolumeSeparatorChar, components);
		else
			return string.Join(Path.VolumeSeparatorChar, components);
	}
}
