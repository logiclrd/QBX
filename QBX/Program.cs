namespace QBX;

using BdfFontParser;
using BdfFontParser.Models;
using QBX.Hardware;

using SDL3;
using System.Reflection;

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

		var machine = new Machine();

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

			if (machine.Display.UpdateResolution(ref textureWidth, ref textureHeight))
			{
				if (texture != default)
					SDL.DestroyTexture(texture);

				texture = SDL.CreateTexture(renderer, SDL.PixelFormat.BGRA8888, SDL.TextureAccess.Streaming, textureWidth, textureHeight);

				SDL.SetWindowSize(window, textureWidth, textureHeight);
			}

			machine.Display.Render(texture);

			SDL.RenderTexture(renderer, texture, default, default);
			SDL.RenderPresent(renderer);
		}

		return 0;
	}
}
