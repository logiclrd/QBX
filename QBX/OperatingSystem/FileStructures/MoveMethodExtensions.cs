using System;
using System.IO;

namespace QBX.OperatingSystem.FileStructures;

public static class MoveMethodExtensions
{
	public static SeekOrigin ToSeekOrigin(this MoveMethod moveMethod)
	{
		switch (moveMethod)
		{
			case MoveMethod.FromBeginning: return SeekOrigin.Begin;
			case MoveMethod.FromCurrent: return SeekOrigin.Current;
			case MoveMethod.FromEnd: return SeekOrigin.End;

			default: throw new DOSException(DOSError.InvalidFunction);
		}
	}
}
