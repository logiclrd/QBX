using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

using QBX.CodeModel;
using QBX.DevelopmentEnvironment.Dialogs;
using QBX.ExecutionEngine;
using QBX.Firmware.Fonts;
using QBX.OperatingSystem;
using QBX.Utility;
using QBX.Utility.Interop;

using OSFileMode = QBX.OperatingSystem.FileStructures.FileMode;
using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

using RegularFileDescriptor = QBX.OperatingSystem.FileDescriptors.RegularFileDescriptor;

namespace QBX.DevelopmentEnvironment
{
	partial class Program
	{
		void ClearProgram(bool chainExecution = false)
		{
			LoadedFiles.Clear();

			if (!chainExecution)
				Terminate();
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

		public void LoadFile(string path, bool replaceExistingProgram, Action<int>? lineCountCallback = null, CodeModel.Statements.Statement? errorContext = null)
		{
			using (var reader = DOSOpenFileReader(path, errorContext))
				LoadFile(reader, path, replaceExistingProgram, lineCountCallback);
		}

		public void LoadFile(StreamReader reader, string sourcePath, bool replaceExistingProgram, Action<int>? lineCountCallback = null)
		{
			try
			{
				Load(reader, sourcePath, replaceExistingProgram, lineCountCallback: lineCountCallback);
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

		public void Load(StreamReader reader, string filePath, bool replaceExistingProgram, bool chainExecution = false, Action<int>? lineCountCallback = null, CodeModel.Statements.Statement? errorContext = null)
		{
			if (!chainExecution)
				Terminate();

			if (replaceExistingProgram)
			{
				ClearProgram(chainExecution);

				string makeFileName = Path.ChangeExtension(filePath, ".MAK");

				StreamReader? makeFileReader = null;

				if (Path.GetExtension(filePath).Equals(".mak", StringComparison.InvariantCultureIgnoreCase))
					makeFileReader = reader;
				else
				{
					try
					{
						makeFileReader = DOSOpenFileReader(makeFileName, errorContext);
					}
					catch { }
				}

				if (makeFileReader != null)
				{
					using (makeFileReader)
					{
						if (makeFileReader.BaseStream != reader.BaseStream)
							reader.Dispose();

						string makeFileDirectory = Path.GetDirectoryName(makeFileName) ?? ".";

						if (!TryLoadMakeFileItems(makeFileReader, makeFileDirectory, showIDEUIFeedback: !chainExecution))
							PresentError(RuntimeException.BadFileName(), ErrorSource.DevelopmentEnvironment);

						reader.Dispose(); // Ensure the reference stays alive, in case the same underlying stream is referenced.

						return;
					}
				}
			}

			if (FileIsAlreadyLoaded(reader))
			{
				ShowDialog(new FilePreviouslyLoadedDialog(Machine, Configuration, filePath));
				return;
			}

			var unit = CompilationUnit.Read(reader, filePath, Configuration.TabSize, ignoreErrors: true, lineCountCallback);

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

		private bool FileIsAlreadyLoaded(StreamReader reader)
		{
			if (reader.BaseStream is FileStream fileStream)
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return FileIsAlreadyLoaded(fileStream.SafeFileHandle, new FileIndexProvider());
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return FileIsAlreadyLoaded(fileStream.SafeFileHandle, new LinuxINodeProvider());
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
					return FileIsAlreadyLoaded(fileStream.SafeFileHandle, new FreeBSDINodeProvider());
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					return FileIsAlreadyLoaded(fileStream.SafeFileHandle, new OSXINodeProvider());
			}

			return false;
		}

		private bool FileIsAlreadyLoaded<TINode>(string filePath, INodeProvider<TINode> inodeProvider)
			where TINode : INode<TINode>
		{
			if (inodeProvider.TryGetINode(filePath, out var inode))
				return FileIsAlreadyLoaded(inode, inodeProvider);

			filePath = Path.GetFullPath(filePath);

			return LoadedFiles.Any(u => u.FilePath.Equals(filePath));
		}

		private bool FileIsAlreadyLoaded<TINode>(SafeFileHandle fileHandle, INodeProvider<TINode> inodeProvider)
			where TINode : INode<TINode>
		{
			if (inodeProvider.TryGetINode(fileHandle, out var inode))
				return FileIsAlreadyLoaded(inode, inodeProvider);

			return false;
		}

		private bool FileIsAlreadyLoaded<TINode>(TINode inode, INodeProvider<TINode> inodeProvider)
			where TINode : INode<TINode>
		{
			foreach (var file in LoadedFiles)
			{
				if (inodeProvider.TryGetINode(file.FilePath, out var loadedINode)
				 && (inode == loadedINode))
					return true;
			}

			return false;
		}

		public void SaveFile(IEditableUnit editable, string filePath, bool saveBackup = true)
		{
			string longFilePath = ShortFileNames.Unmap(filePath);

			if (saveBackup && File.Exists(longFilePath))
			{
				string backupExtension = Path.GetExtension(longFilePath) ?? ".BAS";

				backupExtension = backupExtension.Remove(backupExtension.Length - 1) + "~";

				string backupFilePath = Path.ChangeExtension(longFilePath, backupExtension);

				File.Delete(backupFilePath);
				File.Move(longFilePath, backupFilePath);
			}

			using (var writer = DOSOpenFileWriter(longFilePath))
				Save(editable, writer);

			editable.FilePath = filePath;

			// Also write a .MAK file if this is a multi-module project and this is the first module.
			bool isMultiModule =
				(editable == LoadedFiles.FirstOrDefault()) &&
				LoadedFiles.Any(unit => (unit != editable) && unit.IncludeInBuild);

			string makeFileName = Path.ChangeExtension(longFilePath, ".MAK");

			if (!FileIdentityUtility.IsSameFile(longFilePath, makeFileName))
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

				using (var writer = new StreamWriter(makeFilePath, append: false, new CP437Encoding(ControlCharacterInterpretation.Semantic)))
				{
					writer.NewLine = "\r\n";

					foreach (var unit in LoadedFiles.Where(u => u.IncludeInBuild))
						writer.WriteLine(Path.GetRelativePath(basePath, ShortFileNames.Unmap(unit.FilePath)));
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

		public bool TryLoadMakeFileItems(StreamReader reader, string makeFileDirectory, bool showIDEUIFeedback, CodeModel.Statements.Statement? errorContext = null)
		{
			bool success = false;

			try
			{
				var dummyUnit = CompilationUnit.CreateNew();

				FocusedViewport.SwitchTo(dummyUnit.Elements[0]);

				while (true)
				{
					string? relativePath = reader.ReadLine();

					if (relativePath == null)
					{
						FocusedViewport.SwitchTo(LoadedFiles[0].Elements[0]);
						break;
					}

					string resolvedPath = ShortPath.Join(makeFileDirectory, relativePath);

					StreamReader? moduleReader = null;

					try
					{
						moduleReader = DOSOpenFileReader(resolvedPath, errorContext);
					}
					catch {}

					if (moduleReader != null)
					{
						using (moduleReader)
						{
							Action<int>? lineCountCallback = null;

							if (showIDEUIFeedback)
							{
								FocusedViewport.Heading = Path.GetFileName(resolvedPath);
								Render();

								lineCountCallback =
									lineCount =>
									{
										TextLibrary.MoveCursor(0, TextLibrary.Height - 1);
										UpdateReferenceBar(overrideLineNumber: lineCount);
									};
							}

							using (ShowReferenceBarTextForOperation("Loading and parsing", highlighted: true))
							{
								LoadFile(
									moduleReader,
									resolvedPath,
									replaceExistingProgram: false,
									lineCountCallback: lineCountCallback);
							}

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
			EnsureAllCodeIsParsed(presentErrors: false);

			editable.PrepareForWrite(allElements: LoadedFiles.SelectMany(file => file.Elements));
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

			unit.FilePath = ShortFileNames.GetFullPath(fileName);

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

		public void RemoveFile(IEditableUnit unit)
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

		StreamReader DOSOpenFileReader(string fileName, CodeModel.Statements.Statement? errorContext)
		{
			int fileHandle = -1;
			bool openSucceeded = false;

			if (Path.GetExtension(fileName) == "")
			{
				fileHandle = Machine.DOS.OpenFile(
					fileName,
					OSFileMode.Open,
					OSOpenMode.Access_ReadOnly | OSOpenMode.Share_DenyNone);

				switch (Machine.DOS.LastError)
				{
					case DOSError.None: openSucceeded = true; break;
					case DOSError.FileNotFound: fileName = fileName.TrimEnd('.') + ".BAS"; break;
					default: throw RuntimeException.ForDOSError(Machine.DOS.LastError, errorContext);
				}
			}

			if (!openSucceeded) // try again because we've altered fileName
			{
				fileHandle = Machine.DOS.OpenFile(
					fileName,
					OSFileMode.Open,
					OSOpenMode.Access_ReadOnly | OSOpenMode.Share_DenyNone);

				if (Machine.DOS.LastError != DOSError.None)
					throw RuntimeException.ForDOSError(Machine.DOS.LastError, errorContext);
			}

			if ((fileHandle < 2) || (fileHandle >= Machine.DOS.Files.Count))
				throw RuntimeException.ForDOSError(DOSError.InvalidHandle, errorContext);

			var fileDescriptor = Machine.DOS.Files[fileHandle];

			if (fileDescriptor is not RegularFileDescriptor regularFileDescriptor)
				throw RuntimeException.ForDOSError(DOSError.GeneralFailure, errorContext);

			var reader = new ScopedStreamReader(
				regularFileDescriptor.UnderlyingStream,
				new CP437Encoding(ControlCharacterInterpretation.Semantic));

			reader.Closed +=
				(_, _) =>
				{
					Machine.DOS.CloseFile(fileHandle);
				};

			return reader;
		}

		StreamWriter DOSOpenFileWriter(string fileName, CodeModel.Statements.Statement? errorContext = null)
		{
			if (Path.GetExtension(fileName) == "")
			{
				string longFileName = ShortFileNames.Unmap(fileName);

				if (!File.Exists(longFileName))
					fileName = fileName.TrimEnd('.') + ".BAS";
			}

			int fileHandle = Machine.DOS.OpenFile(
				fileName,
				OSFileMode.Create,
				OSOpenMode.Access_ReadWrite | OSOpenMode.Share_DenyWrite);

			if (Machine.DOS.LastError != DOSError.None)
				throw RuntimeException.ForDOSError(Machine.DOS.LastError, errorContext);

			if ((fileHandle < 2) || (fileHandle >= Machine.DOS.Files.Count))
				throw RuntimeException.ForDOSError(DOSError.InvalidHandle, errorContext);

			var fileDescriptor = Machine.DOS.Files[fileHandle];

			if (fileDescriptor is not RegularFileDescriptor regularFileDescriptor)
				throw RuntimeException.ForDOSError(DOSError.GeneralFailure, errorContext);

			var writer = new ScopedStreamWriter(
				regularFileDescriptor.UnderlyingStream,
				new CP437Encoding(ControlCharacterInterpretation.Semantic));

			writer.NewLine = "\r\n";

			writer.Closed +=
				(_, _) =>
				{
					Machine.DOS.CloseFile(fileHandle);
				};

			return writer;
		}
	}
}
