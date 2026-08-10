using System;

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

[Serializable]
public class CompilerException : Exception
{
	public Token? Context { get; private set; }
	public int ContextLength { get; private set; }
	public string? HelpContextString { get; private set; }

	public CompilerException(string message, string? helpContextString = null)
		: this(default(Token), message, helpContextString)
	{
	}

	public CompilerException(CodeModel.Statements.Statement? statement, string message, string? helpContextString = null)
		: this(
				statement?.FirstToken,
				statement?.SourceLength ?? 0,
				message,
				helpContextString)
	{
	}

	public CompilerException(CodeModel.Expressions.Expression? expression, string message, string? helpContextString = null)
		: this(
				expression?.Token,
				expression?.Token?.Length ?? 0,
				message,
				helpContextString)
	{
	}

	public CompilerException(Token? context, string message, string? helpContextString = null)
		: this(context, context?.Length ?? 0, message, helpContextString)
	{
	}

	public CompilerException(Token? context, int contextLength, string message, string? helpContextString = null)
		: base(message)
	{
		Context = context;
		ContextLength = contextLength;
		HelpContextString = helpContextString;
	}

	public CompilerException AddContext(CodeModel.Statements.Statement? statement)
		=> AddContext(statement?.FirstToken, statement?.SourceLength);

	public CompilerException AddContext(CodeModel.Expressions.Expression? expression)
		=> AddContext(expression?.Token);

	public CompilerException AddContext(Token? context, int? contextLength = null)
	{
		if (Context == null)
		{
			Context = context;
			ContextLength = contextLength ?? context?.Length ?? 0;
		}

		return this;
	}

	public static CompilerException ExpectedStatement(Token? context)
		=> new CompilerException(context, "Expected: statement");
	public static CompilerException ExpectedVariable(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Expected: variable");
	public static CompilerException ExpectedVariable(Token? context)
		=> new CompilerException(context, "Expected: variable");
	public static CompilerException AnyIsNotSupported(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Parameters declared AS ANY are not supported by QBX");

	public static CompilerException NextWithoutFor(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "NEXT without FOR", "-2001");
	public static CompilerException NextWithoutFor(Token? context)
		=> new CompilerException(context, "NEXT without FOR", "-2001");
	public static CompilerException SyntaxError(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Syntax error", "-2002");
	public static CompilerException IllegalFunctionCall(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Illegal function call", "-2005");
	public static CompilerException Overflow(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Overflow", "-2006");
	public static CompilerException Overflow(Token? context)
		=> new CompilerException(context, "Overflow", "-2006");
	public static CompilerException LabelNotDefined(Token? context)
		=> new CompilerException(context, "Label not defined", "-2008");
	public static CompilerException DuplicateDefinition(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Duplicate definition", "-2010");
	public static CompilerException DuplicateDefinition(Token? context)
		=> new CompilerException(context, "Duplicate definition", "-2010");
	public static CompilerException DivisionByZero(Token? context)
		=> new CompilerException(context, "Division by zero", "-2011");
	public static CompilerException TypeMismatch(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Type mismatch", "-2013");
	public static CompilerException TypeMismatch(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Type mismatch", "-2013");
	public static CompilerException TypeMismatch(Token? context)
		=> new CompilerException(context, "Type mismatch", "-2013");
	public static CompilerException TypeMismatch()
		=> new CompilerException("Type mismatch", "-2013");
	public static CompilerException ForWithoutNext(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "FOR without NEXT", "-2026");
	public static CompilerException WhileWithoutWEnd(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "WHILE without WEND", "-2029");
	public static CompilerException WEndWithoutWhile(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "WEND without WHILE", "-2030");
	public static CompilerException DuplicateLabel(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Duplicate label", "-2033");
	public static CompilerException SubprogramNotDefined(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Subprogram not defined", "-2035");
	public static CompilerException SubprogramNotDefined(Token? context)
		=> new CompilerException(context, "Subprogram not defined", "-2035");
	public static CompilerException ArgumentCountMismatch(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Argument count mismatch", "-2037");
	public static CompilerException ArgumentCountMismatch(Token? context)
		=> new CompilerException(context, "Argument count mismatch", "-2037");
	public static CompilerException ArrayNotDefined(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Array not defined", "-2038");
	public static CompilerException ArrayNotDefined(Token? context)
		=> new CompilerException(context, "Array not defined", "-2038");

	public static CompilerException IdentifierCannotIncludePeriod(Token? context)
		=> new CompilerException(context, "Identifier cannot include period", "-117");
	public static CompilerException IllegalNumber(Token? context)
		=> new CompilerException(context, "Illegal number", "-119");
	public static CompilerException IllegalNumber(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Illegal number", "-119");
	public static CompilerException InvalidConstant(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Invalid constant", "-120");
	public static CompilerException InvalidConstant(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Invalid constant", "-120");
	public static CompilerException InvalidConstant(Token? context)
		=> new CompilerException(context, "Invalid constant", "-120");
	public static CompilerException IllegalOutsideOfSubOrFunction(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Illegal outside of SUB/FUNCTION", "-150");
	public static CompilerException IllegalInSubFunctionOrDefFn(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "Illegal in SUB, FUNCTION or DEF FN", "-151");
	public static CompilerException ElementNotDefined(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Element not defined", "-155");
	public static CompilerException TypeNotDefined(Token? context)
		=> new CompilerException(context, "Type not defined", "-156");
	public static CompilerException EndSubOrEndFunctionMustBeLastLine(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "END SUB or END FUNCTION must be last line", "-158");
	public static CompilerException TypeWithoutEndType(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "TYPE without END TYPE", "-159");
	public static CompilerException StatementIllegalInTypeBlock(CodeModel.Statements.Statement statement)
		=> new CompilerException(statement, "Statement illegal in TYPE block", "-161");
	public static CompilerException MetacommandError()
		=> new CompilerException("$Metacommand error", "-165");
	public static CompilerException ArrayAlreadyDimensioned(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Array already dimensioned", "-167");
	public static CompilerException ArrayAlreadyDimensioned(Token? context)
		=> new CompilerException(context, "Array already dimensioned", "-167");
	public static CompilerException EndIfWithoutBlockIf(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "END IF without block IF", "-169");
	public static CompilerException BlockIfWithoutEndIf(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Block IF without END IF", "-170");
	public static CompilerException ElseWithoutIf(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "ELSE without IF", "-171");
	public static CompilerException ExitDoNotWithinDoLoop(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "EXIT DO not within DO...LOOP", "-172");
	public static CompilerException ExitForNotWithinForNext(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "EXIT FOR not within FOR...NEXT", "-173");
	public static CompilerException DoWithoutLoop(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "DO without LOOP", "-174");
	public static CompilerException LoopWithoutDo(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "LOOP without DO", "-175");
	public static CompilerException SelectWithoutEndSelect(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "SELECT without END SELECT", "-176");
	public static CompilerException CaseWithoutSelect(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "CASE without SELECT", "-177");
	public static CompilerException EndSelectWithoutSelect(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "END SELECT without SELECT", "-178");
	public static CompilerException FixedLengthStringIllegal(CodeModel.Expressions.Expression? expression)
		=> new CompilerException(expression, "Fixed-length string illegal", "-179");
	public static CompilerException ParameterTypeMismatch(Token? context)
		=> new CompilerException(context, "Parameter type mismatch", "-182");
	public static CompilerException StatementsAndLabelsIllegalBetweenSelectCaseAndCase(CodeModel.Statements.Statement? statement)
		=> new CompilerException(statement, "Statements/labels illegal between SELECT CASE and CASE", "-186");
}
