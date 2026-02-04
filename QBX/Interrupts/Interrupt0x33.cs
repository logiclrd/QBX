using System;

using QBX.Hardware;

namespace QBX.Interrupts;

// Based on Microsoft Mouse Programmers Reference from 1989.
// Originally based on the reference information in TECH Help! by Flambeaux Software

public class Interrupt0x33(Machine machine) : InterruptHandler
{
	public enum Function : ushort
	{
		MouseResetAndStatus                          = 0x0000,
		ShowCursor                                   = 0x0001,
		HideCursor                                   = 0x0002,
		GetButtonStatusAndMousePosition              = 0x0003,
		SetMouseCursorPosition                       = 0x0004,
		GetButtonPressInformation                    = 0x0005,
		GetButtonReleaseInformation                  = 0x0006,
		SetMinimumAndMaximumHorizontalCursorPosition = 0x0007,
		SetMinimumAndMaximumVerticalCursorPosition   = 0x0008,
		SetGraphicsCursorBlock                       = 0x0009,
		SetTextCursor                                = 0x000A,
		ReadMouseMotionCounters                      = 0x000B,
		SetInterruptSubroutineCallMaskAndAddress     = 0x000C, // not supported
		LightPenEmulationModeOn                      = 0x000D,
		LightPenEmulationModeOff                     = 0x000E,
		SetMickeyPixelRatio                          = 0x000F, // not supported
		ConditionalOff                               = 0x0010,
		SetDoubleSpeedThreshold                      = 0x0013, // not supported
		SwapInterruptSubroutines                     = 0x0014, // not supported
		GetMouseDriverStateStorageRequirements       = 0x0015,
		SaveMouseDriverState                         = 0x0016,
		RestoreMouseDriverState                      = 0x0017,
		SetAlternateSubroutineCallMaskAndAddress     = 0x0018, // not supported
		GetUserAlternateInterruptAddress             = 0x0019, // not supported
		SetMouseSensitivity                          = 0x001A, // not supported
		GetMouseSensitivity                          = 0x001B,
		SetMouseInterruptRate                        = 0x001C, // not supported
		SetCRTPageNumber                             = 0x001D,
		GetCRTPageNumber                             = 0x001E,
		DisableMouseDriver                           = 0x001F,
		EnableMouseDriver                            = 0x0020, // no-op
		SoftwareReset                                = 0x0021,
		SetLanguageForMessages                       = 0x0022, // no-op
		GetLanguageNumber                            = 0x0023,
		GetDriverVersionMouseTypeAndIRQNumber        = 0x0024,
	}

	public override Registers Execute(Registers input)
	{
		var function = (Function)input.AX;

		var result = input;

		switch (function)
		{
			case Function.MouseResetAndStatus:
			case Function.SoftwareReset:
			{
				machine.MouseDriver.MovePointer(machine.MouseDriver.PointerMaximumX / 2, machine.MouseDriver.PointerMaximumY / 2);

				result.AX = 0xFFFF; // installed
				result.BX = 3; // number of installed buttons

				break;
			}
			case Function.ShowCursor: machine.MouseDriver.ShowPointer(); break;
			case Function.HideCursor: machine.MouseDriver.HidePointer(); break;
			case Function.GetButtonStatusAndMousePosition:
			{
				result.BX = unchecked((ushort)(
					(machine.MouseDriver.LeftButton.IsPressed ? 1 : 0) |
					(machine.MouseDriver.RightButton.IsPressed ? 2 : 0) |
					(machine.MouseDriver.MiddleButton.IsPressed ? 4 : 0)));
				result.CX = unchecked((ushort)machine.MouseDriver.PointerX);
				result.DX = unchecked((ushort)machine.MouseDriver.PointerY);
				break;
			}
			case Function.SetMouseCursorPosition:
			{
				machine.MouseDriver.MovePointer(input.CX, input.DX);
				break;
			}
			case Function.GetButtonPressInformation:
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
			case Function.GetButtonReleaseInformation:
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
			case Function.SetMinimumAndMaximumHorizontalCursorPosition:
			{
				machine.MouseDriver.ConstrainPointer(
					machine.MouseDriver.Bounds.X1,
					y1: input.CX,
					machine.MouseDriver.Bounds.X2,
					y2: input.DX);
				break;
			}
			case Function.SetMinimumAndMaximumVerticalCursorPosition:
			{
				machine.MouseDriver.ConstrainPointer(
					x1: input.CX,
					machine.MouseDriver.Bounds.Y1,
					x2: input.DX,
					machine.MouseDriver.Bounds.Y2);
				break;
			}
			case Function.SetGraphicsCursorBlock:
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
			case Function.SetTextCursor:
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
			case Function.ReadMouseMotionCounters:
			{
				var vector = machine.MouseDriver.GetMotionMickeys();

				result.CX = unchecked((ushort)vector.X);
				result.DX = unchecked((ushort)vector.Y);

				break;
			}
			case Function.LightPenEmulationModeOn:
			{
				machine.MouseDriver.EnableLightPenEmulation();
				break;
			}
			case Function.LightPenEmulationModeOff:
			{
				machine.MouseDriver.DisableLightPenEmulation();
				break;
			}
			case Function.ConditionalOff:
			{
				machine.MouseDriver.SetExclusionArea(
					input.CX,
					input.DX,
					input.SI,
					input.DI);

				break;
			}
			case Function.GetMouseDriverStateStorageRequirements:
			{
				var state = machine.MouseDriver.CreateStateBuffer();

				result.BX = (ushort)state.Length;

				break;

			}
			case Function.SaveMouseDriverState:
			{
				var state = machine.MouseDriver.SerializeState();

				int offset = (input.AsRegistersEx().ES << 4) + input.DX;

				for (int i = 0; i < state.Length; i++)
					machine.MemoryBus[offset + i] = state[i];

				break;
			}
			case Function.RestoreMouseDriverState:
			{
				var state = machine.MouseDriver.CreateStateBuffer();

				int offset = (input.AsRegistersEx().ES << 4) + input.DX;

				for (int i = 0; i < state.Length; i++)
					state[i] = machine.MemoryBus[offset + i];

				machine.MouseDriver.DeserializeState(state);

				break;
			}
			case Function.GetMouseSensitivity:
			{
				result.BX = Mouse.MickeysPerPixel;
				result.CX = Mouse.MickeysPerPixel;
				result.DX = 65535; // speed-doubling threshold

				break;
			}
			case Function.SetCRTPageNumber:
			{
				machine.MouseDriver.SetDisplayPageNumber(input.BX);
				break;
			}
			case Function.GetCRTPageNumber:
			{
				result.BX = unchecked((ushort)machine.MouseDriver.DisplayPageNumber);
				break;
			}
			case Function.DisableMouseDriver:
			{
				result.AX = 0xFFFF; // "can't deactivate"
				break;
			}
			case Function.GetDriverVersionMouseTypeAndIRQNumber:
			{
				result.BX = 0x0626; // driver version -- 6.26 corresponds to the set of implemented functions per Ralf Brown's interrupt list
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
