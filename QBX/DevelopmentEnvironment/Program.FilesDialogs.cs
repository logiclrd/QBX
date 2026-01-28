using QBX.DevelopmentEnvironment.Dialogs;

namespace QBX.DevelopmentEnvironment
{
	partial class Program
	{
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
