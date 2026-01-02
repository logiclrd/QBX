namespace QBX.DevelopmentEnvironment;

public class Menu(string label, int width) : MenuList<MenuItem>(label)
{
	public int Width = width;
	public int CachedX;
}
