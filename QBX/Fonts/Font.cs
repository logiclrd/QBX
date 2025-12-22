namespace QBX.Fonts;

public class Font
{
	public byte[] Bits;
	public byte[][] Glyphs;

	public static readonly Font _8x8 = LoadFontResource("8x8.bin");
	public static readonly Font _8x16 = LoadFontResource("8x16.bin");

	static Font LoadFontResource(string resourceName)
	{
		resourceName = "QBX.Fonts." + resourceName;

		var stream = typeof(Font).Assembly.GetManifestResourceStream(resourceName);

		if (stream == null)
			throw new Exception("Failed to load font from resource: " + resourceName);

		return new Font(stream);
	}

	public Font(Stream stream)
	{
		Bits = new byte[stream.Length];

		stream.ReadExactly(Bits);

		Glyphs = UnpackGlyphs();
	}

	byte[][] UnpackGlyphs()
	{
		var glyphs = new byte[256][];

		int charHeight = Bits.Length / 256;

		for (int i = 0; i < 256; i++)
		{
			byte[] glyph = new byte[8 * charHeight];

			int o = i * charHeight;

			for (int y = 0; y < charHeight; y++)
			{
				int row = Bits[o + y];

				int p = y * 8;

				for (int x = 0, b = 128; x < 8; x++, b >>= 1)
					if ((row & b) != 0)
						glyph[p + x] = 1;
			}

			glyphs[i] = glyph;
		}

		return glyphs;
	}
}
