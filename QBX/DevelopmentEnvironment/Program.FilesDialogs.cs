using System;
using System.IO;

using QBX.CodeModel;
using QBX.DevelopmentEnvironment.Dialogs;

namespace QBX.DevelopmentEnvironment
{
	partial class Program
	{
		public void PromptToSaveChanges(Action continuation)
		{
			int i = 0;
			bool saving = false;

			void Continue()
			{
				if (i < LoadedFiles.Count)
				{
					var unit = LoadedFiles[i];

					i++;

					if (!saving)
						PromptToSaveChanges(unit, Continue, notifySave: () => saving = true);
					else
						InteractiveSave(unit, Continue);
				}
				else
					continuation();
			}

			Continue();
		}

		public Dialog? PromptToSaveChanges(IEditableUnit unit, Action continuation, Action? notifySave = null)
		{
			if (unit.IsPristine)
			{
				continuation();
				return null;
			}

			var dialog = new PromptToSaveDialog(Machine, Configuration);

			dialog.Save +=
				() =>
				{
					notifySave?.Invoke();
					InteractiveSave(unit, continuation);
				};

			dialog.DoNotSave += continuation;

			return ShowDialog(dialog);
		}

		public void SaveAll()
		{
			int i = 0;

			void Continue()
			{
				if (i < LoadedFiles.Count)
				{
					var unit = LoadedFiles[i];

					i++;

					SaveIfNeeded(unit, Continue);
				}
			}

			Continue();
		}

		public void SaveIfNeeded(IEditableUnit unit, Action? continuation = null)
		{
			if (unit.IsPristine)
				continuation?.Invoke();
			else if (unit.FilePath != "")
			{
				SaveFile(unit, unit.FilePath);
				continuation?.Invoke();
			}
			else
				InteractiveSave(unit, continuation);
		}

		public void InteractiveSaveIfUnitHasNoFilePath(CompilationUnit unit)
		{
			if (unit.HasName)
				SaveFile(unit, unit.FilePath);
			else
				InteractiveSave(unit);
		}

		public void InteractiveSave(IEditableUnit unit, Action? continuation = null, SaveFileDialogTitle title = SaveFileDialogTitle.Save)
		{
			var dialog = new SaveFileDialog(Machine, Configuration, title, unit.FilePath);

			dialog.Error +=
				(error) =>
				{
					PresentError(error);
				};

			dialog.TargetPathSpecified +=
				(filePath) =>
				{
					SaveFile(unit, filePath);
					dialog.Close();
				};

			dialog.Closed +=
				(_, _) =>
				{
					continuation?.Invoke();
				};

			ShowDialog(dialog);
		}

		public void ShowOpenFileDialog(bool replaceExistingProgram)
		{
			var title = replaceExistingProgram
				? OpenFileDialogTitle.OpenProgram
				: OpenFileDialogTitle.LoadFile;

			var dialog = new OpenFileDialog(Machine, Configuration, title);

			dialog.Error +=
				(error) =>
				{
					PresentError(error);
				};

			dialog.FileSelected +=
				(filePath) =>
				{
					dialog.IsVisible = false;

					var dummyUnit = CompilationUnit.CreateNew();

					dummyUnit.FilePath = filePath;

					FocusedViewport.SwitchTo(dummyUnit.Elements[0]);

					Render();

					LoadFile(
						filePath,
						replaceExistingProgram,
						lineCountCallback:
							lineCount =>
							{
								TextLibrary.MoveCursor(0, TextLibrary.Height - 1);
								RenderReferenceBar(overrideLineNumber: lineCount);
							});

					dialog.Close();
				};

			ShowDialog(dialog);
		}

		public void ShowCreateFileDialog()
		{
			var dialog = new CreateFileDialog(Machine, Configuration);

			dialog.CreateFile +=
				() =>
				{
					CompilationUnit newUnit;

					if ((LoadedFiles.Count == 1)
					 && (LoadedFiles[0] is CompilationUnit existingUnit)
					 && existingUnit.IsEmpty
					 && existingUnit.IsPristine)
						newUnit = existingUnit;
					else
						newUnit = CompilationUnit.CreateNew();

					newUnit.FilePath = Path.GetFullPath(dialog.FileName);
					newUnit.IsPristine = false;

					if (newUnit != LoadedFiles[0])
						LoadedFiles.Add(newUnit);

					FocusedViewport.SwitchTo(newUnit.Elements[0]);
				};

			ShowDialog(dialog);
		}
	}
}
