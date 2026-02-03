using System;
using System.Runtime.InteropServices;
using System.Threading;

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

			var driverThread = new Thread(() => program.Run(cancellationTokenSource.Token));

			driverThread.IsBackground = false;
			driverThread.Name = "Hosted Program";

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

			program.WindowIconChanged +=
				(newIcon) =>
				{
					IntPtr translatedIcon = IntPtr.Zero;
					GCHandle pin = default;

					try
					{
						pin = GCHandle.Alloc(newIcon.Pixels, GCHandleType.Pinned);

						translatedIcon = SDL.CreateSurfaceFrom(newIcon.Width, newIcon.Height, SDL.PixelFormat.ARGB8888, pin.AddrOfPinnedObject(), newIcon.Width * 4);

						SDL.SetWindowIcon(window, translatedIcon);
					}
					finally
					{
						if (translatedIcon != IntPtr.Zero)
							SDL.DestroySurface(translatedIcon);

						if (pin.IsAllocated)
							pin.Free();
					}
				};

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

			// Explicitly capture the delegate, because the only references captured
			// within SDL are unmanaged, and the delegate is at risk of being garbage
			// collected.
			SDL.AudioStreamCallback audioCallback = AudioCallback;

			var audioStream = SDL.OpenAudioDeviceStream(
				SDL.AudioDeviceDefaultPlayback,
				audioSpec,
				audioCallback,
				default);

			if (audioStream != default)
				SDL.ResumeAudioStreamDevice(audioStream);

			machine.Mouse.WarpMouse +=
				() =>
				{
					try
					{
						SDL.WarpMouseInWindow(window, machine.Mouse.X, machine.Mouse.Y);
					}
					catch { }
				};

			machine.Mouse.ResetGeometryOfSpace +=
				() =>
				{
					try
					{
						SDL.SetWindowMouseRect(window, 0);
					}
					catch { }
				};

			machine.Mouse.ChangeGeometryOfSpace +=
				() =>
				{
					SDL.Rect mouseRect = new SDL.Rect();

					mouseRect.X = machine.Mouse.Bounds.X1;
					mouseRect.Y = machine.Mouse.Bounds.Y1;
					mouseRect.W = machine.Mouse.Bounds.X2 - machine.Mouse.Bounds.X1 + 1;
					mouseRect.H = machine.Mouse.Bounds.Y2 - machine.Mouse.Bounds.Y1 + 1;

					try
					{
						SDL.SetWindowMouseRect(window, mouseRect);
					}
					catch { }
				};

			Action? engageMouse = null;

			engageMouse =
				() =>
				{
					SDL.HideCursor();
					machine.MouseDriver.Initialized -= engageMouse;
				};

			machine.MouseDriver.Initialized += engageMouse;

			IntPtr texture = default;
			int textureWidth = -1;
			int textureHeight = -1;
			int physicalWidth = -1;
			int physicalHeight = -1;

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

					switch (eventType)
					{
						case SDL.EventType.KeyDown:
						case SDL.EventType.KeyUp:
							machine.Keyboard.HandleEvent(evt.Key);
							break;

						case SDL.EventType.MouseMotion:
							machine.Mouse.NotifyPositionChanged(evt.Motion.X, evt.Motion.Y);
							break;

						case SDL.EventType.MouseButtonDown:
						case SDL.EventType.MouseButtonUp:
							machine.Mouse.NotifyButtonChanged(
								evt.Button.Button switch
								{
									SDL.ButtonLeft => MouseButton.Left,
									SDL.ButtonMiddle => MouseButton.Middle,
									SDL.ButtonRight => MouseButton.Right,

									_ => default
								},
								isPressed: evt.Button.Down);

							break;
					}
				}

				if (machine.Display.UpdateResolution(ref textureWidth, ref textureHeight, ref physicalWidth, ref physicalHeight))
				{
					if (texture != default)
						SDL.DestroyTexture(texture);

					texture = SDL.CreateTexture(renderer, SDL.PixelFormat.BGRA8888, SDL.TextureAccess.Streaming, textureWidth, textureHeight);

					SDL.SetTextureScaleMode(texture, SDL.ScaleMode.Nearest);

					SDL.SetWindowSize(window, physicalWidth, physicalHeight);

					machine.Mouse.NotifyPhysicalSizeChanged(physicalWidth, physicalHeight);
				}

				machine.Display.Render(texture);

				SDL.RenderTexture(renderer, texture, default, default);
				SDL.RenderPresent(renderer);

				if (driverThread.ThreadState == ThreadState.Unstarted)
					driverThread.Start();
			}

			cancellationTokenSource.Cancel();

			driverThread.Join(TimeSpan.FromSeconds(5));

			SDL.PauseAudioStreamDevice(audioStream);

			// Keep the reference alive to the end of the scope.
			audioCallback.ToString();
		}

		Environment.Exit(machine.ExitCode);
		return machine.ExitCode;
	}
}
