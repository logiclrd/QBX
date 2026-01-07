using System;

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

[Serializable]
public class CompilerException : Exception
{
	public Token? Context { get; }
	public int ContextLength { get; }

	public CompilerException(string message)
		: this(default(Token), message)
	{
	}

	public CompilerException(CodeModel.Statements.Statement? statement, string message)
		: this(
				statement?.FirstToken,
				statement?.SourceLength ?? 0,
				message)
	{
	}

	public CompilerException(Token? context, string message)
		: this(context, context?.Length ?? 0, message)
	{
	}

	public CompilerException(Token? context, int contextLength, string message)
		: base(message)
	{
		Context = context;
		ContextLength = contextLength;
	}

	public static CompilerException TypeMismatch(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Type mismatch");
	public static CompilerException TypeMismatch(Token? context)
		=> new CompilerException(context, "Type mismatch");
	public static CompilerException DuplicateDefinition(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Duplicate definition");
	public static CompilerException DuplicateDefinition(Token? context)
		=> new CompilerException(context, "Duplicate definition");
	public static CompilerException SubprogramNotDefined(Token? context)
		=> new CompilerException(context, "Subprogram not defined");
	public static CompilerException NextWithoutFor(Token? context)
		=> new CompilerException(context, "NEXT without FOR");
	public static CompilerException BlockIfWithoutEndIf(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Block IF without END IF");
}
