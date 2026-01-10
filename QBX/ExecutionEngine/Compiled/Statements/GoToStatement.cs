using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class GoToStatement(string labelName, CodeModel.Statements.Statement source)
	: JumpStatement(labelName, source)
{
}
