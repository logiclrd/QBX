using System;

namespace QBX.OperatingSystem;

public class DOSException(DOSError error) : Exception
{
	public DOSError Error => error;
}
