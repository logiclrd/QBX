using SDL3;

namespace QBX.Hardware;

public unsafe class Display
{
	GraphicsArray _array;
	int _width, _height;

	public Display(GraphicsArray array)
	{
		_array = array;
	}

	public bool UpdateResolution(ref int width, ref int height)
	{
		switch (_array.VideoMode)
		{
			case VideoMode.TextMode: (_width, _height) = (720, 400); break;
			case VideoMode.CGA_320x200_2bpp: (_width, _height) = (320, 200); break;
			case VideoMode.CGA_640x200_1bpp: (_width, _height) = (640, 200); break;
			case VideoMode.EGA_320x200_4bpp: (_width, _height) = (320, 200); break;
			case VideoMode.EGA_640x200_4bpp: (_width, _height) = (640, 200); break;
			case VideoMode.EGA_640x350_4bpp: (_width, _height) = (640, 350); break;
			case VideoMode.EGA_640x350_2bpp_Monochrome: (_width, _height) = (640, 350); break;
			case VideoMode.VGA_640x480_1bpp: (_width, _height) = (640, 480); break;
			case VideoMode.VGA_640x480_4bpp: (_width, _height) = (640, 480); break;
			case VideoMode.VGA_320x200_8bpp: (_width, _height) = (320, 200); break;

			default: return false;
		}

		if ((width != _width) || (height != _height))
		{
			(width, height) = (_width, _height);
			return true;
		}

		return false;
	}

	public void Render(IntPtr texture)
	{
		if (!SDL.LockTexture(texture, default, out var pixelsPtr, out var pitch))
			return;

		try
		{
			switch (_array.VideoMode)
			{
				case VideoMode.TextMode:
					//RenderTextMode();
					break;
				case VideoMode.CGA_320x200_2bpp:
					RenderCGA2bpp(pixelsPtr, pitch);
					break;
				case VideoMode.CGA_640x200_1bpp:
					RenderCGA1bpp(pixelsPtr, pitch);
					break;
				case VideoMode.VGA_640x480_1bpp:
				case VideoMode.EGA_640x350_2bpp_Monochrome:
				case VideoMode.EGA_320x200_4bpp:
				case VideoMode.EGA_640x200_4bpp:
				case VideoMode.EGA_640x350_4bpp:
				case VideoMode.VGA_640x480_4bpp:
					//RenderPlanarGraphics4bpp();
					break;
				case VideoMode.VGA_320x200_8bpp:
					//RenderContiguousGraphics8bpp();
					break;
			}
		}
		finally
		{
			SDL.UnlockTexture(texture);
		}
	}

	private void RenderCGA2bpp(nint pixelsPtr, int pitch)
	{
		int[] palette = _array.Palette;

		for (int y = 0, o = 0xB8000; y < _height; y += 2)
		{
			var targetScan = new Span<int>((void*)(pixelsPtr + y * pitch), _width * 4);

			for (int x = 0; x < _width; x += 4, o++)
			{
				int b = _array[o];

				targetScan[x + 0] = palette[(b >> 6) & 3];
				targetScan[x + 1] = palette[(b >> 4) & 3];
				targetScan[x + 2] = palette[(b >> 2) & 3];
				targetScan[x + 3] = palette[(b >> 0) & 3];
			}
		}

		for (int y = 1, o = 0xBA000; y < _height; y += 2)
		{
			var targetScan = new Span<int>((void*)(pixelsPtr + y * pitch), _width * 4);

			for (int x = 0; x < _width; x += 4, o++)
			{
				int b = _array[o];

				targetScan[x + 0] = palette[(b >> 6) & 3];
				targetScan[x + 1] = palette[(b >> 4) & 3];
				targetScan[x + 2] = palette[(b >> 2) & 3];
				targetScan[x + 3] = palette[(b >> 0) & 3];
			}
		}
	}

	private void RenderCGA1bpp(nint pixelsPtr, int pitch)
	{
		int[] palette = _array.Palette;

		for (int y = 0, o = 0xB8000; y < _height; y += 2)
		{
			var targetScan = new Span<int>((void*)(pixelsPtr + y * pitch), _width * 4);

			for (int x = 0; x < _width; x += 8, o++)
			{
				int b = _array[o];

				targetScan[x + 0] = palette[(b >> 7) & 1];
				targetScan[x + 1] = palette[(b >> 6) & 1];
				targetScan[x + 2] = palette[(b >> 5) & 1];
				targetScan[x + 3] = palette[(b >> 4) & 1];
				targetScan[x + 4] = palette[(b >> 3) & 1];
				targetScan[x + 5] = palette[(b >> 2) & 1];
				targetScan[x + 6] = palette[(b >> 1) & 1];
				targetScan[x + 7] = palette[(b >> 0) & 1];
			}
		}

		for (int y = 1, o = 0xBA000; y < _height; y += 2)
		{
			var targetScan = new Span<int>((void*)(pixelsPtr + y * pitch), _width * 4);

			for (int x = 0; x < _width; x += 8, o++)
			{
				int b = _array[o];

				targetScan[x + 0] = palette[(b >> 7) & 1];
				targetScan[x + 1] = palette[(b >> 6) & 1];
				targetScan[x + 2] = palette[(b >> 5) & 1];
				targetScan[x + 3] = palette[(b >> 4) & 1];
				targetScan[x + 4] = palette[(b >> 3) & 1];
				targetScan[x + 5] = palette[(b >> 2) & 1];
				targetScan[x + 6] = palette[(b >> 1) & 1];
				targetScan[x + 7] = palette[(b >> 0) & 1];
			}
		}
	}
}
