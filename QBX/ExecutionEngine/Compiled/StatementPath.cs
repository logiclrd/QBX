using System.Collections.Generic;
using System.Linq;

namespace QBX.ExecutionEngine.Compiled;

public class StatementPath : Stack<int>
{
	// TODO: StatementPath needs to capture StackFrames to permit resuming across
	//       Routine boundaries (needed for error handling)

	public StatementPath()
	{
	}

	StatementPath(IEnumerable<int> values) : base(values) { }

	public StatementPath Clone()
	{
		return new StatementPath(this.Reverse());
	}
}
