using System.Runtime.InteropServices;

using NUnit.Framework.Internal;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.Tests.Firmware;

public class GraphicsLibraryTests
{
	// Test case structure:
	// - mode: value passed to Video.SetMode
	// - setup: sequence of pairs, (offset, bytevalue)
	// - test: sequence of triplets, (x, y, attribute)
	// - expectations: sequence of pairs, (offset, bytevalue)
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 0, 0, 1 },
		new int[] { 0, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 1, 0, 1 },
		new int[] { 0, 64 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 2, 0, 1 },
		new int[] { 0, 32 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 3, 0, 1 },
		new int[] { 0, 16 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 4, 0, 1 },
		new int[] { 0, 8 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 5, 0, 1 },
		new int[] { 0, 4 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 6, 0, 1 },
		new int[] { 0, 2 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 7, 0, 1 },
		new int[] { 0, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 8, 0, 1 },
		new int[] { 1, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 0, 0, 0 },
		new int[] { 0, ~128 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 1, 0, 0 },
		new int[] { 0, ~64 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 2, 0, 0 },
		new int[] { 0, ~32 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 3, 0, 0 },
		new int[] { 0, ~16 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 4, 0, 0 },
		new int[] { 0, ~8 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 5, 0, 0 },
		new int[] { 0, ~4 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 6, 0, 0 },
		new int[] { 0, ~2 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 7, 0, 0 },
		new int[] { 0, ~1 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[] { 0, 255 },
		new int[] { 8, 0, 0 },
		new int[] { 0, 255, 1, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 638, 0, 1 },
		new int[] { 79, 2 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 639, 0, 1 },
		new int[] { 79, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 640, 0, 1 },
		new int[] { 79, 0, 80, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 0, 1, 1 },
		new int[] { 80, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 638, 199, 1 },
		new int[] { 0x3E7F, 2 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 639, 199, 1 },
		new int[] { 0x3E7F, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 640, 199, 1 },
		new int[] { 0x3E7F, 0, 0x3E80, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		0x6,
		new int[0],
		new int[] { 639, 200, 1 },
		new int[] { 0x3E7F, 0, 0x3ECF, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 0, 0, 3 },
		new int[] { 0x0000, 192, 0x4000, 0, 0x2000, 0, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[] { 0x4000, 128 },
		new int[] { 1, 0, 3 },
		new int[] { 0x0000, 0, 0x4000, 192, 0x2000, 0, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[] { 0x0000, 128 },
		new int[] { 2, 0, 3 },
		new int[] { 0x0000, 48 | 128, 0x4000, 0, 0x2000, 0, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 3, 0, 3 },
		new int[] { 0x0000, 0, 0x4000, 48, 0x2000, 0, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 4, 0, 3 },
		new int[] { 0x0000, 12, 0x4000, 0, 0x2000, 0, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 5, 0, 3 },
		new int[] { 0x0000, 0, 0x4000, 12, 0x2000, 0, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 6, 0, 3 },
		new int[] { 0x0000, 3, 0x4000, 0, 0x2000, 0, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 7, 0, 3 },
		new int[] { 0x0000, 0, 0x4000, 3, 0x2000, 0, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 1, 1, 3 },
		new int[] { 0x0000, 0, 0x4000, 0, 0x2000, 0, 0x6000, 192 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[] { 0x0000, 128, 0x4000, 64 },
		new int[] { 2, 1, 3 },
		new int[] { 0x0000, 128, 0x4000, 64, 0x2000, 48, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 3, 1, 3 },
		new int[] { 0x0000, 0, 0x4000, 0, 0x2000, 0, 0x6000, 48 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[] { 0x2000, 8 | 2 },
		new int[] { 4, 1, 3 },
		new int[] { 0x0000, 0, 0x4000, 0, 0x2000, 12 | 2, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 5, 1, 3 },
		new int[] { 0x0000, 0, 0x4000, 0, 0x2000, 0, 0x6000, 12 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 6, 1, 3 },
		new int[] { 0x0000, 0, 0x4000, 0, 0x2000, 3, 0x6000, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 7, 1, 3 },
		new int[] { 0x0000, 0, 0x4000, 0, 0x2000, 0, 0x6000, 3 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 319, 199, 3 },
		new int[] { 0x0F9F, 0, 0x4F9F, 0, 0x2F9F, 0, 0x6F9F, 3 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 319, 198, 3 },
		new int[] { 0x0F9F, 0, 0x4F9F, 3, 0x2F9F, 0, 0x6F9F, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 318, 199, 3 },
		new int[] { 0x0F9F, 0, 0x4F9F, 0, 0x2F9F, 3, 0x6F9F, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 318, 198, 3 },
		new int[] { 0x0F9F, 3, 0x4F9F, 0, 0x2F9F, 0, 0x6F9F, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 320, 199, 3 },
		new int[]
		{
			0x0F9F, 0, 0x4F9F, 0, 0x2F9F, 0, 0x6F9F, 0,
			0x0FA0, 0, 0x4FA0, 0, 0x2FA0, 0, 0x6FA0, 0,
		})]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 318, 200, 3 },
		new int[]
		{
			0x0F9F, 0, 0x4F9F, 0, 0x2F9F, 0, 0x6F9F, 0,
			0x0FC7, 0, 0x4FC7, 0, 0x2FC7, 0, 0x6FC7, 0,
		})]
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		0x5,
		new int[0],
		new int[] { 319, 200, 3 },
		new int[]
		{
			0x0F9F, 0, 0x4F9F, 0, 0x2F9F, 0, 0x6F9F, 0,
			0x0FC7, 0, 0x4FC7, 0, 0x2FC7, 0, 0x6FC7, 0,
		})]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0xD,
		new int[0],
		new int[] { 319, 0, 1 | 2 | 4 | 8 },
		new int[] { 0x00027, 1, 0x10027, 1, 0x20027, 1, 0x30027, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0xD,
		new int[0],
		new int[] { 320, 0, 1 | 2 | 4 | 8 },
		new int[]
		{
			0x00027, 0, 0x10027, 0, 0x20027, 0, 0x30027, 0,
			0x00028, 0, 0x10028, 0, 0x20028, 0, 0x30028, 0,
		})]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 0, 0, 1 | 2 | 4 | 8 },
		new int[] { 0, 128, 0x10000, 128, 0x20000, 128, 0x30000, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0xD,
		new int[0],
		new int[] { 0, 1, 1 | 2 | 4 | 8 },
		new int[] { 0x00028, 128, 0x10028, 128, 0x20028, 128, 0x30028, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0xD,
		new int[0],
		new int[] { 319, 199, 1 | 2 | 4 | 8 },
		new int[] { 0x01F3F, 1, 0x11F3F, 1, 0x21F3F, 1, 0x31F3F, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0xD,
		new int[0],
		new int[] { 320, 199, 1 | 2 | 4 | 8 },
		new int[]
		{
			0x01F3F, 0, 0x11F3F, 0, 0x21F3F, 0, 0x31F3F, 0,
			0x01F40, 0, 0x11F40, 0, 0x21F40, 0, 0x31F40, 0,
		})]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0xD,
		new int[0],
		new int[] { 319, 200, 1 | 2 | 4 | 8 },
		new int[]
		{
			0x01F3F, 0, 0x11F3F, 0, 0x21F3F, 0, 0x31F3F, 0,
			0x01F67, 0, 0x11F67, 0, 0x21F67, 0, 0x31F67, 0,
		})]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 1, 0, 1 | 2 | 4 | 8 },
		new int[] { 0, 64, 0x10000, 64, 0x20000, 64, 0x30000, 64 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 2, 0, 1 | 2 | 4 | 8 },
		new int[] { 0, 32, 0x10000, 32, 0x20000, 32, 0x30000, 32 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 3, 0, 1 | 2 | 4 | 8 },
		new int[] { 0, 16, 0x10000, 16, 0x20000, 16, 0x30000, 16 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 4, 0, 1 | 2 | 4 | 8 },
		new int[] { 0, 8, 0x10000, 8, 0x20000, 8, 0x30000, 8 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 5, 0, 1 | 2 | 4 | 8 },
		new int[] { 0, 4, 0x10000, 4, 0x20000, 4, 0x30000, 4 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 6, 0, 1 | 2 | 4 | 8 },
		new int[] { 0, 2, 0x10000, 2, 0x20000, 2, 0x30000, 2 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 7, 0, 1 | 2 | 4 | 8 },
		new int[] { 0, 1, 0x10000, 1, 0x20000, 1, 0x30000, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 8, 0, 1 | 2 | 4 | 8 },
		new int[] { 1, 128, 0x10001, 128, 0x20001, 128, 0x30001, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 638, 0, 1 | 2 | 4 | 8 },
		new int[] { 0x0004F, 2, 0x1004F, 2, 0x2004F, 2, 0x3004F, 2 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 639, 0, 1 | 2 | 4 | 8 },
		new int[] { 0x0004F, 1, 0x1004F, 1, 0x2004F, 1, 0x3004F, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 640, 0, 1 | 2 | 4 | 8 },
		new int[]
		{
			0x0004F, 0, 0x1004F, 0, 0x2004F, 0, 0x3004F, 0,
			0x00050, 0, 0x10050, 0, 0x20050, 0, 0x30050, 0,
		})]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 0, 1, 1 | 2 | 4 | 8 },
		new int[] { 0x00050, 128, 0x10050, 128, 0x20050, 128, 0x30050, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 632, 479, 1 | 2 | 4 | 8 },
		new int[] { 0x095FF, 128, 0x195FF, 128, 0x295FF, 128, 0x395FF, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 639, 479, 1 | 2 | 4 | 8 },
		new int[] { 0x095FF, 1, 0x195FF, 1, 0x295FF, 1, 0x395FF, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 639, 479, 1 | 4 },
		new int[] { 0x095FF, 1, 0x195FF, 0, 0x295FF, 1, 0x395FF, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 639, 479, 2 | 8 },
		new int[] { 0x095FF, 0, 0x195FF, 1, 0x295FF, 0, 0x395FF, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 639, 479, 1 | 2 },
		new int[] { 0x095FF, 1, 0x195FF, 1, 0x295FF, 0, 0x395FF, 0 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[0],
		new int[] { 639, 479, 4 | 8 },
		new int[] { 0x095FF, 0, 0x195FF, 0, 0x295FF, 1, 0x395FF, 1 })]
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12,
		new int[] { 0, 129 },
		new int[] { 0, 0, 2 | 4 | 8 },
		new int[] { 0, 1, 0x10000, 128, 0x20000, 128, 0x30000, 128 })]
	[TestCase(
		typeof(GraphicsLibrary_8bppFlat),
		0x13,
		new int[] { 0, 1 },
		new int[] { 0, 0, 2 },
		new int[] { 0, 2 })]
	[TestCase(
		typeof(GraphicsLibrary_8bppFlat),
		0x13,
		new int[] { 0, 1 },
		new int[] { 319, 0, 2 },
		new int[] { 0, 1, 319, 2 })]
	[TestCase(
		typeof(GraphicsLibrary_8bppFlat),
		0x13,
		new int[] { 0, 1 },
		new int[] { 0, 1, 2 },
		new int[] { 0, 1, 320, 2 })]
	[TestCase(
		typeof(GraphicsLibrary_8bppFlat),
		0x13,
		new int[] { 0, 1 },
		new int[] { 319, 199, 2 },
		new int[] { 0, 1, 63999, 2 })]
	public void TestPixelSet(
		Type libraryImplementationType,
		int mode,
		int[] setup,
		int[] test,
		int[] expectations)
	{
		// Arrange
		var machine = new Machine();

		var array = machine.GraphicsArray;

		machine.VideoFirmware.SetMode(mode);

		var sut = (GraphicsLibrary)Activator.CreateInstance(libraryImplementationType, machine)!;

		for (int i = 0; i + 1 < setup.Length; i += 2)
		{
			int offset = setup[i];
			byte byteValue = (byte)setup[i + 1];

			array.VRAM[offset] = byteValue;
		}

		// Act
		for (int i = 0; i + 2 < test.Length; i += 3)
		{
			int x = test[i];
			int y = test[i + 1];
			int attribute = test[i + 2];

			sut.PixelSet(x, y, attribute);
		}

		// Assert
		for (int i = 0; i + 1 < expectations.Length; i += 2)
		{
			int offset = expectations[i];
			byte expectedByteValue = unchecked((byte)expectations[i + 1]);

			array.VRAM[offset].Should().Be(expectedByteValue);
		}
	}

	[Test]
	public void HorizontalSpanCompleteBytes(
		[Values(5, 6, 0xD, 0x12)]
		int mode,
		[Values(8, 16, 24, 32)]
		int pixelCount,
		[Values(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)]
		int attribute)
	{
		// Arrange
		var machine = new Machine();

		var array = machine.GraphicsArray;

		machine.VideoFirmware.SetMode(mode);

		var sut =
			mode switch
			{
				5 => new GraphicsLibrary_2bppInterleaved(machine),
				6 => new GraphicsLibrary_1bppPacked(machine),
				0xD => new GraphicsLibrary_4bppPlanar(machine),
				0x12 => new GraphicsLibrary_4bppPlanar(machine),

				_ => default(GraphicsLibrary) ?? throw new Exception("Unrecognized mode")
			};

		sut.PixelSet(0, 0, attribute);

		var adapter = new Adapter(array);

		int targetWidth = 0, targetHeight = 0;
		int targetWidthScale = 0, targetHeightScale = 0;

		adapter.UpdateResolution(ref targetWidth, ref targetHeight, ref targetWidthScale, ref targetHeightScale);

		var target = new int[targetWidth * targetHeight];

		var targetBuffer = MemoryMarshal.AsBytes(target.AsSpan());

		int targetPitch = sut.Width * 4;

		adapter.Render(targetBuffer, targetPitch);

		int expectedBGRA = target[0];

		sut.Clear();

		adapter.Render(targetBuffer, targetPitch);

		const int Black = 0x000000FF;

		Assume.That(target[0] == Black);

		// Act
		sut.HorizontalLine(0, pixelCount - 1, 0, attribute);

		// Assert
		adapter.Render(targetBuffer, targetPitch);

		for (int i = 0; i < pixelCount; i++)
			target[i].Should().Be(expectedBGRA);
		for (int i = pixelCount; i < sut.Width; i++)
			target[i].Should().Be(Black);
	}

	[Test]
	public void GetSprite_PutSprite(
		[Values(5, 6, 0x12, 0x13)]
		int mode,
		[Values(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
		int x,
		[Values(0, 5, 10)]
		int y,
		[Values(1, 2, 3, 4, 5, 6, 7, 8, 9, 15, 16, 17, 23, 24, 25)]
		int w,
		[Values(1, 3, 5)]
		int h,
		[Values(155, 156, 157, 158, 159, 160, 161, 162)]
		int tx)
	{
		// Arrange
		var machine = new Machine();

		var array = machine.GraphicsArray;

		machine.VideoFirmware.SetMode(mode);

		var sut =
			mode switch
			{
				5 => new GraphicsLibrary_2bppInterleaved(machine),
				6 => new GraphicsLibrary_1bppPacked(machine),
				0x12 => new GraphicsLibrary_4bppPlanar(machine),
				0x13 => new GraphicsLibrary_8bppFlat(machine),
				_ => default(GraphicsLibrary) ?? throw new Exception("Sanity failure")
			};

		int maxAttribute =
			mode switch
			{
				5 => 3,
				6 => 1,
				0x12 => 15,
				0x13 => 255,
				_ => throw new Exception("Sanity failure")
			};

		int seed = mode + x * 5 + y * 50 + w * 500 + h * 5000;

		Random rnd = new Random(seed);

		for (int i = 0; i < 50; i++)
			sut.Ellipse(rnd.Next(50), rnd.Next(50), rnd.Next(20), rnd.Next(20), 0, 0, false, false, rnd.Next(maxAttribute));

		var buffer = new byte[(w + 7) * h + 4];

		int[] expectedPixels = new int[w * h];

		for (int yy = 0, o = 0; yy < h; yy++)
			for (int xx = 0; xx < w; xx++, o++)
				expectedPixels[o] = sut.PixelGet(x + xx, y + yy);

		// Act
		sut.GetSprite(x, y, x + w - 1, y + h - 1, buffer);
		sut.Clear();
		sut.PutSprite(buffer, PutSpriteAction.PixelSet, tx, 100);

		// Assert
		char[] buf = new char[w + 20];

		for (int yy = -10; yy < h + 10; yy++)
		{
			for (int xx = -10; xx < w + 10; xx++)
			{
				int pel = sut.PixelGet(xx + tx, yy + 100);

				if (pel > 9)
					buf[xx + 10] = unchecked((char)('A' + (pel - 10)));
				else
					buf[xx + 10] = unchecked((char)('0' + pel));
			}

			System.Diagnostics.Debug.WriteLine(new string(buf));
		}

		for (int yy = 0, o = 0; yy < h; yy++)
			for (int xx = 0; xx < w; xx++, o++)
				sut.PixelGet(xx + tx, yy + 100).Should().Be(expectedPixels[o]);

		for (int yy = -10; yy < h + 10; yy++)
			for (int xx = -10; xx < w + 10; xx++)
				if ((xx < 0) || (xx >= w) || (yy < 0) || (yy >= h))
					sut.PixelGet(xx + tx, yy + 100).Should().Be(0);
	}
}
