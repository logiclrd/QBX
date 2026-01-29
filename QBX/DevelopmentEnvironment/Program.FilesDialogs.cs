using System;

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

		public void PromptToSaveChanges(CompilationUnit unit, Action continuation, Action? notifySave = null)
		{
			if (unit.IsPristine)
			{
				continuation();
				return;
			}

			var dialog = new PromptToSaveDialog(Configuration);

			dialog.Save +=
				() =>
				{
					notifySave?.Invoke();
					InteractiveSave(unit, continuation);
				};

			dialog.DoNotSave += continuation;

			ShowDialog(dialog);
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

		public void SaveIfNeeded(CompilationUnit unit, Action? continuation = null)
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

		public void InteractiveSave(CompilationUnit unit, Action? continuation = null)
		{
			var dialog = new SaveFileDialog(unit.FilePath, Configuration);

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
			var dialog = new OpenFileDialog(Configuration);

			dialog.Error +=
				(error) =>
				{
					PresentError(error);
				};

			dialog.FileSelected +=
				(filePath) =>
				{
					LoadFile(filePath, replaceExistingProgram);
					dialog.Close();
				};

			ShowDialog(dialog);
		}
	}
}
