using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using QBX.Hardware;
using QBX.Parser;
using QBX.Utility;

namespace QBX.Firmware;

public class MouseDriver
{
	public bool IsInitialized;

	public int PointerX => Bounds.ConstrainX(_machine.Mouse.X * _width / _machine.Mouse.Width);
	public int PointerY => Bounds.ConstrainY(_machine.Mouse.Y * _height / _machine.Mouse.Height);

	public MouseButton LeftButton;
	public MouseButton MiddleButton;
	public MouseButton RightButton;

	public int PointerMaximumX => _width - 1;
	public int PointerMaximumY => _height - 1;

	public IntegerRect Bounds;

	public bool PointerVisible => (_pointerVisible > 0) && !ExclusionArea.Contains(PointerX, PointerY);

	public int DisplayPageNumber => _displayPageNumber;

	int _displayPageNumber;

	public int LastMickeyX;
	public int LastMickeyY;

	public IntegerRect ExclusionArea;
	public bool IsExcluded;

	// Text depiction
	public bool TextPointerEnableSoftware;
	public byte TextPointerCharacterMask;
	public byte TextPointerCharacterInvert;
	public byte TextPointerAttributeMask;
	public byte TextPointerAttributeInvert;
	public byte TextPointerHardwareCursorStartScan;
	public byte TextPointerHardwareCursorEndScan;

	// Graphical depiction
	public byte[] PointerShape;
	public byte[] PointerShapeMask;
	public int PointerHotSpotX;
	public int PointerHotSpotY;

	public event Action? Initialized;
	public event Action? DisplayPageNumberChanged;
	public event Action? PositionChanged;
	public event Action? PointerVisibleChanged;
	public event Action? TextPointerAppearanceChanged;
	public event Action? PointerShapeChanged;

	Machine _machine;

	int _width;
	int _height;
	int _pointerVisible;

	public MouseDriver(Machine machine)
	{
		LeftButton = new MouseButton(this);
		MiddleButton = new MouseButton(this);
		RightButton = new MouseButton(this);

		_machine = machine;

		ResetTextPointerMode();
		ResetPointerShape();

		machine.VideoFirmware.ModeChanged +=
			_ =>
			{
				if (machine.GraphicsArray.Graphics.DisableText)
				{
					_width = machine.GraphicsArray.MiscellaneousOutput.BasePixelWidth >> (machine.GraphicsArray.Sequencer.DotDoubling ? 1 : 0);
					_height = machine.GraphicsArray.CRTController.NumScanLines;
				}
				else
				{
					int dotWidth = machine.GraphicsArray.MiscellaneousOutput.BasePixelWidth >> (machine.GraphicsArray.Sequencer.DotDoubling ? 1 : 0);
					int dotHeight = machine.GraphicsArray.CRTController.NumScanLines;

					_width = dotWidth * 8 / machine.GraphicsArray.Sequencer.CharacterWidth;
					_height = dotHeight * 8 / machine.GraphicsArray.CRTController.CharacterHeight;
				}

				ResetBounds();
			};

		machine.Mouse.PositionChanged +=
			() =>
			{
				if (IsInitialized)
				{
					bool newIsExcluded = ExclusionArea.Contains(PointerX, PointerY);

					if (newIsExcluded != IsExcluded)
					{
						UpdateHardwareTextPointer();
						PointerVisibleChanged?.Invoke();

						IsExcluded = newIsExcluded;
					}

					PositionChanged?.Invoke();
				}
			};

		machine.Mouse.ButtonChanged +=
			(which) =>
			{
				if (IsInitialized)
				{
					switch (which)
					{
						case Hardware.MouseButton.Left: LeftButton.Set(machine.Mouse.LeftButton); break;
						case Hardware.MouseButton.Middle: MiddleButton.Set(machine.Mouse.MiddleButton); break;
						case Hardware.MouseButton.Right: RightButton.Set(machine.Mouse.RightButton); break;
					}
				}
			};
	}

	public void SetDisplayPageNumber(int newPageNumber)
	{
		_displayPageNumber = newPageNumber;
		DisplayPageNumberChanged?.Invoke();
	}

	public void Reset()
	{
		ResetBounds();
		ResetPointerShape();

		_machine.Mouse.PushPositionChange(
			_machine.Mouse.Width / 2,
			_machine.Mouse.Height / 2);

		IsInitialized = true;

		Initialized?.Invoke();
	}

	public void ResetBounds()
	{
		Bounds.X1 = 0;
		Bounds.Y1 = 0;
		Bounds.X2 = _width - 1;
		Bounds.Y2 = _height - 1;

		_machine.Mouse.PushResetToTheGeometryOfSpace();
	}

	public void ResetTextPointerMode()
	{
		byte preserveBlinkBit = 0;

		if (_machine.GraphicsArray.AttributeController.EnableBlinking)
			preserveBlinkBit = 0x80;

		SetSoftwareTextPointer(
			characterMask: 0xFF,
			characterInvert: 0x00,
			attributeMask: preserveBlinkBit,
			attributeInvert: unchecked((byte)(0xFF & ~preserveBlinkBit)));
	}

	public void SetSoftwareTextPointer(byte characterMask, byte characterInvert, byte attributeMask, byte attributeInvert)
	{
		if (!IsInitialized)
			return;

		TextPointerEnableSoftware = true;
		TextPointerCharacterMask = characterMask;
		TextPointerCharacterInvert = characterInvert;
		TextPointerAttributeMask = attributeMask;
		TextPointerAttributeInvert = attributeInvert;

		TextPointerAppearanceChanged?.Invoke();
	}

	public void SetHardwareTextPointer(int startScan, int endScan)
	{
		if (!IsInitialized)
			return;

		if (startScan < 0)
			startScan = 0;
		if (startScan > 31)
			startScan = 31;
		if (endScan < 0)
			endScan = 0;
		if (endScan > 31)
			endScan = 31;

		if (startScan > endScan)
			(startScan, endScan) = (endScan, startScan);

		TextPointerEnableSoftware = false;
		TextPointerHardwareCursorStartScan = unchecked((byte)startScan);
		TextPointerHardwareCursorEndScan = unchecked((byte)endScan);

		TextPointerAppearanceChanged?.Invoke();
	}

	void UpdateHardwareTextPointer()
	{
		if (TextPointerEnableSoftware)
			return;

		int characterWidth = _machine.GraphicsArray.CRTController.Registers.EndHorizontalDisplay + 1;

		int cursorOffset = (PointerY / 8) * characterWidth + (PointerX / 8);

		// Move the cursor
		_machine.OutPort(
			GraphicsArray.CRTControllerRegisters.IndexPort,
			GraphicsArray.CRTControllerRegisters.CursorLocationLow);
		_machine.OutPort(
			GraphicsArray.CRTControllerRegisters.DataPort,
			unchecked((byte)(cursorOffset & 0xFF)));

		_machine.OutPort(
			GraphicsArray.CRTControllerRegisters.IndexPort,
			GraphicsArray.CRTControllerRegisters.CursorLocationHigh);
		_machine.OutPort(
			GraphicsArray.CRTControllerRegisters.DataPort,
			unchecked((byte)(cursorOffset & 0xFF)));

		// Set the cursor scan range
		_machine.OutPort(
			GraphicsArray.CRTControllerRegisters.IndexPort,
			GraphicsArray.CRTControllerRegisters.CursorStart);
		_machine.OutPort(
			GraphicsArray.CRTControllerRegisters.DataPort,
			unchecked((byte)(TextPointerHardwareCursorStartScan & GraphicsArray.CRTControllerRegisters.CursorStart_Mask)));

		_machine.OutPort(
			GraphicsArray.CRTControllerRegisters.IndexPort,
			GraphicsArray.CRTControllerRegisters.CursorEnd);
		_machine.OutPort(
			GraphicsArray.CRTControllerRegisters.DataPort,
			unchecked((byte)(TextPointerHardwareCursorEndScan & GraphicsArray.CRTControllerRegisters.CursorEnd_Mask)));
	}

	[MemberNotNull(nameof(PointerShape))]
	[MemberNotNull(nameof(PointerShapeMask))]
	public void ResetPointerShape()
	{
		SetPointerShape(
			newPointerShape:
			[
				0b00000000, 0b00000000,
				0b01000000, 0b00000000,
				0b01100000, 0b00000000,
				0b01110000, 0b00000000,
				0b01111000, 0b00000000,
				0b01111100, 0b00000000,
				0b01111110, 0b00000000,
				0b01111111, 0b00000000,
				0b01111111, 0b10000000,
				0b01111100, 0b00000000,
				0b01101100, 0b00000000,
				0b01000110, 0b00000000,
				0b00000110, 0b00000000,
				0b00000011, 0b00000000,
				0b00000011, 0b00000000,
				0b00000000, 0b00000000,
			],
			newPointerShapeMask:
			[
				0b00111111, 0b11111111,
				0b00011111, 0b11111111,
				0b00001111, 0b11111111,
				0b00000111, 0b11111111,
				0b00000011, 0b11111111,
				0b00000001, 0b11111111,
				0b00000000, 0b11111111,
				0b00000000, 0b01111111,
				0b00000000, 0b00111111,
				0b00000000, 0b00011111,
				0b00000001, 0b11111111,
				0b00000000, 0b11111111,
				0b00110000, 0b11111111,
				0b11111000, 0b01111111,
				0b11111000, 0b01111111,
				0b11111100, 0b11111111,
			],
			hotSpotX: 0,
			hotSpotY: 0);
	}

	[MemberNotNull(nameof(PointerShape))]
	[MemberNotNull(nameof(PointerShapeMask))]
	public void SetPointerShape(byte[] newPointerShape, byte[] newPointerShapeMask, int hotSpotX, int hotSpotY)
	{
		if (newPointerShape.Length == 32)
			PointerShape = newPointerShape;
		else
		{
			if (newPointerShape.Length > 32)
				PointerShape = newPointerShape.Slice(0, 32).ToArray();
			else
			{
				PointerShape = new byte[32];
				newPointerShape.CopyTo(PointerShape);
			}
		}

		if (newPointerShapeMask.Length == 32)
			PointerShapeMask = newPointerShapeMask;
		else
		{
			if (newPointerShapeMask.Length > 32)
				PointerShapeMask = newPointerShapeMask.Slice(0, 32).ToArray();
			else
			{
				PointerShapeMask = new byte[32];
				newPointerShapeMask.CopyTo(PointerShapeMask);
			}
		}

		if (IsInitialized)
		{
			PointerHotSpotX = hotSpotX;
			PointerHotSpotY = hotSpotY;

			PointerShapeChanged?.Invoke();
		}
	}

	[MemberNotNull(nameof(PointerShape))]
	[MemberNotNull(nameof(PointerShapeMask))]
	public void SetPointerShape(Span<byte> newPointerShape, Span<byte> newPointerShapeMask, int hotSpotX, int hotSpotY)
	{
		if (newPointerShape.Length > 32)
			PointerShape = newPointerShape.Slice(0, 32).ToArray();
		else
		{
			PointerShape = new byte[32];
			newPointerShape.CopyTo(PointerShape);
		}

		if (newPointerShapeMask.Length > 32)
			PointerShapeMask = newPointerShapeMask.Slice(0, 32).ToArray();
		else
		{
			PointerShapeMask = new byte[32];
			newPointerShapeMask.CopyTo(PointerShapeMask);
		}

		PointerHotSpotX = hotSpotX;
		PointerHotSpotY = hotSpotY;

		if (!IsInitialized)
			return;

		PointerShapeChanged?.Invoke();
	}

	public void ShowPointer()
	{
		if (!IsInitialized)
			return;

		ClearExclusionArea();

		_pointerVisible++;

		if (_pointerVisible == 1)
		{
			UpdateHardwareTextPointer();
			PointerVisibleChanged?.Invoke();
		}
	}

	public void HidePointer()
	{
		if (!IsInitialized)
			return;

		if (_pointerVisible > 0)
		{
			_pointerVisible--;

			if (_pointerVisible == 0)
				PointerVisibleChanged?.Invoke();
		}
	}

	public void MovePointer(int x, int y)
	{
		if (!IsInitialized)
			return;

		_machine.Mouse.PushPositionChange(
			x * _machine.Mouse.Width / _width,
			y * _machine.Mouse.Height / _height);
	}

	public void ConstrainPointer(int x1, int y1, int x2, int y2)
	{
		if (!IsInitialized)
			return;

		var bounds = new IntegerRect(0, 0, _width - 1, _height - 1);

		x1 = bounds.ConstrainX(x1);
		y1 = bounds.ConstrainX(y1);
		x2 = bounds.ConstrainY(x2);
		y2 = bounds.ConstrainY(y2);

		if (x1 > x2)
			(x1, x2) = (x2, x1);
		if (y1 > y2)
			(y1, y2) = (y2, y1);

		Bounds.X1 = x1;
		Bounds.Y1 = y1;
		Bounds.X2 = x2;
		Bounds.Y2 = y2;

		_machine.Mouse.PushChangeToTheGeometryOfSpace(
			x1 * _machine.Mouse.Width / _width,
			y1 * _machine.Mouse.Height / _height,
			x2 * _machine.Mouse.Width / _width,
			y2 * _machine.Mouse.Height / _height);
	}

	public void UnconstrainPointer() => ResetBounds();

	public IntegerPoint GetMotionMickeys()
	{
		int newX = _machine.Mouse.X;
		int newY = _machine.Mouse.Y;

		int deltaX = newX - LastMickeyX;
		int deltaY = newY - LastMickeyY;

		LastMickeyY = newX;
		LastMickeyY = newY;

		return new IntegerPoint(deltaX, deltaY);
	}

	public void SetExclusionArea(int x1, int y1, int x2, int y2)
	{
		if (!IsInitialized)
			return;

		ExclusionArea.X1 = x1;
		ExclusionArea.Y1 = y1;
		ExclusionArea.X2 = x2;
		ExclusionArea.Y2 = y2;

		if (ExclusionArea.Contains(PointerX, PointerY))
		{
			UpdateHardwareTextPointer();
			PointerVisibleChanged?.Invoke();
		}
	}

	public void ClearExclusionArea()
	{
		ExclusionArea = new IntegerRect(-1, -1, -1, -1);

		if (IsExcluded)
		{
			UpdateHardwareTextPointer();
			PointerVisibleChanged?.Invoke();

			IsExcluded = false;
		}
	}

	public const int StateBufferSize = 207;

	public byte[] CreateStateBuffer()
	{
		return new byte[StateBufferSize];
	}

	public byte[] SerializeState()
	{
		var buffer = CreateStateBuffer();

		if (IsInitialized)
		{
			var stream = new MemoryStream(buffer);

			var writer = new BinaryWriter(stream);

			writer.Write(_displayPageNumber);

			writer.Write(LastMickeyX);
			writer.Write(LastMickeyY);

			writer.Write(_width);
			writer.Write(_height);

			LeftButton.SerializeTo(writer);
			MiddleButton.SerializeTo(writer);
			RightButton.SerializeTo(writer);

			writer.Write(Bounds.X1);
			writer.Write(Bounds.Y1);
			writer.Write(Bounds.X2);
			writer.Write(Bounds.Y2);

			writer.Write(ExclusionArea.X1);
			writer.Write(ExclusionArea.Y1);
			writer.Write(ExclusionArea.X2);
			writer.Write(ExclusionArea.Y2);

			writer.Write(IsExcluded);

			writer.Write(TextPointerEnableSoftware);
			writer.Write(TextPointerCharacterMask);
			writer.Write(TextPointerCharacterInvert);
			writer.Write(TextPointerAttributeMask);
			writer.Write(TextPointerAttributeInvert);
			writer.Write(TextPointerHardwareCursorStartScan);
			writer.Write(TextPointerHardwareCursorEndScan);

			writer.Write(PointerShape);
			writer.Write(PointerShapeMask);
			writer.Write(PointerHotSpotX);
			writer.Write(PointerHotSpotY);

			writer.Flush();

			stream.Flush();

			if (stream.Position != StateBufferSize)
				throw new Exception("Internal error: Serialized data was not the expected size");
		}

		return buffer;
	}

	public void DeserializeState(byte[] state)
	{
		if (!IsInitialized)
			return;

		var stream = new MemoryStream(state);

		var reader = new BinaryReader(stream);

		_displayPageNumber = reader.ReadInt32();

		LastMickeyX = reader.ReadInt32();
		LastMickeyY = reader.ReadInt32();

		_width = reader.ReadInt32();
		_height = reader.ReadInt32();

		LeftButton.DeserializeFrom(reader);
		MiddleButton.DeserializeFrom(reader);
		RightButton.DeserializeFrom(reader);

		Bounds.X1 = reader.ReadInt32();
		Bounds.Y1 = reader.ReadInt32();
		Bounds.X2 = reader.ReadInt32();
		Bounds.Y2 = reader.ReadInt32();

		ExclusionArea.X1 = reader.ReadInt32();
		ExclusionArea.Y1 = reader.ReadInt32();
		ExclusionArea.X2 = reader.ReadInt32();
		ExclusionArea.Y2 = reader.ReadInt32();

		IsExcluded = reader.ReadBoolean();

		TextPointerEnableSoftware = reader.ReadBoolean();
		TextPointerCharacterMask = reader.ReadByte();
		TextPointerCharacterInvert = reader.ReadByte();
		TextPointerAttributeMask = reader.ReadByte();
		TextPointerAttributeInvert = reader.ReadByte();
		TextPointerHardwareCursorStartScan = reader.ReadByte();
		TextPointerHardwareCursorEndScan = reader.ReadByte();

		PointerShape = reader.ReadBytes(32);
		PointerShapeMask = reader.ReadBytes(32);
		PointerHotSpotX = reader.ReadInt32();
		PointerHotSpotY = reader.ReadInt32();

		if (stream.Position != StateBufferSize)
			throw new Exception("Internal error: Did not read the expected number of bytes");
	}
}
