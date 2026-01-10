using System.Collections.Generic;
using System.Linq;

namespace QBX.ExecutionEngine.Compiled;

public class StatementPath : Stack<int>
{
	public StatementPath()
	{
	}

	StatementPath(IEnumerable<int> values) : base(values) { }

	public StatementPath Clone()
	{
		return new StatementPath(this.Reverse());
	}
}
