using System.Collections;

namespace QBX.DevelopmentEnvironment;

public class Menu(string label, int width) : IEnumerable<MenuItem>
{
	public string Label = label;
	public int Width = width;
	public List<MenuItem> Items = new List<MenuItem>();

	public int CachedX;

	public IEnumerator<MenuItem> GetEnumerator() => Items.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
	public void Add(MenuItem menuItem) => Items.Add(menuItem);
}
