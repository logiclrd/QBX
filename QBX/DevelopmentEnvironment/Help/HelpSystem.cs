using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace QBX.DevelopmentEnvironment.Help;

public class HelpSystem
{
	Dictionary<string, HelpDatabase> _helpFiles = new Dictionary<string, HelpDatabase>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, HelpDatabaseTopic> _contextStrings = new Dictionary<string, HelpDatabaseTopic>();

	bool _haveCaseInsensitiveContextStrings = false;
	bool _haveCaseSensitiveContextStrings = false;

	Configuration _configuration;

	public HelpSystem(Configuration configuration)
	{
		_configuration = configuration;
	}

	public bool ProbeForFile(string fileName)
	{
		if (!string.IsNullOrWhiteSpace(_configuration.HelpFileSearchPath))
		{
			return ProbeForFileAtPath(Path.Combine(_configuration.HelpFileSearchPath, fileName));
		}
		else
		{
			return
				ProbeForFileAtPath(fileName) ||
				ProbeForFileAtPath(Path.Join("../HELP", fileName));
		}
	}

	public bool ProbeForFileAtPath(string path)
	{
		try
		{
			// Embedded paths could conceivably contain DOS directory separator characters.
			path = path.Replace('\\', '/');

			// Need to process the filename case-insensitively even if the underlying platform is case-sensitive.
			string containerPath = Path.GetDirectoryName(path) ?? ".";
			string requestedFileName = Path.GetFileName(path);

			if (!Directory.Exists(containerPath))
				return false;

			if (string.IsNullOrWhiteSpace(containerPath))
				containerPath = ".";

			string[] files = Directory.GetFiles(containerPath, "*");

			var matchingFile = Array.Find(
				files,
				name => string.Equals(Path.GetFileName(name), requestedFileName, StringComparison.InvariantCultureIgnoreCase));

			if ((matchingFile != null) && File.Exists(matchingFile))
			{
				LoadFile(matchingFile);
				return true;
			}
		}
		catch { }

		return false;
	}

	public void LoadFile(string path)
	{
		using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			LoadFile(stream);
	}

	public void LoadFile(Stream stream)
	{
		foreach (var database in HelpFileLoader.LoadDatabases(stream))
			AddHelpDatabase(database);
	}

	public void AddHelpDatabase(HelpDatabase helpFile)
	{
		_helpFiles.Add(helpFile.DatabaseName, helpFile);

		if (helpFile.CaseSensitiveContextStrings)
		{
			_haveCaseSensitiveContextStrings = true;

			foreach (var contextString in helpFile.GlobalContextStrings)
				_contextStrings[contextString] = helpFile.TopicByContextString[contextString];
		}
		else
		{
			_haveCaseInsensitiveContextStrings = true;

			foreach (var contextString in helpFile.GlobalContextStrings)
				_contextStrings[contextString.ToLowerInvariant()] = helpFile.TopicByContextString[contextString];
		}
	}

	public bool TryGetTopic(HelpDatabase? currentDatabase, string contextString, [NotNullWhen(true)] out HelpDatabaseTopic? topic)
	{
		int separator = contextString.IndexOf('!');

		if ((separator > 0) && (separator + 1 < contextString.Length))
		{
			// Search specific help file
			string databaseName = contextString.Substring(0, separator);

			contextString = contextString.Substring(separator + 1);

			if (!_helpFiles.TryGetValue(databaseName, out var helpFile))
			{
				ProbeForFile(databaseName);

				if (!_helpFiles.TryGetValue(databaseName, out helpFile))
				{
					topic = null;
					return false;
				}
			}

			return helpFile.TryGetGlobalTopic(contextString, out topic);
		}
		else
		{
			if (currentDatabase != null)
			{
				// Search the local database, including local context strings.
				if (currentDatabase.TryGetTopic(contextString, out topic))
					return true;
			}

			if (!contextString.StartsWith('@'))
			{
				// Search all indexed help files
				if (_haveCaseInsensitiveContextStrings
				 && _contextStrings.TryGetValue(contextString.ToLowerInvariant(), out topic))
					return true;

				if (_haveCaseSensitiveContextStrings
				 && _contextStrings.TryGetValue(contextString, out topic))
					return true;
			}
		}

		topic = null;
		return false;
	}
}
