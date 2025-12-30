using System;
using System.Collections.Generic;
using System.Text;

namespace QBX.ExecutionEngine.Execution;

public interface IExecutable
{
	CodeModel.Statements.Statement Source { get; }

	void Execute();
}
