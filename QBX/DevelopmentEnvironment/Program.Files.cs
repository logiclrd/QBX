using QBX.CodeModel;
using QBX.LexicalAnalysis;

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

			PrimaryViewport.CompilationUnit = unit;
			PrimaryViewport.CompilationElement = unit.Elements[0];

			if (SplitViewport != null)
			{
				SplitViewport.CompilationUnit = unit;
				SplitViewport.CompilationElement = unit.Elements[0];
			}
		}

		public void LoadFile(string path, bool replaceExistingProgram)
		{
			if (replaceExistingProgram)
				ClearProgram();

			using (var reader = new StreamReader(path))
			{
				var lexer = new Lexer(reader);

				var unit = Parser.Parse(lexer);

				unit.Name = Path.GetFileName(path).ToUpperInvariant();

				LoadedFiles.Add(unit);

				if (replaceExistingProgram)
				{
					PrimaryViewport.CompilationUnit = unit;
					PrimaryViewport.CompilationElement = unit.Elements[0];

					if (SplitViewport != null)
					{
						SplitViewport.CompilationUnit = unit;
						SplitViewport.CompilationElement = unit.Elements[0];
					}
				}
			}
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

				PrimaryViewport.CompilationUnit = unit;
				PrimaryViewport.CompilationElement = unit.Elements[0];

				if (SplitViewport != null)
				{
					SplitViewport.CompilationUnit = unit;
					SplitViewport.CompilationElement = unit.Elements[0];
				}
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
