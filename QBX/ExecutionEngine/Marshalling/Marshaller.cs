namespace QBX.ExecutionEngine.Marshalling;

public abstract class Marshaller
{
	public abstract void Map(object from, ref object? to);
}

