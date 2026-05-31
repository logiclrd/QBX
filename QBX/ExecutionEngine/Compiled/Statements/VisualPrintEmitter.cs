using System;

using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class VisualPrintEmitter(VisualLibrary visual) : PrintEmitter
{
	public override int CursorX
	{
		get => visual.CursorX;
		set => visual.MoveCursor(value, visual.CursorY);
	}

	public override int Width => visual.CharacterWidth;

	public override void EmitNewLine() => visual.NewLine();

	public override void Emit(ReadOnlySpan<char> chars)
	{
		if ((visual.CursorX > 0) && (visual.CursorX + chars.Length >= visual.CharacterWidth))
			visual.NewLine();

		visual.WriteText(chars);
	}

	public override void Emit(ReadOnlySpan<byte> str)
	{
		if ((visual.CursorX > 0) && (visual.CursorX + str.Length >= visual.CharacterWidth))
			visual.NewLine();

		visual.WriteText(str);
	}

	public override void Emit(char ch) => visual.WriteText(ch);
	public override void Emit(byte ch) => visual.WriteText(ch);
}
