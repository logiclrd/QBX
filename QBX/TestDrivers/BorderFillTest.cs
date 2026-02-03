using System;
using System.Diagnostics;
using System.Threading;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.TestDrivers;

public class BorderFillTest(Machine machine) : HostedProgram(machine)
{
	public override bool EnableMainLoop => true;

	void T1_SmallEllipse(GraphicsLibrary visual)
	{
		visual.Ellipse(visual.Width / 2, visual.Height / 2, 3, 3, 0, 0, false, false, 1);
	}

	void T2_LargeEllipse(GraphicsLibrary visual)
	{
		visual.Ellipse(visual.Width / 2, visual.Height / 2, 50, 25, 0, 0, false, false, 1);
	}

	void T3_WedgeToLeftEdge(GraphicsLibrary visual)
	{
		visual.Line(0, visual.Height / 4, visual.Width * 5 / 8, visual.Height / 2, 1);
		visual.Line(0, visual.Height * 3 / 4, visual.Width * 5 / 8, visual.Height / 2, 1);
	}

	void T4_EllipseWithCutouts(GraphicsLibrary visual)
	{
		visual.Ellipse(visual.Width / 2, visual.Height / 2, 50, 25, 0, 0, false, false, 1);
		visual.Ellipse(visual.Width / 2 - visual.Width / 20, visual.Height / 2, 15, 15, 0, 0, false, false, 1);
		visual.Ellipse(visual.Width / 2 + visual.Width / 20, visual.Height / 2, 15, 15, 0, 0, false, false, 1);
	}

	void T5_BoxWithPixels(GraphicsLibrary visual)
	{
		int hw = visual.Width / 2;
		int hh = visual.Height / 2;

		visual.Box(hw - 5, hh - 10, hw + 5, hh + 10, 1);

		visual.PixelSet(hw - 2, hh, 1);
		visual.PixelSet(hw - 2, hh - 1, 1);

		visual.PixelSet(hw - 2, hh - 3, 1);
		visual.PixelSet(hw - 2, hh - 4, 1);
	}

	void T6_RandomMaze(GraphicsLibrary visual)
	{
		Random rnd = new Random();

		var seed = rnd.Next();

		//seed = 916776031;
		//seed = 396652144;

		Debug.WriteLine("SEED: " + seed);

		rnd = new Random(seed);

		visual.FillBox(0, 0, visual.Width - 1, visual.Height - 1, 1);

		for (int i = 0; i < 100; i++)
		{
			int x = rnd.Next(0, visual.Width);
			int y = rnd.Next(0, visual.Height);

			int dx = rnd.Next(visual.Width / 13, visual.Width / 6);
			int dy = rnd.Next(visual.Width / 13, visual.Width / 6);

			if ((rnd.Next() & 1) == 0)
				dx /= 20;
			else
				dy /= 20;

			visual.FillBox(x - dx, y - dy, x + dx, y + dy, 0);
		}

		int hw = visual.Width / 2;
		int hh = visual.Height / 2;

		visual.FillBox(hw - 10, hh - 10, hw + 10, hh + 10, 0);
	}

	void T7_MultipleEntrancesInScan(GraphicsLibrary visual)
	{
		int hw = visual.Width / 2;
		int hh = visual.Height / 2;

		visual.Box(hw - 20, hh - 20, hw + 20, hh + 20, 1);

		for (int i = 0; i < 3; i++)
			for (int j = hw - 10; j <= hw + 10; j += 10)
				visual.Line(j - 1, hh - 5 - i, j + 1, hh - 5 - i, 1);
	}

	void T8_TouchRightEdgeOfScreen(GraphicsLibrary visual)
	{
		int hw = visual.Width / 2;
		int hh = visual.Height / 2;

		visual.FillBox(0, 0, visual.Width - 1, visual.Height - 1, 1);
		visual.FillBox(hw - 10, hh - 10, visual.Width - 1, hh + 10, 0);
	}

	void T9_MultipleDiscoverDownWhenExtendingToTheRight(GraphicsLibrary visual)
	{
		int hw = visual.Width / 2;
		int hh = visual.Height / 2;

		visual.FillBox(0, 0, visual.Width - 1, visual.Height - 1, 1);
		visual.FillBox(hw - 10, hh - 10, hw + 10, hh + 10, 0);
		visual.FillBox(hw - 10, hh - 10, hw + 90, hh - 5, 0);
		visual.FillBox(hw, hh - 4, hw + 5, hh - 2, 1);
		visual.FillBox(hw + 40, hh - 10, hw + 50, hh + 30, 0);
		visual.FillBox(hw + 70, hh - 10, hw + 80, hh + 30, 0);
	}

	void T10_MeetUpWithExtensionScan(GraphicsLibrary visual)
	{
		int hw = visual.Width / 2;
		int hh = visual.Height / 2;

		visual.FillBox(0, 0, visual.Width - 1, visual.Height - 1, 1);
		visual.FillBox(hw - 5, hh - 5, hw + 15, hh + 5, 0);
		visual.Line(hw - 30, hh + 1, hw, hh + 1, 0);
		visual.FillBox(hw - 30, hh - 10, hw - 25, hh + 1, 0);
		visual.FillBox(hw - 20, hh - 10, hw - 15, hh + 1, 0);
		visual.Line(hw - 30, hh - 10, hw, hh - 10, 0);
		visual.FillBox(hw - 5, hh - 10, hw, hh, 0);
	}

	void T11_MiniMaze(GraphicsLibrary visual)
	{
		Random rnd = new Random();

		var seed = rnd.Next();

		//seed = 738594425;
		//seed = 302437658;

		Debug.WriteLine("SEED: " + seed);

		rnd = new Random(seed);

		int hw = visual.Width / 2;
		int hh = visual.Height / 2;

		visual.Box(hw - 10, hh - 10, hw + 10, hh + 10, 1);

		for (int i = 0; i < 30; i++)
			visual.PixelSet(rnd.Next(hw - 10, hw + 10), rnd.Next(hh - 10, hh + 10), 1);
	}

	void T12_MediumMaze(GraphicsLibrary visual)
	{
		Random rnd = new Random();

		var seed = rnd.Next();

		//seed = 710843189;

		Debug.WriteLine("SEED: " + seed);

		rnd = new Random(seed);

		int hw = visual.Width / 2;
		int hh = visual.Height / 2;

		visual.Box(hw - 60, hh - 10, hw + 60, hh + 10, 1);

		for (int i = 0; i < 500; i++)
			visual.PixelSet(rnd.Next(hw - 60, hw + 60), rnd.Next(hh - 10, hh + 10), 1);

		visual.PixelSet(hw, hh, 0);
	}

	void T13_FullScreenMaze(GraphicsLibrary visual)
	{
		Random rnd = new Random();

		var seed = rnd.Next();

		Debug.WriteLine("SEED: " + seed);

		rnd = new Random(seed);

		for (int i = 0; i < visual.Width * visual.Height / 6; i++)
			visual.PixelSet(rnd.Next(0, visual.Width), rnd.Next(0, visual.Height), 1);

		visual.PixelSet(visual.Width / 2, visual.Height / 2, 0);
	}

	void DrawBoundary(GraphicsLibrary visual)
	{
		//T1_SmallEllipse(visual);
		//T2_LargeEllipse(visual);
		//T3_WedgeToLeftEdge(visual);
		//T4_EllipseWithCutouts(visual);
		//T5_BoxWithPixels(visual);
		T6_RandomMaze(visual);
		//T7_MultipleEntrancesInScan(visual);
		//T8_TouchRightEdgeOfScreen(visual);
		//T9_MultipleDiscoverDownWhenExtendingToTheRight(visual);
		//T10_MeetUpWithExtensionScan(visual);
		//T11_MiniMaze(visual);
		//T12_MediumMaze(visual);
		//T13_FullScreenMaze(visual);
	}

	public override void Run(CancellationToken cancellationToken)
	{
		machine.VideoFirmware.SetMode(0x12);

		GraphicsLibrary visual = new GraphicsLibrary_4bppPlanar(machine);

		int count = 5;
		int mode = 0x13;

		while (machine.KeepRunning)
		{
			if (count == 0)
			{
				count = 5;
				machine.VideoFirmware.SetMode(mode);

				visual =
					mode switch
					{
						0x12 => new GraphicsLibrary_4bppPlanar(machine),
						0x13 => new GraphicsLibrary_8bppFlat(machine),
						_ => throw new Exception()
					};

				mode ^= 1;
			}

			count--;

			visual.Clear();

			DrawBoundary(visual);

			Thread.Sleep(1500);

			visual.BorderFill(visual.Width / 2, visual.Height / 2, 1, 4);
			visual.Box(0, 0, visual.Width - 1, visual.Height - 1, 14);

			Thread.Sleep(1200);
		}
	}
}
