using System;
using System.Threading;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.TestDrivers;

public class PointerTest(Machine machine) : HostedProgram(machine)
{
	public override bool EnableMainLoop => true;

	public override void Run(CancellationToken cancellationToken)
	{
		machine.VideoFirmware.SetMode(0x12);

		machine.MouseDriver.Reset();

		var visual = new GraphicsLibrary_4bppPlanar(machine);

		double a = 0;
		int waveAttribute = 1;

		int baseRadius = (int)(0.45 * visual.Height);

		int pointerVisibilityCounter = 0;

		int mx = visual.Width / 2;
		int my = visual.Height / 2;

		machine.MouseDriver.ShowPointer();

		visual.EnablePointerAwareDrawing = true;

		if (visual.MaximumAttribute > 1)
			visual.BorderFill(10, 10, 1, 1);

		int dotWidth = machine.GraphicsArray.Sequencer.DotDoubling ? 2 : 1;
		int scanHeight = machine.GraphicsArray.CRTController.ScanDoubling ? 2 : 1;

		while (machine.KeepRunning)
		{
			a = a + 1;

			double pointerAngle = a * 0.01;
			double waveAngle = -a * 0.005;
			double wavePhase = a * 0.2005;
			double waveRadius = visual.Height * (0.45 + 0.02 * Math.Sin(wavePhase));

			waveAttribute = (waveAttribute & visual.MaximumAttribute) + 1;

			visual.PixelSet(
				(int)(mx + waveRadius * Math.Cos(waveAngle)),
				(int)(my + waveRadius * Math.Sin(waveAngle)),
				waveAttribute);

			machine.Mouse.NotifyPositionChanged(
				(int)(dotWidth * (mx + baseRadius * Math.Cos(pointerAngle))),
				(int)(scanHeight * (my + baseRadius * Math.Sin(pointerAngle))));

			//pointerVisibilityCounter++;

			if (pointerVisibilityCounter == 40)
				machine.MouseDriver.HidePointer();
			else if (pointerVisibilityCounter == 80)
			{
				machine.MouseDriver.ShowPointer();
				pointerVisibilityCounter = 0;
			}

			//Thread.Sleep(100);
			machine.Display.VerticalSync();
		}
	}
}
