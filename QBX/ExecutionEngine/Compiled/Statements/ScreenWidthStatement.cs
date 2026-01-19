using System;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ScreenWidthStatement(CodeModel.Statements.ScreenWidthStatement source)
	: Executable(source)
{
	public Evaluable? WidthExpression;
	public Evaluable? HeightExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if ((WidthExpression == null) && (HeightExpression == null))
			throw new Exception("ScreenWidthStatement with no argument expressions");

		int width = context.VisualLibrary.CharacterWidth;
		int height = context.VisualLibrary.CharacterHeight;

		if (WidthExpression != null)
			width = WidthExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (HeightExpression != null)
			height = HeightExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (width != context.VisualLibrary.CharacterWidth)
		{
			if (context.VisualLibrary is not TextLibrary textLibrary)
				throw RuntimeException.IllegalFunctionCall(Source);

			context.Machine.VideoFirmware.SetMode(
				width switch
				{
					80 => 3,
					40 => 1,

					_ => throw RuntimeException.IllegalFunctionCall(WidthExpression?.Source)
				});

			textLibrary.RefreshParameters();
		}

		if (height != context.VisualLibrary.CharacterWidth)
		{
			try
			{
				switch (context.VisualLibrary)
				{
					case TextLibrary:
						context.Machine.VideoFirmware.SetCharacterRows(height);
						break;
					case GraphicsLibrary graphicsLibrary:
					{
						int characterScans = graphicsLibrary.Height / height;

						if (!graphicsLibrary.SetCharacterScans(characterScans))
							throw new Exception();

						break;
					}
				}

				context.VisualLibrary.RefreshParameters();
				context.VisualLibrary.Clear();
			}
			catch
			{
				throw RuntimeException.IllegalFunctionCall(HeightExpression?.Source);
			}
		}
	}
}
