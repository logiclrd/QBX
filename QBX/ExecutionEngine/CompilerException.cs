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
	public static CompilerException IllegalNumber(Token? context)
		=> new CompilerException(context, "Illegal number");
	public static CompilerException BlockIfWithoutEndIf(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Block IF without END IF");
	public static CompilerException TypeWithoutEndType(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "TYPE without END TYPE");
	public static CompilerException StatementIllegalInTypeBlock(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "Statement illegal in TYPE block");
	public static CompilerException ValueIsNotConstant(Token? context)
		=> new CompilerException(context, "Value is not constant");
	public static CompilerException Overflow(Token? context)
		=> new CompilerException(context, "Overflow");
	public static CompilerException DivisionByZero(Token? context)
		=> new CompilerException(context, "Division by zero");
	public static CompilerException IllegalInSubFunctionOrDefFn(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "Illegal in SUB, FUNCTION or DEF FN");
	public static CompilerException ElementNotDefined(Token? context)
		=> new CompilerException(context, "Element not defined");
	public static CompilerException ArgumentCountMismatch(Token? context)
		=> new CompilerException(context, "Argument count mismatch");
	public static CompilerException IdentifierCannotIncludePeriod(Token? context)
		=> new CompilerException(context, "Identifier cannot include period");
}
