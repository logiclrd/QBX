namespace QBX.Utility;

public class SharedRef<T>
	where T : class
{
	public T? Value;

	public SharedRef()
	{
	}

	public SharedRef(T? value)
	{
		Value = value;
	}
}
