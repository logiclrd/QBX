using System;
using System.Threading;
using System.Threading.Tasks;

using SDL3;

using QBX.Hardware;

namespace QBX;

class Program
{
	static int Main()
	{
		DebugExceptionHelper.Install();

		var machine = new Machine();

		var video = machine.VideoFirmware;

		HostedProgram program = new DevelopmentEnvironment.Program(machine);
		//HostedProgram program = new TestDrivers.GraphicsArrayTest(machine);

		if (!program.EnableMainLoop)
			program.Run(CancellationToken.None);
		else
		{
			var cancellationTokenSource = new CancellationTokenSource();

			var driverTask = new Task(() => program.Run(cancellationTokenSource.Token));

			if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Audio))
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

			var audioSpec =
				new SDL.AudioSpec()
				{
					Channels = 1,
					Format = SDL.AudioFormat.AudioU8,
					Freq = Speaker.SampleRate,
				};

			byte[] audioBuffer = new byte[8192];

			void AudioCallback(nint userData, IntPtr stream, int additionalAmount, int totalAmount)
			{
				while (additionalAmount > 0)
				{
					Span<byte> buffer = audioBuffer;

					if (buffer.Length > additionalAmount)
						buffer = buffer.Slice(0, additionalAmount);

					machine.Speaker.GetMoreSound(buffer);

					SDL.PutAudioStreamData(stream, audioBuffer, buffer.Length);

					additionalAmount -= buffer.Length;
				}
			}

			var audioStream = SDL.OpenAudioDeviceStream(
				SDL.AudioDeviceDefaultPlayback,
				audioSpec,
				AudioCallback,
				default);

			if (audioStream != default)
				SDL.ResumeAudioStreamDevice(audioStream);

			IntPtr texture = default;
			int textureWidth = -1;
			int textureHeight = -1;
			int widthScale = -1;
			int heightScale = -1;

			while (machine.KeepRunning)
			{
				while (SDL.PollEvent(out var evt))
				{
					var eventType = (SDL.EventType)evt.Type;

					if (eventType == SDL.EventType.Quit)
					{
						machine.KeepRunning = false;
						break;
					}

					if ((eventType == SDL.EventType.KeyDown) || (eventType == SDL.EventType.KeyUp))
						machine.Keyboard.HandleEvent(evt.Key);
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

				if (driverTask.Status == TaskStatus.Created)
					driverTask.Start();
			}

			cancellationTokenSource.Cancel();

			driverTask.Wait(TimeSpan.FromSeconds(5));
		}

		return machine.ExitCode;
	}
}
