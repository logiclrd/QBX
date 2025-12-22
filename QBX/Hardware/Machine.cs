namespace QBX.Hardware;

public class Machine
{
	public GraphicsArray GraphicsArray { get; }
	public Display Display { get; }

	public Machine()
	{
		GraphicsArray = new GraphicsArray();
		Display = new Display(GraphicsArray);
	}
}
