using QBX.Firmware;
using QBX.Hardware;

using System.Runtime.InteropServices;

namespace QBX.Tests.Hardware;

public class AdapterTests
{
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		5, // mode
		3, // max colour/attribute value
		0, false)] // CGA palette
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		5, // mode
		3, // max colour/attribute value
		0, true)] // CGA palette
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		5, // mode
		3, // max colour/attribute value
		1, false)] // CGA palette
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		5, // mode
		3, // max colour/attribute value
		1, true)] // CGA palette
	[TestCase(
		typeof(GraphicsLibrary_2bppInterleaved),
		5, // mode
		3, // max colour/attribute value
		2, false)] // CGA palette
	[TestCase(
		typeof(GraphicsLibrary_1bppPacked),
		6, // mode
		1, // max colour/attribute value
		null, false)] // CGA palette
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0xD, // mode
		15, // max colour/attribute value
		null, false)] // CGA palette
	[TestCase(
		typeof(GraphicsLibrary_4bppPlanar),
		0x12, // mode
		15, // max colour/attribute value
		null, false)] // CGA palette
	[TestCase(
		typeof(GraphicsLibrary_8bppFlat),
		0x13, // mode
		255, // max colour/attribute value
		null, false)] // CGA palette
	public void Render(Type libraryImplementationType, int modeNumber, int maxColour, int? cgaPalette, bool cgaPaletteIntensity)
	{
		// Arrange
		Assume.That((maxColour & (maxColour + 1)) == 0, "maxColour must be one less than a power of two");

		var machine = new Machine();

		var array = machine.GraphicsArray;

		var video = machine.VideoFirmware;

		video.SetMode(modeNumber);

		var library = (GraphicsLibrary)Activator.CreateInstance(libraryImplementationType, array)!;

		for (int y = 0; y < library.Height; y++)
			for (int x = 0; x < library.Width; x++)
				library.PixelSet(x, y, (x + y) & maxColour);

		int[] paletteBGRA = new int[maxColour + 1];

		var dacPaletteBGRA = MemoryMarshal.Cast<byte, int>(array.DAC.PaletteBGRA);

		if (maxColour == 255)
			dacPaletteBGRA.CopyTo(paletteBGRA);
		else
		{
			if (cgaPalette != null)
				machine.VideoFirmware.LoadCGAPalette(cgaPalette.Value, cgaPaletteIntensity);

			if (maxColour == 1)
			{
				paletteBGRA[0] = dacPaletteBGRA[0];
				paletteBGRA[1] = dacPaletteBGRA[15]; // monochrome is actually 4bpp with all planes mapped
			}
			else
			{
				for (int i = 0; i < paletteBGRA.Length; i++)
				{
					int paletteIndex = array.AttributeController.Registers[i];

					paletteBGRA[i] = dacPaletteBGRA[paletteIndex];
				}
			}
		}

		var sut = new Adapter(array);

		int targetWidth = 0, targetHeight = 0;
		int targetWidthScale = 0, targetHeightScale = 0;

		sut.UpdateResolution(ref targetWidth, ref targetHeight, ref targetWidthScale, ref targetHeightScale);

		var target = new int[targetWidth * targetHeight];

		var targetBuffer = MemoryMarshal.AsBytes(target.AsSpan());

		int targetPitch = library.Width * 4;

		// Act
		sut.Render(targetBuffer, targetPitch);

		// Assert
		targetWidth.Should().Be(library.Width);
		targetHeight.Should().Be(library.Height);

		for (int y = 0, o = 0; y < targetHeight; y++)
			for (int x = 0; x < targetWidth; x++, o++)
			{
				int expectedPaletteIndex = (x + y) & maxColour;

				target[o].Should().Be(paletteBGRA[expectedPaletteIndex]);
			}
	}

	[TestCase(0, false, new uint[] { 0x000000FF, 0x00AA00FF, 0x0000AAFF, 0x0055AAFF })]
	[TestCase(0, true, new uint[] { 0x000000FF, 0x55FF55FF, 0x5555FFFF, 0x55FFFFFF })]
	[TestCase(1, false, new uint[] { 0x000000FF, 0xAAAA00FF, 0xAA00AAFF, 0xAAAAAAFF })]
	[TestCase(1, true, new uint[] { 0x000000FF, 0xFFFF55FF, 0xFF55FFFF, 0xFFFFFFFF })]
	[TestCase(2, false, new uint[] { 0x000000FF, 0xAAAA00FF, 0x0000AAFF, 0xAAAAAAFF })]
	[TestCase(2, true, new uint[] { 0x000000FF, 0xFFFF55FF, 0x5555FFFF, 0xFFFFFFFF })]
	public void TestCGAPalette(int cgaPalette, bool cgaPaletteIntensity, uint[] expectedPaletteBGRA)
	{
		// Arrange
		var machine = new Machine();

		var array = machine.GraphicsArray;

		var video = machine.VideoFirmware;

		video.SetMode(5);

		var library = new GraphicsLibrary_2bppInterleaved(array);

		for (int x = 0; x < 4; x++)
			library.PixelSet(x, 0, x);

		machine.VideoFirmware.LoadCGAPalette(cgaPalette, cgaPaletteIntensity);

		var sut = new Adapter(array);

		int targetWidth = 0, targetHeight = 0;
		int targetWidthScale = 0, targetHeightScale = 0;

		sut.UpdateResolution(ref targetWidth, ref targetHeight, ref targetWidthScale, ref targetHeightScale);

		var target = new uint[targetWidth * targetHeight];

		var targetBuffer = MemoryMarshal.AsBytes(target.AsSpan());

		int targetPitch = library.Width * 4;

		// Act
		sut.Render(targetBuffer, targetPitch);

		// Assert
		targetWidth.Should().Be(library.Width);
		targetHeight.Should().Be(library.Height);

		for (int x = 0; x < 4; x++)
			target[x].Should().Be(expectedPaletteBGRA[x]);
	}
}
