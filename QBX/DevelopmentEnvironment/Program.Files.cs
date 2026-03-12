using System;
using System.IO;
using System.Linq;

using QBX.CodeModel;
using QBX.ExecutionEngine;
using QBX.Firmware.Fonts;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment
{
	partial class Program
	{
		void ClearProgram()
		{
			LoadedFiles.Clear();
			MainModuleIndex = 0;
		}

		public void StartNewProgram()
		{
			ClearProgram();

			var unit = CompilationUnit.CreateNew();

			LoadedFiles.Add(unit);
			ResetCallsMenu();

			PrimaryViewport.SwitchTo(unit.Elements[0]);

			if (SplitViewport != null)
				SplitViewport.SwitchTo(unit.Elements[0]);
		}

		public void LoadFile(string path, bool replaceExistingProgram)
		{
			try
			{
				using (var reader = new StreamReader(path, new CP437Encoding(ControlCharacterInterpretation.Semantic)))
					Load(reader, path, replaceExistingProgram);
			}
			catch (IOException e)
			{
				PresentError(RuntimeException.ForIOException(e), ErrorSource.DevelopmentEnvironment);
			}
			catch (Exception e)
			{
				PresentError(e.Message);
			}
		}

		public void Load(TextReader reader, string filePath, bool replaceExistingProgram)
		{
			if (replaceExistingProgram)
			{
				ClearProgram();

				string makeFileName = Path.ChangeExtension(filePath, ".MAK");

				if (File.Exists(makeFileName) && !FileIdentityUtility.IsSameFile(filePath, makeFileName))
				{
					if (!TryLoadMakeFileItems(makeFileName))
						PresentError(RuntimeException.BadFileName(), ErrorSource.DevelopmentEnvironment);

					return;
				}
			}

			var unit = CompilationUnit.Read(reader, filePath, Parser, ignoreErrors: true);

			LoadedFiles.Add(unit);

			var mainModule = LoadedFiles.First(u => u.IncludeInBuild);

			if (unit != mainModule)
				mainModule.IsPristine = false; // trigger save to .MAK file

			PrimaryViewport.SwitchTo(unit.Elements[0]);

			if (SplitViewport != null)
				SplitViewport.SwitchTo(unit.Elements[0]);
		}

		public void SaveFile(IEditableUnit editable, string filePath, bool saveBackup = true)
		{
			if (saveBackup && File.Exists(filePath))
			{
				string backupExtension = Path.GetExtension(filePath) ?? ".BAS";

				backupExtension = backupExtension.Remove(backupExtension.Length - 1) + "~";

				string backupFilePath = Path.ChangeExtension(filePath, backupExtension);

				File.Delete(backupFilePath);
				File.Move(filePath, backupFilePath);
			}

			using (var writer = new StreamWriter(filePath) { NewLine = "\r\n" })
				Save(editable, writer);

			editable.FilePath = filePath;

			// Also write a .MAK file if this is a multi-module project and this is the first module.
			bool isMultiModule =
				(editable == LoadedFiles.FirstOrDefault()) &&
				LoadedFiles.Any(unit => (unit != editable) && unit.IncludeInBuild);

			string makeFileName = Path.ChangeExtension(filePath, ".MAK");

			if (!FileIdentityUtility.IsSameFile(filePath, makeFileName))
			{
				if (isMultiModule)
				{
					if (!TrySaveMakeFile(makeFileName))
						editable.IsPristine = false;
				}
				else
					File.Delete(makeFileName);
			}

			PrimaryViewport.UpdateHeading();
			SplitViewport?.UpdateHeading();
		}

		public bool TrySaveMakeFile(string makeFilePath)
		{
			try
			{
				string basePath = Path.GetDirectoryName(Path.GetFullPath(makeFilePath)) ?? ".";

				using (var writer = new StreamWriter(makeFilePath))
				{
					foreach (var unit in LoadedFiles.Where(u => u.IncludeInBuild))
						writer.WriteLine(Path.GetRelativePath(basePath, unit.FilePath));
				}

				return true;
			}
			catch (IOException e)
			{
				PresentError(RuntimeException.ForIOException(e), ErrorSource.DevelopmentEnvironment);
			}
			catch (Exception e)
			{
				PresentError(e.Message);
			}

			return false;
		}

		public bool TryLoadMakeFileItems(string makeFilePath)
		{
			bool success = false;

			try
			{
				using (var reader = new StreamReader(makeFilePath))
				{
					while (true)
					{
						string? relativePath = reader.ReadLine();

						if (relativePath == null)
							break;

						if (File.Exists(relativePath))
						{
							LoadFile(relativePath, replaceExistingProgram: false);
							success = true;
						}
					}
				}

				if (!success)
					throw RuntimeException.BadFileName();
			}
			catch (Exception e)
			{
				PresentError(e.ToString());
				success = false;
			}

			return success;
		}

		void Save(IEditableUnit editable, TextWriter writer)
		{
			editable.Write(writer);
			editable.IsPristine = true;
		}

		bool IsBlankProgram()
		{
			if (LoadedFiles.Count == 0)
				return true;
			if (LoadedFiles.Count > 1)
				return false;

			var file = LoadedFiles[0];

			return file.IsEmpty && file.IsPristine;
		}

		public void CreateFile(string fileName)
		{
			bool replaceExistingProgram = IsBlankProgram();

			var unit = CompilationUnit.CreateNew();

			string filePath = fileName;

			if (Path.GetDirectoryName(filePath) == null)
				filePath = Path.Combine(Environment.CurrentDirectory, filePath);

			unit.FilePath = filePath;

			if (replaceExistingProgram)
				ClearProgram();

			LoadedFiles.Add(unit);

			var mainModule = LoadedFiles.First(u => u.IncludeInBuild);

			if (mainModule != unit)
				mainModule.IsPristine = false;

			if (replaceExistingProgram)
			{
				LoadedFiles.Clear();

				PrimaryViewport.SwitchTo(unit.Elements[0]);

				if (SplitViewport != null)
					SplitViewport.SwitchTo(unit.Elements[0]);
			}
		}

		public void RemoveFile(CompilationUnit unit)
		{
			var unitIndex = LoadedFiles.IndexOf(unit);

			if (unitIndex < 0)
				return;

			if (MainModuleIndex == unitIndex)
				MainModuleIndex = 0;

			LoadedFiles.RemoveAt(unitIndex);

			var mainModule = LoadedFiles.FirstOrDefault(u => u.IncludeInBuild);

			if (mainModule != null)
				mainModule.IsPristine = false;

			if (PrimaryViewport.EditableUnit == unit)
				PrimaryViewport.SwitchTo(LoadedFiles[0].Elements[0]);

			if (SplitViewport?.EditableUnit == unit)
				SplitViewport.SwitchTo(LoadedFiles[0].Elements[0]);
		}
	}
}
