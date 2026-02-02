using System;

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

[Serializable]
public class CompilerException : Exception
{
	public Token? Context { get; private set; }
	public int ContextLength { get; private set; }

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

	public CompilerException(CodeModel.Expressions.Expression? expression, string message)
		: this(
				expression?.Token,
				expression?.Token?.Length ?? 0,
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

	public CompilerException AddContext(CodeModel.Statements.Statement? statement)
		=> AddContext(statement?.FirstToken, statement?.SourceLength);

	public CompilerException AddContext(CodeModel.Expressions.Expression? expression)
		=> AddContext(expression?.Token);

	CompilerException AddContext(Token? context, int? contextLength = null)
	{
		if (Context == null)
		{
			Context = context;
			ContextLength = contextLength ?? context?.Length ?? 0;
		}

		return this;
	}

	public static CompilerException TypeMismatch(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Type mismatch");
	public static CompilerException TypeMismatch(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Type mismatch");
	public static CompilerException TypeMismatch(Token? context)
		=> new CompilerException(context, "Type mismatch");
	public static CompilerException TypeMismatch()
		=> new CompilerException("Type mismatch");
	public static CompilerException DuplicateDefinition(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Duplicate definition");
	public static CompilerException DuplicateDefinition(Token? context)
		=> new CompilerException(context, "Duplicate definition");
	public static CompilerException TypeNotDefined(Token? context)
		=> new CompilerException(context, "Type not defined");
	public static CompilerException SubprogramNotDefined(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Subprogram not defined");
	public static CompilerException SubprogramNotDefined(Token? context)
		=> new CompilerException(context, "Subprogram not defined");
	public static CompilerException NextWithoutFor(Token? context)
		=> new CompilerException(context, "NEXT without FOR");
	public static CompilerException IllegalNumber(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Illegal number");
	public static CompilerException BlockIfWithoutEndIf(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Block IF without END IF");
	public static CompilerException TypeWithoutEndType(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "TYPE without END TYPE");
	public static CompilerException StatementIllegalInTypeBlock(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "Statement illegal in TYPE block");
	public static CompilerException ValueIsNotConstant(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Value is not constant");
	public static CompilerException Overflow(Token? context)
		=> new CompilerException(context, "Overflow");
	public static CompilerException DivisionByZero(Token? context)
		=> new CompilerException(context, "Division by zero");
	public static CompilerException IllegalInSubFunctionOrDefFn(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "Illegal in SUB, FUNCTION or DEF FN");
	public static CompilerException ElementNotDefined(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Element not defined");
	public static CompilerException ArgumentCountMismatch(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Argument count mismatch");
	public static CompilerException ArgumentCountMismatch(Token? context)
		=> new CompilerException(context, "Argument count mismatch");
	public static CompilerException IdentifierCannotIncludePeriod(Token? context)
		=> new CompilerException(context, "Identifier cannot include period");
	public static CompilerException LabelNotDefined(Token? context)
		=> new CompilerException(context, "Label not defined");
	public static CompilerException DuplicateLabel(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Duplicate label");
	public static CompilerException SelectWithoutEndSelect(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "SELECT without END SELECT");
	public static CompilerException StatementsAndLabelsIllegalBetweenSelectCaseAndCase(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Statements/labels illegal between SELECT CASE and CASE");
	public static CompilerException WhileWithoutWEnd(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "WHILE without WEND");
	public static CompilerException WEndWithoutWhile(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "WEND without WHILE");
	public static CompilerException DoWithoutLoop(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "DO without LOOP");
	public static CompilerException LoopWithoutDo(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "LOOP without DO");
	public static CompilerException InvalidConstant(Token? context)
		=> new CompilerException(context, "Invalid constant");
	public static CompilerException EndSubOrEndFunctionMustBeLastLine(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "END SUB or END FUNCTION must be last line");
	public static CompilerException IllegalOutsideOfSubOrFunction(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Illegal outside of SUB/FUNCTION");
	public static CompilerException ExpectedStatement(Token? context)
		=> new CompilerException(context, "Expected: statement");
	public static CompilerException ExpectedVariable(Token? context)
		=> new CompilerException(context, "Expected: variable");
	public static CompilerException ArrayNotDefined(Token? context)
		=> new CompilerException(context, "Array not defined");
	public static CompilerException AnyIsNotSupported(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Parameters declared AS ANY are not supported by QBX");
}
