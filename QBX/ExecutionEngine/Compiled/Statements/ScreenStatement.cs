using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ScreenStatement(CodeModel.Statements.ScreenStatement source) : Executable(source)
{
	public Evaluable? ModeExpression;
	public Evaluable? ColourSwitchExpression;
	public Evaluable? ActivePageExpression;
	public Evaluable? VisiblePageExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (ModeExpression != null)
		{
			int qbMode = ModeExpression.EvaluateAndCoerceToInt(context, stackFrame);

			bool clearVRAM = (qbMode != context.RuntimeState.LastScreenMode);

			int hardwareMode = System.Array.FindLastIndex(
				Video.Modes,
				mode => mode?.ScreenNumber == qbMode);

			if (!context.Machine.VideoFirmware.SetMode(hardwareMode, clearVRAM))
				throw RuntimeException.IllegalFunctionCall(ModeExpression.Source);

			context.RuntimeState.LastScreenMode = qbMode;

			context.VisualLibrary = context.Machine.VideoFirmware.VisualLibrary;

			var graphics = context.VisualLibrary as GraphicsLibrary;

			graphics?.LastPoint = (graphics.Width / 2, graphics.Height / 2);

			if (Video.Modes[hardwareMode] is ModeParameters modeParams)
			{
				context.RuntimeState.EnablePaletteRemapping = (hardwareMode < 0x12);
				context.RuntimeState.PaletteMode = modeParams.Use256Colours ? PaletteMode.DAC : PaletteMode.Attribute;
				context.RuntimeState.MaximumAttribute = modeParams.Use256Colours ? 255 : 15;

				if (modeParams.ShiftRegisterInterleave)
				{
					context.RuntimeState.PaletteMode = PaletteMode.CGA;
					context.RuntimeState.MaximumAttribute = 3;
				}
				else if (modeParams.IsMonochrome)
					context.RuntimeState.MaximumAttribute = 1;
			}

			switch (qbMode)
			{
				case 0:
				case 9:
					context.RuntimeState.MaximumColour = 63;
					break;
				case 1:
				case 7:
				case 8:
					context.RuntimeState.MaximumColour = 15;
					break;
				case 10:
					context.RuntimeState.MaximumColour = 7;
					break;
				case 11:
				case 12:
				case 13:
					context.RuntimeState.MaximumColour = -1; // unused for this mode
					break;
			}

			if (graphics != null)
				context.DrawProcessor.Initialize(graphics, context.RuntimeState.MaximumAttribute);
			else
				context.DrawProcessor.Disable();
		}

		if (ActivePageExpression != null)
		{
			int activePage = ActivePageExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if (!context.VisualLibrary.SetActivePage(activePage))
				throw RuntimeException.IllegalFunctionCall(ActivePageExpression.Source);
		}

		if (VisiblePageExpression != null)
		{
			int visiblePage = VisiblePageExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if (!context.Machine.VideoFirmware.SetVisiblePage(visiblePage))
				throw RuntimeException.IllegalFunctionCall(VisiblePageExpression.Source);
		}
	}
}
