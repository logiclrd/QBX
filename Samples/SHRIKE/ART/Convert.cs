using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

class Convert
{
	static void Main()
	{
		Bitmap vgaPalBmp;

		using (var vgaPalStream = File.OpenRead("VGAPAL.PNG"))
			vgaPalBmp = (Bitmap)Bitmap.FromStream(vgaPalStream);

		s_vgaPal = new Dictionary<Color, byte>();

		for (int i = 0; i < 256; i++)
		{
			var palPel = vgaPalBmp.GetPixel(i % 16, i / 16);

			if (!s_vgaPal.ContainsKey(palPel))
				s_vgaPal[palPel] = (byte)i;
		}

		foreach (var filename in Directory.GetFiles(".", "*.PNG"))
			DoConversion(filename);
	}

	static Dictionary<Color, byte> s_vgaPal;

	static void DoConversion(string pngPath)
	{
		Console.Write("{0}: ", pngPath);

		string artPath = Path.ChangeExtension(pngPath, ".ART");

		var pngData = File.ReadAllBytes(pngPath);
		var pngStream = new MemoryStream(pngData);

		using (var img = (Bitmap)Bitmap.FromFile(pngPath))
		using (var art = File.OpenWrite(artPath))
		{
			Console.WriteLine("{0}x{1}", img.Width, img.Height);

			art.WriteByte((byte)img.Width);
			art.WriteByte((byte)img.Height);

			//var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

			try
			{
				for (int y = 0; y < img.Height; y++)
					for (int x = 0; x < img.Width; x++)
					{
						var pel = img.GetPixel(x, y);

						s_vgaPal.TryGetValue(pel, out var pelIdx);

						art.WriteByte(pelIdx);
					}
			}
			finally
			{
				//img.UnlockBits(data);
			}

			for (int y = 0; y < img.Height; y++)
				for (int x = 0; x < img.Width; x++)
				{
					var pel = img.GetPixel(x, y);

					art.WriteByte(pel.A);
				}
		}
	}
}
