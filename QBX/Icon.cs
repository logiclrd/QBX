namespace QBX;

public class Icon
{
	public int[] Pixels;
	public int Width;
	public int Height;

	public Icon()
		: this(32, 32)
	{
	}

	public Icon(int width, int height)
	{
		Width = width;
		Height = height;
		Pixels = new int[width * height];
	}
}
