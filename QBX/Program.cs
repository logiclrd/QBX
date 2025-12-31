namespace QBX;

using QBX.Firmware;
using QBX.Hardware;

using SDL3;

class Program
{
	static int Main()
	{
		if (!SDL.Init(SDL.InitFlags.Video))
		{
			Console.WriteLine("Failed to initialize SDL: {0}", SDL.GetError());
			return 1;
		}

		bool success = SDL.CreateWindowAndRenderer(
			"QBX",
			720,
			400,
			default,
			out var window,
			out var renderer);

		if (!success)
		{
			Console.WriteLine("Failed to create window and/or renderer: {0}", SDL.GetError());
			return 2;
		}

		IntPtr texture = default;
		int textureWidth = -1;
		int textureHeight = -1;
		int widthScale = -1;
		int heightScale = -1;

		var machine = new Machine();

		var video = new Video(machine);

		bool suppressDeadCodeWarning = !success;

		int TestSCREEN = 0;

		int TextColumns = 80;
		int TextRows = 25;
		bool Use8x14Font = false;

		bool Test40ColumnsWithoutHalfDotClock = false;

		const int CGAPalette = 1;
		const bool CGAPaletteHighIntensity = false;

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
						(TestSCREEN == 1) ? 5 :
						(TestSCREEN == 2) ? 6 :
						(TestSCREEN == 12) ? 0x12 :
						(TestSCREEN == 13) ? 0x13 : -1;

					video.SetMode(ModeNumber);

					if ((TestSCREEN == 1) || suppressDeadCodeWarning)
						machine.GraphicsArray.LoadCGAPalette(CGAPalette, CGAPaletteHighIntensity);

					var library =
						TestSCREEN switch
						{
							1 => new GraphicsLibrary_2bppInterleaved(machine.GraphicsArray),
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

		bool keepRunning = true;

		while (keepRunning)
		{
			while (SDL.PollEvent(out var evt))
			{
				if ((SDL.EventType)evt.Type == SDL.EventType.Quit)
				{
					keepRunning = false;
					break;
				}
			}

			if (machine.Display.UpdateResolution(ref textureWidth, ref textureHeight, ref widthScale, ref heightScale))
			{
				if (texture != default)
					SDL.DestroyTexture(texture);

				texture = SDL.CreateTexture(renderer, SDL.PixelFormat.BGRA8888, SDL.TextureAccess.Streaming, textureWidth, textureHeight);

				SDL.SetTextureScaleMode(texture, SDL.ScaleMode.Nearest);

				SDL.SetWindowSize(window, textureWidth * widthScale, textureHeight * heightScale);
			}

			machine.Display.Render(texture);

			SDL.RenderTexture(renderer, texture, default, default);
			SDL.RenderPresent(renderer);
		}

		return 0;
	}
}
