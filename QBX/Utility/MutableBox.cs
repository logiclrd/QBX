namespace QBX.Utility;

public class MutableBox<T>
	where T : struct
{
	public T Value;

	public MutableBox()
	{
	}

	public MutableBox(T value)
	{
		Value = value;
	}
}
