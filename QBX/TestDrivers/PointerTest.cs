using QBX.Firmware;
using QBX.Hardware;
using System;
using System.Threading;

namespace QBX.TestDrivers;

public class PointerTest(Machine machine) : HostedProgram
{
	public override bool EnableMainLoop => true;

	public override void Run(CancellationToken cancellationToken)
	{
		machine.VideoFirmware.SetMode(0x6);

		var visual = new GraphicsLibrary_1bppPacked(machine);

		double a = 0;
		int waveAttribute = 1;

		int baseRadius = (int)(0.45 * visual.Height);

		int pointerVisibilityCounter = 0;

		int mx = visual.Width / 2;
		int my = visual.Height / 2;

		visual.ShowPointer();

		while (true)
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

			visual.MovePointer(
				(int)(mx + baseRadius * Math.Cos(pointerAngle)),
				(int)(my + baseRadius * Math.Sin(pointerAngle)));

			//pointerVisibilityCounter++;

			if (pointerVisibilityCounter == 40)
				visual.HidePointer();
			else if (pointerVisibilityCounter == 80)
			{
				visual.ShowPointer();
				pointerVisibilityCounter = 0;
			}

			Thread.Sleep(10);
		}
	}
}
