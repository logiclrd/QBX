using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Functions;
using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Compiled.Statements;

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

public class Compiler
{
	public Module Compile(CodeModel.CompilationUnit unit, TypeRepository typeRepository)
	{
		var module = new Module();

		var unresolvedCallStatements = new List<CallStatement>();

		var rootMapper = new Mapper();

		var routineByName = module.Routines;

		// First pass: collect all routines
		foreach (var element in unit.Elements)
		{
			var routine = new Routine(element, typeRepository);

			if (rootMapper.IsRegistered(routine.Name))
				throw RuntimeException.DuplicateDefinition(element.AllStatements.FirstOrDefault());

			if (routine.Name == Routine.MainRoutineName)
				module.MainRoutine = routine;
			else
				routine.Register(rootMapper);

			routineByName[routine.Name] = routine;
		}

		// Second pass: process parameters, which requires that we know all the FUNCTIONs
		foreach (var routine in rootMapper.AllRegisteredRoutines)
			routine.TranslateParameters(rootMapper, typeRepository);

		foreach (var element in unit.Elements)
		{
			var mapper = (element.Type == CodeModel.CompilationElementType.Main)
				? rootMapper
				: rootMapper.CreateScope();

			string routineName = Routine.GetName(element);

			var routine = routineByName[routineName];

			int lineIndex = 0;
			int statementIndex = 0;

			while (lineIndex < element.Lines.Count)
				TranslateStatement(element, ref lineIndex, ref statementIndex, routine, mapper, typeRepository);
		}

		return module;
	}

	void TranslateStatement(CodeModel.CompilationElement element, ref int lineIndexRef, ref int statementIndexRef, ISequence container, Mapper mapper, TypeRepository typeRepository)
	{
		int lineIndex = lineIndexRef;
		int statementIndex = statementIndexRef;

		if (lineIndex >= element.Lines.Count)
			return;

		var line = element.Lines[lineIndex];

		if (statementIndex >= line.Statements.Count)
		{
			lineIndex++;
			statementIndex = 0;
			return;
		}

		try
		{
			var statement = line.Statements[statementIndex];

			bool Advance()
			{
				statementIndex++;

				while (statementIndex >= line.Statements.Count)
				{
					lineIndex++;
					statementIndex = 0;

					if (lineIndex >= element.Lines.Count)
						return false;

					line = element.Lines[lineIndex];
				}

				statement = line.Statements[statementIndex];

				return true;
			}

			TranslateStatement(element.Type, ref statement, Advance, container, mapper, typeRepository);
		}
		finally
		{
			lineIndexRef = lineIndex;
			statementIndexRef = statementIndex;
		}
	}

	void TranslateStatement(CodeModel.CompilationElementType elementType, IList<CodeModel.Statements.Statement> statements, ref int statementIndexRef, ISequence container, Mapper mapper, TypeRepository typeRepository)
	{
		int statementIndex = statementIndexRef;

		if (statementIndex >= statements.Count)
			return;

		try
		{
			var statement = statements[statementIndex];

			bool Advance()
			{
				statementIndex++;

				if (statementIndex < statements.Count)
				{
					statement = statements[statementIndex];
					return true;
				}
				else
					return false;
			}

			TranslateStatement(elementType, ref statement, Advance, container, mapper, typeRepository);
		}
		finally
		{
			statementIndexRef = statementIndex;
		}
	}

	void TranslateStatement(CodeModel.CompilationElementType elementType, ref CodeModel.Statements.Statement statement, Func<bool> advance, ISequence container, Mapper mapper, TypeRepository typeRepository)
	{
		switch (statement)
		{
			case CodeModel.Statements.DefTypeStatement defTypeStatement:
			{
				var dataType = DataType.FromCodeModelDataType(defTypeStatement.DataType);

				if (!dataType.IsPrimitiveType)
					throw new Exception("DefTypeStatement's DataType is not a primitive type");

				foreach (var range in defTypeStatement.Ranges)
					mapper.SetIdentifierTypes(range.Start, range.End ?? range.Start, dataType.PrimitiveType);

				break;
			}
			case CodeModel.Statements.ColorStatement colorStatement:
			{
				var translatedColorStatement = new ColorStatement();

				var argument1 = colorStatement.Arguments.Count > 0 ? colorStatement.Arguments[0] : null;
				var argument2 = colorStatement.Arguments.Count > 1 ? colorStatement.Arguments[1] : null;
				var argument3 = colorStatement.Arguments.Count > 2 ? colorStatement.Arguments[2] : null;

				translatedColorStatement.Argument1Expression = TranslateExpression(argument1);
				translatedColorStatement.Argument2Expression = TranslateExpression(argument2);
				translatedColorStatement.Argument3Expression = TranslateExpression(argument3);

				container.Append(translatedColorStatement);

				break;
			}
			case CodeModel.Statements.ElseStatement: // these are normally subsumed by IfStatement parsing
			case CodeModel.Statements.ElseIfStatement:
				throw new RuntimeException(statement, "ELSE without IF");
			case CodeModel.Statements.IfStatement ifStatement:
			{
				var translatedIfStatement = new IfStatement();

				translatedIfStatement.Condition = TranslateExpression(ifStatement.ConditionExpression);

				if (ifStatement.ThenBody == null)
				{
					// Block IF/ELSEIF/ELSE/END IF

					var block = translatedIfStatement;
					var subsequence = new Sequence();

					translatedIfStatement.ThenBody = subsequence;

					while (advance())
					{
						if (statement is CodeModel.Statements.EndIfStatement)
							break;

						switch (statement)
						{
							case CodeModel.Statements.ElseStatement:
							{
								if (translatedIfStatement.ElseBody != null)
									throw new RuntimeException(statement, "ELSE without IF");

								subsequence = new Sequence();

								translatedIfStatement.ElseBody = subsequence;

								break;
							}
							case CodeModel.Statements.ElseIfStatement elseIfStatement:
							{
								// Transform: ELSEIF becomes an IF statement in an ELSE block.
								//
								// The block variable points at the IF statement that owns the
								// current ThenBody. With this transform, block switches to
								// pointing at the nested IfStatement.

								var elseBody = new Sequence();

								block.ElseBody = elseBody;

								block = new IfStatement();
								block.Condition = TranslateExpression(ifStatement.ConditionExpression);

								elseBody.Append(block);

								subsequence = new Sequence();

								block.ThenBody = subsequence;

								if (elseIfStatement.ThenBody != null)
								{
									// Weird syntax: ELSEIF can have an inline THEN block. The statements are
									// just part of the multi-line THEN block up to the next ELSEIF/ELSE/END IF.
									int idx = 0;

									while (idx < elseIfStatement.ThenBody.Count)
										TranslateStatement(elementType, elseIfStatement.ThenBody, ref idx, subsequence, mapper, typeRepository);
								}

								break;
							}
						}
					}

					container.Append(translatedIfStatement);
				}
				else
				{
					// Inline IF statement.

					var thenBody = new Sequence();

					int idx = 0;

					while (idx < ifStatement.ThenBody.Count)
						TranslateStatement(elementType, ifStatement.ThenBody, ref idx, thenBody, mapper, typeRepository);

					translatedIfStatement.ThenBody = thenBody;

					if (ifStatement.ElseBody != null)
					{
						var elseBody = new Sequence();

						idx = 0;

						while (idx < ifStatement.ThenBody.Count)
							TranslateStatement(elementType, ifStatement.ElseBody, ref idx, elseBody, mapper, typeRepository);

						translatedIfStatement.ElseBody = elseBody;
					}
				}

				break;
			}
			case CodeModel.Statements.ScreenStatement screenStatement:
			{
				var translatedScreenStatement = new ScreenStatement();

				translatedScreenStatement.ModeExpression = TranslateExpression(screenStatement.ModeExpression);
				translatedScreenStatement.ColourSwitchExpression = TranslateExpression(screenStatement.ColourSwitchExpression);
				translatedScreenStatement.ActivePageExpression = TranslateExpression(screenStatement.ActivePageExpression);
				translatedScreenStatement.VisiblePageExpression = TranslateExpression(screenStatement.VisiblePageExpression);

				container.Append(translatedScreenStatement);

				break;
			}
			case CodeModel.Statements.TypeStatement typeStatement:
			{
				// TODO: track whether we are in a DEF FN
				if (elementType != CodeModel.CompilationElementType.Main)
					throw new RuntimeException(statement, "Illegal in SUB, FUNCTION or DEF FN");

				var udt = new UserDataType(typeStatement);

				while (advance())
				{
					if ((statement is CodeModel.Statements.EmptyStatement) || (statement is CodeModel.Statements.CommentStatement))
						continue;
					if (statement is CodeModel.Statements.EndTypeStatement)
						break;

					if (statement is CodeModel.Statements.TypeElementStatement typeElementStatement)
					{
						var type = typeRepository.ResolveType(
							typeElementStatement.ElementType,
							typeElementStatement.ElementUserType,
							isArray: false,
							typeElementStatement.TypeToken);

						udt.Members.Add(
							new UserDataTypeMember(
								typeElementStatement.Name,
								type));
					}
				}

				if (statement is not CodeModel.Statements.EndTypeStatement)
					throw new RuntimeException(typeStatement, "Unterminated TYPE definition");

				typeRepository.RegisterType(udt);

				break;
			}
		}

		advance();
	}

	private IEvaluable? TranslateExpression(CodeModel.Expressions.Expression? expression)
	{
		if (expression == null)
			return null;

		switch (expression)
		{
			case CodeModel.Expressions.LiteralExpression literal:
				return LiteralValue.ConstructFromCodeModel(literal);

			case CodeModel.Expressions.KeywordFunctionExpression keywordFunction:
			{
				switch (keywordFunction.Function)
				{
					case TokenType.RND: return RndFunction.NoParameterInstance;

					default: throw new NotImplementedException("Keyword function: " + keywordFunction.Function);
				}
			}

			case CodeModel.Expressions.BinaryExpression binaryExpression:
			{
				var left = TranslateExpression(binaryExpression.Left);
				var right = TranslateExpression(binaryExpression.Right);

				if ((left == null) || (right == null))
					throw new Exception("Internal error: Binary expression operand translated to null");

				switch (binaryExpression.Operator)
				{
					case CodeModel.Expressions.Operator.Add: return Addition.Construct(left, right);
					case CodeModel.Expressions.Operator.Subtract: return Subtraction.Construct(left, right);
					case CodeModel.Expressions.Operator.Multiply: return Multiplication.Construct(left, right);
					case CodeModel.Expressions.Operator.Divide: return Division.Construct(left, right);
					case CodeModel.Expressions.Operator.Exponentiate: return Exponentiation.Construct(left, right);
					case CodeModel.Expressions.Operator.IntegerDivide: return IntegerDivision.Construct(left, right);
					case CodeModel.Expressions.Operator.Modulo: return Modulo.Construct(left, right);

					case CodeModel.Expressions.Operator.Equals:
					case CodeModel.Expressions.Operator.NotEquals:
					case CodeModel.Expressions.Operator.LessThan:
					case CodeModel.Expressions.Operator.LessThanOrEquals:
					case CodeModel.Expressions.Operator.GreaterThan:
					case CodeModel.Expressions.Operator.GreaterThanOrEquals:

					case CodeModel.Expressions.Operator.Not:
					case CodeModel.Expressions.Operator.And:
					case CodeModel.Expressions.Operator.Or:
					case CodeModel.Expressions.Operator.ExclusiveOr:
					case CodeModel.Expressions.Operator.Equivalent:
					case CodeModel.Expressions.Operator.Implies:

					default: throw new Exception("Internal error: Unrecognized binary expression operator " + binaryExpression.Operator);
				}
			}
		}

		throw new Exception("Internal error: Can't translate expression");
	}
}
