using System;
using System.Collections.Generic;

namespace QBX.ExecutionEngine;

public class UnresolvedReferences
{
	// TODO: declare that a SUB or FUNCTION is expected to exist in the future
	// TODO: track uses of these SUBs/FUNCTIONs so they can be fixed up once
	//       all modules have been compiled
	public List<IUnresolvedCall> UnresolvedCalls = new List<IUnresolvedCall>();

	public bool IsDeclared(string identifier)
	{
		// TODO
		return false;
	}
}
