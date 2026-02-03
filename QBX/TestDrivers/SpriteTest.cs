using System;
using System.Threading;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.TestDrivers;

public class SpriteTest(Machine machine) : HostedProgram(machine)
{
	public override bool EnableMainLoop => true;

	public override void Run(CancellationToken cancellationToken)
	{
		Run2bpp();
	}

	void Run2bpp()
	{
		machine.VideoFirmware.SetMode(0x5);

		var visual = new GraphicsLibrary_2bppInterleaved(machine);

		var buffer = new byte[100];

		for (int j = 0; j < 15; j++)
		{
			visual.Clear();
			visual.PixelSet(j & 7, j / 8, 1);

			visual.GetSprite(0, 0, 8, 2, buffer);
		}

		Random rnd = new Random();

		for (int i = 0; i < 20; i++)
			for (int j = 0; j < 8; j++)
				visual.PixelSet(i, j, (rnd.Next() & 1) + 2);

		for (int i = 0; i < 8; i++)
		{
			visual.PixelSet(i, i, 1);
			visual.PixelSet(8 + i / 2, i, 15);
		}

		Thread.Sleep(2000);

		visual.GetSprite(0, 0, 16, 8, buffer);

		for (int i = 0; i < 20; i++)
			for (int j = 0; j < 8; j++)
				visual.PixelSet(i, j, rnd.Next() & 1);

		const int ZoomFactor = 5;

		for (int ii = 0; ii < 20 * ZoomFactor; ii++)
			for (int jj = 0; jj < 8 * ZoomFactor; jj++)
				visual.PixelSet(ii, jj + 100, visual.PixelGet(ii / ZoomFactor, jj / ZoomFactor));

		Thread.Sleep(2000);

		for (int i = 0; i < 8; i++)
		{
			visual.PutSprite(buffer, PutSpriteAction.PixelSet, i, 0);

			for (int ii = 0; ii < 60; ii++)
				for (int jj = 0; jj < 24; jj++)
					visual.PixelSet(ii, jj + 100, visual.PixelGet(ii / 3, jj / 3));

			Thread.Sleep(100);
		}
	}

	void Run4pp()
	{
		machine.VideoFirmware.SetMode(0x12);

		var visual = new GraphicsLibrary_4bppPlanar(machine);

		Random rnd = new Random();

		for (int i = 0; i < 20; i++)
			for (int j = 0; j < 8; j++)
				visual.PixelSet(i, j, (rnd.Next() & 1) + 2);

		for (int i = 0; i < 8; i++)
		{
			visual.PixelSet(i, i, 1);
			visual.PixelSet(8 + i / 2, i, 15);
		}

		var buffer = new byte[100];

		Thread.Sleep(2000);

		visual.GetSprite(0, 0, 16, 8, buffer);

		for (int i = 0; i < 20; i++)
			for (int j = 0; j < 8; j++)
				visual.PixelSet(i, j, rnd.Next() & 1);

		const int ZoomFactor = 5;

		for (int ii = 0; ii < 20 * ZoomFactor; ii++)
			for (int jj = 0; jj < 8 * ZoomFactor; jj++)
				visual.PixelSet(ii, jj + 100, visual.PixelGet(ii / ZoomFactor, jj / ZoomFactor));

		Thread.Sleep(2000);

		for (int i = 0; i < 8; i++)
		{
			visual.PutSprite(buffer, PutSpriteAction.PixelSet, i, 0);

			for (int ii = 0; ii < 60; ii++)
				for (int jj = 0; jj < 24; jj++)
					visual.PixelSet(ii, jj + 100, visual.PixelGet(ii / 3, jj / 3));

			Thread.Sleep(100);
		}
	}
}
