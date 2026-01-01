namespace QBX.Hardware;

public class Machine
{
	public GraphicsArray GraphicsArray { get; }
	public Adapter Display { get; }
	public Keyboard Keyboard { get; }

	public Machine()
	{
		GraphicsArray = new GraphicsArray();
		Display = new Adapter(GraphicsArray);
		Keyboard = new Keyboard();
	}
}
