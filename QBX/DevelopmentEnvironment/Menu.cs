namespace QBX.DevelopmentEnvironment;

public class Menu(string label, int width, string? helpContextString = null) : MenuList<MenuItem>(label, helpContextString)
{
	public int Width = width;
	public int CachedX;
}
