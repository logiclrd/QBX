using System;

namespace QBX.ExecutionEngine;

[Serializable]
public class BadModelException : Exception
{
	public BadModelException(string message)
		: base(message)
	{
	}
}
