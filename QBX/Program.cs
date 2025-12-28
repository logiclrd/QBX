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

		video.SetMode(0x13);

		var library = new GraphicsLibrary_8bppFlat(machine.GraphicsArray);

		Task.Run(
			async () =>
			{
				double start = 1;
				double end = 0.5;
				int c = 15;

				var rnd = new Random(1234);

				while (true)
				{
					if (c == 1)
					{
						await Task.Delay(15);
						library.Clear();
					}

						int x = rnd.Next(-100, 420);
					int y = rnd.Next(-100, 300);

					int rx = rnd.Next(50, 150);
					int ry = rnd.Next(50, 150);

					start = rnd.NextDouble() * (2 * Math.PI);
					end = rnd.NextDouble() * (2 * Math.PI);

					try
					{
						library.Ellipse(x, y, rx, ry, -start, -end, c);
					}
					catch
					{
						Console.WriteLine();
					}

					c = (c % 15) + 1;
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
