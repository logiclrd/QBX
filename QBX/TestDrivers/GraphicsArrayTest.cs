using QBX.Firmware;
using QBX.Hardware;

namespace QBX.TestDrivers;

public class GraphicsArrayTest(Machine machine, Video video) : HostedProgram
{
	public override bool EnableMainLoop => true;

	public int TestSCREEN = 12;

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

					video.SetMode(ModeNumber);

					if (Use8x14Font)
					{
						video.SetCharacterRows(43);
						video.SetCharacterRows(25);
					}
					else if (TextRows != 25)
						video.SetCharacterRows(TextRows);

					if (Test40ColumnsWithoutHalfDotClock)
					{
						machine.GraphicsArray.Sequencer.Registers[GraphicsArray.SequencerRegisters.ClockingMode] =
							unchecked((byte)(
								machine.GraphicsArray.Sequencer.Registers[GraphicsArray.SequencerRegisters.ClockingMode] &
								~GraphicsArray.SequencerRegisters.ClockingMode_DotClockHalfRate));

						machine.GraphicsArray.AttributeController.Registers.OverscanPaletteIndex = 13;
					}

					new DevelopmentEnvironment.Program(machine, video);

					if (Test40ColumnsWithoutHalfDotClock || !Test40ColumnsWithoutHalfDotClock)
						return;

					var library = new TextLibrary(machine.GraphicsArray);

					library.WriteAt(0, 0, "This is a ");

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
								library.WriteAt(xx + x * 5, y, "test");
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

					video.SetMode(ModeNumber);

					if ((TestSCREEN == 1) || suppressDeadCodeWarning)
						machine.GraphicsArray.LoadCGAPalette(CGAPalette, CGAPaletteHighIntensity);

					var library =
						TestSCREEN switch
						{
							1 => new GraphicsLibrary_2bppInterleaved(machine.GraphicsArray),
							2 => new GraphicsLibrary_1bppPacked(machine.GraphicsArray),
							7 => new GraphicsLibrary_4bppPlanar(machine.GraphicsArray),
							12 => new GraphicsLibrary_4bppPlanar(machine.GraphicsArray),
							13 => new GraphicsLibrary_8bppFlat(machine.GraphicsArray),

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
						if (c == 1)
						{
							await Task.Delay(350);
							library.Clear();
						}

						for (int i = 0; i < Math.Max(1, 15 / MaxC); i++)
						{
							int x = rnd.Next(-100, library.Width + 100);
							int y = rnd.Next(-100, library.Height + 100);

							int rx = rnd.Next(50, 150);
							int ry = rnd.Next(50, 150);

							double start = rnd.NextDouble() * (2 * Math.PI);
							double end = rnd.NextDouble() * (2 * Math.PI);

							library.Ellipse(x, y, rx, ry, start, end, c, true, true);

							c = (c % MaxC) + 1;
						}
					}
				}
			});
	}
}
