using System;
using System.Diagnostics;
using System.Threading;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.TestDrivers;

public class BorderFillTest(Machine machine) : HostedProgram
{
	public override bool EnableMainLoop => true;

	void T1_SmallEllipse(GraphicsLibrary visual)
	{
		visual.Ellipse(160, 100, 3, 3, 0, 0, false, false, 1);
	}

	void T2_LargeEllipse(GraphicsLibrary visual)
	{
		visual.Ellipse(160, 100, 50, 25, 0, 0, false, false, 1);
	}

	void T3_WedgeToLeftEdge(GraphicsLibrary visual)
	{
		visual.Line(0, 50, 200, 100, 1);
		visual.Line(0, 150, 200, 100, 1);
	}

	void T4_EllipseWithCutouts(GraphicsLibrary visual)
	{
		visual.Ellipse(160, 100, 50, 25, 0, 0, false, false, 1);
		visual.Ellipse(130, 100, 15, 15, 0, 0, false, false, 1);
		visual.Ellipse(190, 100, 15, 15, 0, 0, false, false, 1);
	}

	void T5_BoxWithPixels(GraphicsLibrary visual)
	{
		visual.Box(155, 90, 165, 110, 1);

		visual.PixelSet(158, 100, 1);
		visual.PixelSet(158, 99, 1);

		visual.PixelSet(158, 97, 1);
		visual.PixelSet(158, 96, 1);
	}

	void T6_RandomMaze(GraphicsLibrary visual)
	{
		Random rnd = new Random();

		var seed = rnd.Next();

		//seed = 916776031;
		seed = 396652144;

		Debug.WriteLine("SEED: " + seed);

		rnd = new Random(seed);

		visual.FillBox(0, 0, 319, 199, 1);

		for (int i = 0; i < 20; i++)
		{
			int x = rnd.Next(0, 320);
			int y = rnd.Next(0, 200);

			int dx = rnd.Next(50, 100);
			int dy = rnd.Next(50, 100);

			if ((rnd.Next() & 1) == 0)
				dx /= 20;
			else
				dy /= 20;

			visual.FillBox(x - dx, y - dy, x + dx, y + dy, 0);
		}

		visual.FillBox(150, 90, 170, 110, 0);
	}

	void T7_MultipleEntrancesInScan(GraphicsLibrary visual)
	{
		visual.Box(140, 80, 180, 120, 1);

		for (int i = 0; i < 3; i++)
			for (int j = 150; j <= 170; j += 10)
				visual.Line(j - 1, 95 - i, j + 1, 95 - i, 1);
	}

	void T8_TouchRightEdgeOfScreen(GraphicsLibrary visual)
	{
		visual.FillBox(0, 0, 319, 199, 1);
		visual.FillBox(150, 90, 319, 110, 0);
	}

	void T9_MultipleDiscoverDownWhenExtendingToTheRight(GraphicsLibrary visual)
	{
		visual.FillBox(0, 0, 319, 199, 1);
		visual.FillBox(150, 90, 170, 110, 0);
		visual.FillBox(150, 90, 250, 95, 0);
		visual.FillBox(160, 96, 165, 98, 1);
		visual.FillBox(200, 90, 210, 130, 0);
		visual.FillBox(230, 90, 240, 130, 0);
	}

	void T10_MeetUpWithExtensionScan(GraphicsLibrary visual)
	{
		visual.FillBox(0, 0, 319, 199, 1);
		visual.FillBox(155, 95, 175, 105, 0);
		visual.Line(130, 101, 160, 101, 0);
		visual.FillBox(130, 90, 135, 101, 0);
		visual.FillBox(140, 90, 145, 101, 0);
		visual.Line(130, 90, 160, 90, 0);
		visual.FillBox(155, 90, 160, 100, 0);
	}

	void T11_MiniMaze(GraphicsLibrary visual)
	{
		Random rnd = new Random();

		var seed = rnd.Next();

		//seed = 738594425;
		//seed = 302437658;

		Debug.WriteLine("SEED: " + seed);

		rnd = new Random(seed);

		visual.Box(150, 90, 170, 110, 1);

		for (int i = 0; i < 30; i++)
			visual.PixelSet(rnd.Next(150, 170), rnd.Next(90, 110), 1);
	}

	void T12_MediumMaze(GraphicsLibrary visual)
	{
		Random rnd = new Random();

		var seed = rnd.Next();

		//seed = 710843189;

		Debug.WriteLine("SEED: " + seed);

		rnd = new Random(seed);

		visual.Box(100, 90, 220, 110, 1);

		for (int i = 0; i < 500; i++)
			visual.PixelSet(rnd.Next(100, 220), rnd.Next(90, 110), 1);

		visual.PixelSet(160, 100, 0);
	}

	void DrawBoundary(GraphicsLibrary visual)
	{
		//T1_SmallEllipse(visual);
		//T2_LargeEllipse(visual);
		//T3_WedgeToLeftEdge(visual);
		//T4_EllipseWithCutouts(visual);
		//T5_BoxWithPixels(visual);
		//T6_RandomMaze(visual);
		//T7_MultipleEntrancesInScan(visual);
		//T8_TouchRightEdgeOfScreen(visual);
		//T9_MultipleDiscoverDownWhenExtendingToTheRight(visual);
		//T10_MeetUpWithExtensionScan(visual);
		//T11_MiniMaze(visual);
		T12_MediumMaze(visual);
	}

	public override void Run(CancellationToken cancellationToken)
	{
		machine.VideoFirmware.SetMode(0x13);

		var visual = new GraphicsLibrary_8bppFlat(machine);

		while (true)
		{
			visual.Clear();

			DrawBoundary(visual);

			Thread.Sleep(1500);

			visual.BorderFill(160, 100, 1, 112);
			visual.Box(0, 0, 319, 199, 14);

			Thread.Sleep(1200);
		}
	}
}
