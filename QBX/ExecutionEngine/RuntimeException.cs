using System;

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

[Serializable]
public class RuntimeException : Exception
{
	public Token? Context { get; }
	public int ContextLength { get; }

	public RuntimeException(string message)
		: this(default(Token), message)
	{
	}

	public RuntimeException(CodeModel.Statements.Statement? statement, string message)
		: this(
				statement?.FirstToken,
				statement?.SourceLength ?? 0,
				message)
	{
	}

	public RuntimeException(Token? context, string message)
		: this(context, context?.Length ?? 0, message)
	{
	}

	public RuntimeException(Token? context, int contextLength, string message)
		: base(message)
	{
		Context = context;
		ContextLength = contextLength;
	}

	public static RuntimeException IllegalFunctionCall(CodeModel.Statements.Statement? statement)
		=> new RuntimeException(statement, "Illegal function call");
	public static RuntimeException Overflow(CodeModel.Statements.Statement? statement)
		=> new RuntimeException(statement, "Overflow");
	public static RuntimeException Overflow(Token? context)
		=> new RuntimeException(context, "Overflow");
	public static RuntimeException DivisionByZero(Token? context)
		=> new RuntimeException(context, "Division by zero");
}
