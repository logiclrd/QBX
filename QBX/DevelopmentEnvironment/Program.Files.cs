using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using QBX.CodeModel;
using QBX.DevelopmentEnvironment.Dialogs;
using QBX.ExecutionEngine;
using QBX.Firmware.Fonts;
using QBX.Utility;
using QBX.Utility.Interop;

namespace QBX.DevelopmentEnvironment
{
	partial class Program
	{
		void ClearProgram()
		{
			LoadedFiles.Clear();
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

		public void LoadFile(string path, bool replaceExistingProgram, Action<int>? lineCountCallback = null)
		{
			try
			{
				using (var reader = new StreamReader(path, new CP437Encoding(ControlCharacterInterpretation.Semantic)))
					Load(reader, path, replaceExistingProgram, lineCountCallback);
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

		class NameComparer : IComparer<IEditableUnit>
		{
			public int Compare(IEditableUnit? x, IEditableUnit? y)
			{
				int result = StringComparer.OrdinalIgnoreCase.Compare(x?.Name, y?.Name);

				if (result == 0)
					result = StringComparer.OrdinalIgnoreCase.Compare(x?.FilePath, y?.FilePath);

				return result;
			}
		}

		static NameComparer s_nameComparer = new NameComparer();

		public void Load(TextReader reader, string filePath, bool replaceExistingProgram, Action<int>? lineCountCallback = null)
		{
			if (replaceExistingProgram)
			{
				ClearProgram();

				string makeFileName = Path.ChangeExtension(filePath, ".MAK");

				if (File.Exists(makeFileName) && !FileIdentityUtility.IsSameFile(filePath, makeFileName))
				{
					reader.Dispose();

					if (!TryLoadMakeFileItems(makeFileName))
						PresentError(RuntimeException.BadFileName(), ErrorSource.DevelopmentEnvironment);

					return;
				}
			}

			if (FileIsAlreadyLoaded(filePath))
			{
				ShowDialog(new FilePreviouslyLoadedDialog(Machine, Configuration, filePath));
				return;
			}

			var unit = CompilationUnit.Read(reader, filePath, Parser, ignoreErrors: true, lineCountCallback);

			int insertIndex = 0;

			if (LoadedFiles.Count > 0)
			{
				insertIndex = LoadedFiles.BinarySearch(
					index: 1,
					count: LoadedFiles.Count - 1,
					unit,
					s_nameComparer);
			}

			if (insertIndex < 0)
				insertIndex = ~insertIndex;

			LoadedFiles.Insert(insertIndex, unit);

			var mainModule = LoadedFiles.First(u => u.IncludeInBuild);

			if (unit != mainModule)
				mainModule.IsPristine = false; // trigger save to .MAK file

			PrimaryViewport.SwitchTo(unit.Elements[0]);

			if (SplitViewport != null)
				SplitViewport.SwitchTo(unit.Elements[0]);
		}

		private bool FileIsAlreadyLoaded(string filePath)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return FileIsAlreadyLoaded(filePath, new FileIndexProvider());
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return FileIsAlreadyLoaded(filePath, new LinuxINodeProvider());
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
				return FileIsAlreadyLoaded(filePath, new FreeBSDINodeProvider());
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return FileIsAlreadyLoaded(filePath, new OSXINodeProvider());
			else
			{
				filePath = Path.GetFullPath(filePath);

				return LoadedFiles.Any(u => u.FilePath.Equals(filePath));
			}
		}

		private bool FileIsAlreadyLoaded<TINode>(string filePath, INodeProvider<TINode> inodeProvider)
			where TINode : INode<TINode>
		{
			if (inodeProvider.TryGetINode(filePath, out var inode))
			{
				foreach (var file in LoadedFiles)
				{
					if (inodeProvider.TryGetINode(file.FilePath, out var loadedINode)
					 && (inode == loadedINode))
						return true;
				}

				return false;
			}

			filePath = Path.GetFullPath(filePath);

			return LoadedFiles.Any(u => u.FilePath.Equals(filePath));
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
				var dummyUnit = CompilationUnit.CreateNew();

				FocusedViewport.SwitchTo(dummyUnit.Elements[0]);

				string makeFileDirectory = Path.GetDirectoryName(Path.GetFullPath(makeFilePath)) ?? ".";

				using (var reader = new StreamReader(makeFilePath))
				{
					while (true)
					{
						string? relativePath = reader.ReadLine();

						if (relativePath == null)
							break;

						if (File.Exists(relativePath))
						{
							FocusedViewport.Heading = Path.GetFileName(resolvedPath);
							Render();

							LoadFile(
								resolvedPath,
								replaceExistingProgram: false,
								lineCountCallback:
									lineCount =>
									{
										TextLibrary.MoveCursor(0, TextLibrary.Height - 1);
										RenderReferenceBar(overrideLineNumber: lineCount);
									});

							success = true;

							FocusedViewport.SwitchTo(dummyUnit.Elements[0]);
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
			editable.SortElements();
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

			LoadedFiles.RemoveAt(unitIndex);

			if (!LoadedFiles.Any(u => u.IncludeInBuild))
				LoadedFiles.Insert(0, CompilationUnit.CreateNew());

			if (PrimaryViewport.EditableUnit == unit)
				PrimaryViewport.SwitchTo(LoadedFiles[0].Elements[0]);
			if (SplitViewport?.EditableUnit == unit)
				SplitViewport.SwitchTo(LoadedFiles[0].Elements[0]);

			if (unitIndex == 0)
				SetMainModule();
			else
			{
				var mainModule = LoadedFiles.First(u => u.IncludeInBuild);

				if (mainModule != LoadedFiles[0])
				{
					LoadedFiles.Remove(mainModule);
					LoadedFiles.Insert(0, mainModule);
				}

				mainModule.IsPristine = false;
			}
		}

		public SelectModuleDialog SetMainModule()
		{
			var dialog = new SelectModuleDialog(LoadedFiles, Machine, Configuration);

			dialog.ModuleSelected +=
				() =>
				{
					var unit = dialog.SelectedItem;

					if (LoadedFiles.Remove(unit))
						LoadedFiles.Insert(0, unit);

					unit.IsPristine = false;
				};

			return ShowDialog(dialog);
		}
	}
}
