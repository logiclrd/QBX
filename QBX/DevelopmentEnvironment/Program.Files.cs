using System.IO;

using QBX.CodeModel;

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
			using (var reader = new StreamReader(path))
			{
				string shortName = Path.GetFileName(path).ToUpperInvariant();

				Load(reader, shortName, replaceExistingProgram);
			}
		}

		public void Load(TextReader reader, string unitName, bool replaceExistingProgram)
		{
			if (replaceExistingProgram)
				ClearProgram();

			var unit = CompilationUnit.Read(reader, unitName, Parser, ignoreErrors: true);

			LoadedFiles.Add(unit);

			PrimaryViewport.SwitchTo(unit.Elements[0]);

			if (SplitViewport != null)
				SplitViewport.SwitchTo(unit.Elements[0]);
		}

		public void SaveFile(CompilationUnit unit, string filePath, bool saveBackup = true)
		{
			if (saveBackup && File.Exists(filePath))
			{
				string backupFilePath = Path.ChangeExtension(filePath, ".ba~");

				File.Delete(backupFilePath);
				File.Move(filePath, backupFilePath);
			}

			using (var writer = new StreamWriter(filePath))
				Save(unit, writer);
		}

		public void Save(CompilationUnit unit, TextWriter writer)
		{
			unit.Write(writer);
			unit.IsPristine = true;
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

			unit.Name = fileName;

			if (replaceExistingProgram)
				ClearProgram();

			LoadedFiles.Add(unit);

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

			if (PrimaryViewport.CompilationUnit == unit)
				PrimaryViewport.SwitchTo(LoadedFiles[0].Elements[0]);

			if (SplitViewport?.CompilationUnit == unit)
				SplitViewport.SwitchTo(LoadedFiles[0].Elements[0]);
		}
	}
}
