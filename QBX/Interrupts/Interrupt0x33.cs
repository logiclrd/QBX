using System;

using QBX.Hardware;

namespace QBX.Interrupts;

// Based on the reference information in TECH Help! by Flambeaux Software

public class Interrupt0x33(Machine machine) : InterruptHandler
{
	enum Function : ushort
	{
		ResetQuery                          = 0x0000,
		ShowMousePointer                    = 0x0001,
		HideMousePointer                    = 0x0002,
		QueryPositionAndButtonStatus        = 0x0003,
		SetMousePointerPosition             = 0x0004,
		QueryButtonPressedCounter           = 0x0005,
		QueryButtonReleasedCounter          = 0x0006,
		SetHorizontalRange                  = 0x0007,
		SetVerticalRange                    = 0x0008,
		SetGraphicsPointerShape             = 0x0009,
		SetTextPointerMask                  = 0x000A,
		QueryMotionDistance                 = 0x000B,
		SetMouseEventHandler                = 0x000C, // not supported
		EnableLightPenEmulation             = 0x000D, // not supported
		DisableLightPenEmulation            = 0x000E, // not supported
		SetMouseSpeed                       = 0x000F, // not supported
		SetExclusionArea                    = 0x0010,
		SetSpeedDoublingThreshold           = 0x0013, // not supported
		ExchangeMouseEventHandler           = 0x0014, // not supported
		QuerySizeOfMouseStatusBuffer        = 0x0015,
		SaveMouseStatus                     = 0x0016,
		RestoreMouseStatus                  = 0x0017,
		InstallMouseAndKeyEventHandler      = 0x0018, // not supported
		GetAddressOfMouseAndKeyEventHandler = 0x0019, // not supported
		SetMouseSensitivity                 = 0x001A, // not supported
		QueryMouseSensitivity               = 0x001B,
		SetMouseInterruptRate               = 0x001C, // not supported
		SetDisplayPage                      = 0x001D,
		QueryActiveDisplayPage              = 0x001E,
		DeactivateMouseDriver               = 0x001F,
		ActivateMouseDriver                 = 0x0020, // no-op
		ResetMouseDriver                    = 0x0021,
		SetLanguageForMessages              = 0x0022, // no-op
		GetLanguageNumber                   = 0x0023,
		QueryMouseAndDriverInformation      = 0x0024,
	}

	public override Registers Execute(Registers input)
	{
		var function = (Function)input.AX;

		var result = input;

		switch (function)
		{
			case Function.ResetQuery:
			case Function.ResetMouseDriver:
			{
				machine.MouseDriver.MovePointer(machine.MouseDriver.PointerMaximumX / 2, machine.MouseDriver.PointerMaximumY / 2);

				result.AX = 0xFFFF; // installed
				result.BX = 3; // number of installed buttons

				break;
			}
			case Function.ShowMousePointer: machine.MouseDriver.ShowPointer(); break;
			case Function.HideMousePointer: machine.MouseDriver.HidePointer(); break;
			case Function.QueryPositionAndButtonStatus:
			{
				result.BX = unchecked((ushort)(
					(machine.MouseDriver.LeftButton.IsPressed ? 1 : 0) |
					(machine.MouseDriver.RightButton.IsPressed ? 2 : 0) |
					(machine.MouseDriver.MiddleButton.IsPressed ? 4 : 0)));
				result.CX = unchecked((ushort)machine.MouseDriver.PointerX);
				result.DX = unchecked((ushort)machine.MouseDriver.PointerY);
				break;
			}
			case Function.SetMousePointerPosition:
			{
				machine.MouseDriver.MovePointer(input.CX, input.DX);
				break;
			}
			case Function.QueryButtonPressedCounter:
			{
				var button =
					input.BX switch
					{
						0 => machine.MouseDriver.LeftButton,
						1 => machine.MouseDriver.RightButton,
						2 => machine.MouseDriver.MiddleButton,

						_ => default
					};

				result.AX = unchecked((ushort)(
					(machine.MouseDriver.LeftButton.IsPressed ? 1 : 0) |
					(machine.MouseDriver.RightButton.IsPressed ? 2 : 0) |
					(machine.MouseDriver.MiddleButton.IsPressed ? 4 : 0)));

				if (button != null)
				{
					result.BX = unchecked((ushort)button.ClickCounter.Count);
					result.CX = unchecked((ushort)button.ClickCounter.LastX);
					result.DX = unchecked((ushort)button.ClickCounter.LastY);
					button.ClickCounter.Count = 0;
				}

				break;
			}
			case Function.QueryButtonReleasedCounter:
			{
				var button =
					input.BX switch
					{
						0 => machine.MouseDriver.LeftButton,
						1 => machine.MouseDriver.RightButton,
						2 => machine.MouseDriver.MiddleButton,

						_ => default
					};

				result.AX = unchecked((ushort)(
					(machine.MouseDriver.LeftButton.IsPressed ? 1 : 0) |
					(machine.MouseDriver.RightButton.IsPressed ? 2 : 0) |
					(machine.MouseDriver.MiddleButton.IsPressed ? 4 : 0)));

				if (button != null)
				{
					result.BX = unchecked((ushort)button.ReleaseCounter.Count);
					result.CX = unchecked((ushort)button.ReleaseCounter.LastX);
					result.DX = unchecked((ushort)button.ReleaseCounter.LastY);
					button.ReleaseCounter.Count = 0;
				}

				break;
			}
			case Function.SetHorizontalRange:
			{
				machine.MouseDriver.ConstrainPointer(
					machine.MouseDriver.Bounds.X1,
					y1: input.CX,
					machine.MouseDriver.Bounds.X2,
					y2: input.DX);
				break;
			}
			case Function.SetVerticalRange:
			{
				machine.MouseDriver.ConstrainPointer(
					x1: input.CX,
					machine.MouseDriver.Bounds.Y1,
					x2: input.DX,
					machine.MouseDriver.Bounds.Y2);
				break;
			}
			case Function.SetGraphicsPointerShape:
			{
				int offset = (input.AsRegistersEx().ES << 4) + input.DX;

				byte[] data = new byte[64];

				for (int i = 0; i < data.Length; i++)
					data[i] = machine.MemoryBus[offset + i];

				var dataSpan = data.AsSpan();

				machine.MouseDriver.SetPointerShape(
					newPointerShape: dataSpan.Slice(0, 32),
					newPointerShapeMask: dataSpan.Slice(32, 32),
					hotSpotX: input.CX,
					hotSpotY: input.DX);

				break;
			}
			case Function.SetTextPointerMask:
			{
				if (input.BX == 0)
				{
					machine.MouseDriver.SetSoftwareTextPointer(
						characterMask: unchecked((byte)(input.CX & 0xFF)),
						characterInvert: unchecked((byte)(input.DX & 0xFF)),
						attributeMask: unchecked((byte)(input.CX >> 8)),
						attributeInvert: unchecked((byte)(input.DX >> 8)));
				}
				else
				{
					machine.MouseDriver.SetHardwareTextPointer(
						startScan: input.CX,
						endScan: input.DX);
				}

				break;
			}
			case Function.QueryMotionDistance:
			{
				var vector = machine.MouseDriver.GetMotionMickeys();

				result.CX = unchecked((ushort)vector.X);
				result.DX = unchecked((ushort)vector.Y);

				break;
			}
			case Function.SetExclusionArea:
			{
				machine.MouseDriver.SetExclusionArea(
					input.CX,
					input.DX,
					input.SI,
					input.DI);

				break;
			}
			case Function.QuerySizeOfMouseStatusBuffer:
			{
				var state = machine.MouseDriver.CreateStateBuffer();

				result.BX = (ushort)state.Length;

				break;

			}
			case Function.SaveMouseStatus:
			{
				var state = machine.MouseDriver.SerializeState();

				int offset = (input.AsRegistersEx().ES << 4) + input.DX;

				for (int i = 0; i < state.Length; i++)
					machine.MemoryBus[offset + i] = state[i];

				break;
			}
			case Function.RestoreMouseStatus:
			{
				var state = machine.MouseDriver.CreateStateBuffer();

				int offset = (input.AsRegistersEx().ES << 4) + input.DX;

				for (int i = 0; i < state.Length; i++)
					state[i] = machine.MemoryBus[offset + i];

				machine.MouseDriver.DeserializeState(state);

				break;
			}
			case Function.QueryMouseSensitivity:
			{
				result.BX = Mouse.MickeysPerPixel;
				result.CX = Mouse.MickeysPerPixel;
				result.DX = 65535; // speed-doubling threshold

				break;
			}
			case Function.SetDisplayPage:
			{
				machine.MouseDriver.SetDisplayPageNumber(input.BX);
				break;
			}
			case Function.QueryActiveDisplayPage:
			{
				result.BX = unchecked((ushort)machine.MouseDriver.DisplayPageNumber);
				break;
			}
			case Function.DeactivateMouseDriver:
			{
				result.AX = 0xFFFF; // "can't deactivate"
				break;
			}
			case Function.QueryMouseAndDriverInformation:
			{
				result.BX = 0x1201; // driver version -- "12.01", newer than any actual MOUSE.SYS
				result.CX = 0x0400; // mouse type 4 (PS/2), IRQ 0 (PS/2)
				break;
			}
			case Function.GetLanguageNumber:
			{
				result.BX = 0; // English
				break;
			}
		}

		return result;
	}
}
