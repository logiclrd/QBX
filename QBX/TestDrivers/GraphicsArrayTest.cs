using System;
using System.Threading;
using System.Threading.Tasks;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.TestDrivers;

public class GraphicsArrayTest(Machine machine) : HostedProgram
{
	public override bool EnableMainLoop => true;

	public int TestSCREEN = 13;

	public int TextColumns = 80;
	public int TextRows = 25;
	public bool Use8x14Font = false;

	public bool Test40ColumnsWithoutHalfDotClock = false;

	public const int CGAPalette = 1;
	public const bool CGAPaletteHighIntensity = false;

	public override void Run(CancellationToken cancellationToken)
	{
		bool suppressDeadCodeWarning = EnableMainLoop;

		Task.Run(
			async () =>
			{
				if (TestSCREEN == 0)
				{
					int ModeNumber = (TextColumns == 40) ? 1 : 3;

					machine.VideoFirmware.SetMode(ModeNumber);

					if (Use8x14Font)
					{
						machine.VideoFirmware.SetCharacterRows(43);
						machine.VideoFirmware.SetCharacterRows(25);
					}
					else if (TextRows != 25)
						machine.VideoFirmware.SetCharacterRows(TextRows);

					if (Test40ColumnsWithoutHalfDotClock)
					{
						machine.GraphicsArray.Sequencer.Registers[GraphicsArray.SequencerRegisters.ClockingMode] =
							unchecked((byte)(
								machine.GraphicsArray.Sequencer.Registers[GraphicsArray.SequencerRegisters.ClockingMode] &
								~GraphicsArray.SequencerRegisters.ClockingMode_DotClockHalfRate));

						machine.GraphicsArray.AttributeController.Registers.OverscanPaletteIndex = 13;
					}

					if (Test40ColumnsWithoutHalfDotClock || !Test40ColumnsWithoutHalfDotClock)
						return;

					var library = new TextLibrary(machine);

					library.WriteTextAt(0, 0, "This is a ");

					int xx = library.CursorX;

					int bg = 0;

					while (true)
					{
						if (bg == 0)
						{
							machine.GraphicsArray.AttributeController.Registers[GraphicsArray.AttributeControllerRegisters.ModeControl]
								^= GraphicsArray.AttributeControllerRegisters.ModeControl_BlinkEnable;
						}

						for (int y = 0; y < 4; y++)
							for (int x = 0; x < 4; x++)
							{
								library.SetAttributes(15 - (y * 4 + x), bg);
								library.WriteTextAt(xx + x * 5, y, "test");
							}

						await Task.Delay(350);

						bg = (bg + 1) & 15;
					}
				}
				else
				{
					int ModeNumber =
						TestSCREEN switch
						{
							1 => 5,
							2 => 6,
							7 => 0xD,
							12 => 0x12,
							13 => 0x13,

							_ => -1
						};

					machine.VideoFirmware.SetMode(ModeNumber);

					if ((TestSCREEN == 1) || suppressDeadCodeWarning)
						machine.VideoFirmware.LoadCGAPalette(CGAPalette, CGAPaletteHighIntensity);

					var library =
						TestSCREEN switch
						{
							1 => new GraphicsLibrary_2bppInterleaved(machine),
							2 => new GraphicsLibrary_1bppPacked(machine),
							7 => new GraphicsLibrary_4bppPlanar(machine),
							12 => new GraphicsLibrary_4bppPlanar(machine),
							13 => new GraphicsLibrary_8bppFlat(machine),

							_ => default(GraphicsLibrary) ?? throw new NotImplementedException()
						};

					int MaxC =
						(TestSCREEN == 1) ? 3 :
						(TestSCREEN == 2) ? 1 :
						(TestSCREEN == 13) ? 255 : 15;

					int c = MaxC;

					var rnd = new Random(1234);

					while (true)
					{
						for (int i = 0; i < Math.Max(1, 15 / MaxC); i++)
						{
							int x = rnd.Next(-100, library.Width + 100);
							int y = rnd.Next(-100, library.Height + 100);

							int rx = rnd.Next(50, 150);
							int ry = rnd.Next(50, 150);

							double start = rnd.NextDouble() * (2 * Math.PI);
							double end = rnd.NextDouble() * (2 * Math.PI);

							library.Ellipse(x, y, rx, ry, start, end, true, true, c);

							c = (c % MaxC) + 1;
						}

						if (c == MaxC)
						{
							await Task.Delay(350);

							for (int i = 1; i <= 10; i++)
							{
								for (int j = 0; j < 6; j++)
								{
									library.ScrollUp(i);

									await Task.Delay(100);
								}
							}

							library.Clear();
						}
					}
				}
			});
	}
}
