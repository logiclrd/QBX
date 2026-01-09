using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.BitwiseOperators;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Functions;
using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Compiled.RelationalOperators;
using QBX.ExecutionEngine.Compiled.Statements;

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

public class Compiler
{
	class TranslationInfo(CodeModel.CompilationElement element, Mapper rootMapper, Routine routine)
	{
		public CodeModel.CompilationElement Element = element;

		public Routine Routine = routine;

		public Mapper Mapper =
			(element.Type == CodeModel.CompilationElementType.Main)
			? rootMapper
			: rootMapper.CreateScope();
	}

	public Module Compile(CodeModel.CompilationUnit unit, Compilation compilation)
	{
		var module = new Module();

		var rootMapper = new Mapper();

		var routineByName = module.Routines;

		var translationInfo = new List<TranslationInfo>();

		// First pass: collect all routines
		foreach (var element in unit.Elements)
		{
			var routine = new Routine(element, compilation.TypeRepository);

			if (compilation.IsRegistered(routine.Name))
				throw CompilerException.DuplicateDefinition(element.AllStatements.FirstOrDefault());

			if (routine.Name == Routine.MainRoutineName)
				module.MainRoutine = routine;
			else
				routine.Register(compilation);

			var info = new TranslationInfo(element, rootMapper, routine);

			translationInfo.Add(info);
			routineByName[routine.Name] = routine;
		}

		// Second pass: process all TYPE definitions
		CodeModel.Statements.TypeStatement? typeStatement = null;
		var typeElementStatements = new List<CodeModel.Statements.TypeElementStatement>();

		foreach (var statement in unit.Elements[0].AllStatements)
		{
			if (typeStatement == null)
				typeStatement = statement as CodeModel.Statements.TypeStatement;
			else
			{
				switch (statement)
				{
					case CodeModel.Statements.TypeStatement:
						throw CompilerException.TypeWithoutEndType(typeStatement);

					case CodeModel.Statements.TypeElementStatement typeElementStatement:
						typeElementStatements.Add(typeElementStatement);
						break;

					case CodeModel.Statements.EndTypeStatement:
						TranslateTypeDefinition(typeStatement, typeElementStatements, compilation);

						typeStatement = null;
						typeElementStatements.Clear();

						break;
				}
			}
		}

		if (typeStatement != null)
			throw CompilerException.TypeWithoutEndType(typeStatement);

		// Third pass: process parameters, which requires that we know all the FUNCTIONs and UDTs
		foreach (var info in translationInfo)
		{
			if (info.Element.Type != CodeModel.CompilationElementType.Main)
			{
				info.Routine.TranslateParameters(info.Mapper, compilation);
				info.Mapper.LinkGlobalVariables();
			}

			if (info.Routine.ReturnType != null)
				info.Routine.ReturnValueVariableIndex = info.Mapper.DeclareVariable(info.Routine.Name, info.Routine.ReturnType);
		}

		foreach (var info in translationInfo)
		{
			var element = info.Element;

			var mapper = (element.Type == CodeModel.CompilationElementType.Main)
				? rootMapper
				: rootMapper.CreateScope();

			mapper.ScanForDisallowedSlugs(element.AllStatements);

			string routineName = Routine.GetName(element);

			var routine = routineByName[routineName];

			int lineIndex = 0;
			int statementIndex = 0;

			while (lineIndex < element.Lines.Count)
				TranslateStatement(element, ref lineIndex, ref statementIndex, routine, mapper, compilation);

			routine.VariableTypes = mapper.GetVariableTypes();
			routine.LinkedVariables = mapper.GetLinkedVariables();
		}

		compilation.Modules.Add(module);

		return module;
	}

	void TranslateTypeDefinition(CodeModel.Statements.TypeStatement typeStatement, List<CodeModel.Statements.TypeElementStatement> elements, Compilation compilation)
	{
		var typeRepository = compilation.TypeRepository;

		var udt = new UserDataType(typeStatement);

		foreach (var typeElementStatement in elements)
		{
			var type = typeRepository.ResolveType(
				typeElementStatement.ElementType,
				typeElementStatement.ElementUserType,
				isArray: false,
				typeElementStatement.TypeToken);

			udt.Fields.Add(
				new UserDataTypeField(
					typeElementStatement.Name,
					type));
		}

		typeRepository.RegisterType(udt);
	}

	void TranslateStatement(CodeModel.CompilationElement element, ref int lineIndexRef, ref int statementIndexRef, ISequence container, Mapper mapper, Compilation compilation)
	{
		int lineIndex = lineIndexRef;
		int statementIndex = statementIndexRef;

		if (lineIndex >= element.Lines.Count)
			return;

		var line = element.Lines[lineIndex];

		if (statementIndex >= line.Statements.Count)
		{
			lineIndexRef = lineIndex + 1;
			statementIndexRef = 0;
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

			bool HaveCurrentStatement()
			{
				return
					(lineIndex < element.Lines.Count) &&
					(statementIndex < line.Statements.Count);
			}

			TranslateStatement(element.Type, ref statement, Advance, HaveCurrentStatement, container, mapper, compilation, out var nextStatementInfo);

			if (nextStatementInfo != null)
			{
				throw CompilerException.NextWithoutFor(
					nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
			}
		}
		finally
		{
			lineIndexRef = lineIndex;
			statementIndexRef = statementIndex;
		}
	}

	void TranslateStatement(CodeModel.CompilationElementType elementType, IList<CodeModel.Statements.Statement> statements, ref int statementIndexRef, ISequence container, Mapper mapper, Compilation compilation)
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

			bool HaveCurrentStatement()
			{
				return (statementIndex < statements.Count);
			}

			TranslateStatement(elementType, ref statement, Advance, HaveCurrentStatement, container, mapper, compilation, out var nextStatementInfo);

			if (nextStatementInfo != null)
			{
				throw CompilerException.NextWithoutFor(
					nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
			}
		}
		finally
		{
			statementIndexRef = statementIndex;
		}
	}

	class NextStatementInfo(CodeModel.Statements.NextStatement statement)
	{
		public CodeModel.Statements.NextStatement Statement = statement;
		public int LoopsMatched = 0;
	}

	void TranslateStatement(CodeModel.CompilationElementType elementType, ref CodeModel.Statements.Statement statement, Func<bool> advance, Func<bool> haveCurrentStatement, ISequence container, Mapper mapper, Compilation compilation, out NextStatementInfo? nextStatementInfo)
	{
		var typeRepository = compilation.TypeRepository;

		nextStatementInfo = null;

		switch (statement)
		{
			case CodeModel.Statements.AssignmentStatement assignmentStatement:
			{
				var targetExpression = TranslateExpression(assignmentStatement.TargetExpression, mapper, compilation);
				var valueExpression = TranslateExpression(assignmentStatement.ValueExpression, mapper, compilation);

				if (targetExpression == null)
					throw new BadModelException("AssignmentStatement with no TargetExpression");
				if (valueExpression == null)
					throw new BadModelException("AssignmentStatement with no ValueExpression");

				if (targetExpression.Type != valueExpression.Type)
				{
					if (targetExpression.Type.IsString != valueExpression.Type.IsString)
						throw CompilerException.TypeMismatch(assignmentStatement.ValueExpression?.Token);
					if (!targetExpression.Type.IsPrimitiveType
					 || !valueExpression.Type.IsPrimitiveType)
						throw CompilerException.TypeMismatch(assignmentStatement.ValueExpression?.Token);

					valueExpression = Conversion.Construct(valueExpression, targetExpression.Type.PrimitiveType);
				}

				var translatedAssignmentStatement = new AssignmentStatement();

				translatedAssignmentStatement.TargetExpression = targetExpression;
				translatedAssignmentStatement.ValueExpression = valueExpression;

				container.Append(translatedAssignmentStatement);

				break;
			}
			case CodeModel.Statements.ColorStatement colorStatement:
			{
				var translatedColorStatement = new ColorStatement();

				var argument1 = colorStatement.Arguments.Count > 0 ? colorStatement.Arguments[0] : null;
				var argument2 = colorStatement.Arguments.Count > 1 ? colorStatement.Arguments[1] : null;
				var argument3 = colorStatement.Arguments.Count > 2 ? colorStatement.Arguments[2] : null;

				translatedColorStatement.Argument1Expression = TranslateExpression(argument1, mapper, compilation);
				translatedColorStatement.Argument2Expression = TranslateExpression(argument2, mapper, compilation);
				translatedColorStatement.Argument3Expression = TranslateExpression(argument3, mapper, compilation);

				container.Append(translatedColorStatement);

				break;
			}
			case CodeModel.Statements.ConstStatement constStatement:
			{
				foreach (var definition in constStatement.Definitions)
				{
					var translatedValue = TranslateExpression(definition.Value, mapper, compilation);

					if (translatedValue == null)
						throw new Exception("ConstStatement has no Value");

					mapper.DefineConstant(
						definition.Identifier,
						translatedValue.EvaluateConstant());
				}

				break;
			}
			case CodeModel.Statements.DeclareStatement declareStatement:
			{
				// If the declared routine is already known, verify that the parameters
				// and type match.
				//
				// If the declared routine is not known, then record the fact that the
				// routine should exist so that we know we can generate
				// UnresolvedCallStatements and UnresolvedFunctionCalls to be linked
				// up later.

				// TODO

				break;
			}
			case CodeModel.Statements.DefTypeStatement defTypeStatement:
			{
				var dataType = DataType.FromCodeModelDataType(defTypeStatement.DataType);

				if (!dataType.IsPrimitiveType)
					throw new Exception("DefTypeStatement's DataType is not a primitive type");

				foreach (var range in defTypeStatement.Ranges)
					mapper.SetIdentifierTypes(range.Start, range.End ?? range.Start, dataType.PrimitiveType);

				break;
			}
			case CodeModel.Statements.DimStatement dimStatement: // also matches RedimStatement
			{
				if (dimStatement.Shared && (elementType != CodeModel.CompilationElementType.Main))
					throw CompilerException.IllegalInSubFunctionOrDefFn(statement);

				foreach (var declaration in dimStatement.Declarations)
				{
					DataType dataType;

					if (declaration.UserType != null)
						dataType = compilation.TypeRepository.ResolveType(declaration.UserType);
					else
						dataType = DataType.ForPrimitiveDataType(mapper.GetTypeForIdentifier(declaration.Name));

					if (declaration.Subscripts != null)
						dataType = dataType.MakeArrayType();

					int variableIndex = mapper.DeclareVariable(declaration.Name, dataType);

					if (dimStatement.Shared)
						mapper.MakeGlobalVariable(declaration.Name);

					if (declaration.Subscripts != null)
					{
						var translatedDimStatement = new DimensionArrayStatement();

						translatedDimStatement.VariableIndex = variableIndex;

						// TODO: '$STATIC and '$DYNAMIC (can also be used with REM)
						// => when the following conditions are met, arrays are configured prior to execution commenting:
						//    - '$STATIC
						//    - the DIM statement is not in any sort of conditional compilation clause (IF, FOR, etc.)
						//    - the DIM statement is not inside a SUB, FUNCTION or DEF FN
						//    - all of the bounds expressions are evaluable at compile time
						//    - there is no preceding DIM statement for the same identifier
						// in this instance, we set up the array right here and now

						// TODO: if the array has already been initialized, then:
						// - if DimStatement then runtime error "Duplicate definition"
						// - if RedimStatement then dynamically resize
						// - RedimStatements fail on arrays set up statically (see preceding TODO)

						if (dimStatement is CodeModel.Statements.RedimStatement redimStatement)
						{
							translatedDimStatement.IsRedimension = true;
							translatedDimStatement.PreserveData = redimStatement.Preserve;
						}

						foreach (var subscript in declaration.Subscripts.Subscripts)
						{
							var bound1 = TranslateExpression(subscript.Bound1, mapper, compilation);
							var bound2 = TranslateExpression(subscript.Bound2, mapper, compilation);

							if (bound1 == null)
								throw new Exception("Must specify the first bound for an array subscript");

							if (bound2 == null)
								translatedDimStatement.Subscripts.Add(new IntegerLiteralValue(0), bound1);
							else
								translatedDimStatement.Subscripts.Add(bound1, bound2);
						}

						container.Append(translatedDimStatement);
					}
				}

				break;
			}
			case CodeModel.Statements.ElseStatement: // these are normally subsumed by IfStatement parsing
			case CodeModel.Statements.ElseIfStatement:
				throw new RuntimeException(statement, "ELSE without IF");
			case CodeModel.Statements.EmptyStatement:
				break;
			case CodeModel.Statements.IfStatement ifStatement:
			{
				var translatedIfStatement = new IfStatement();

				translatedIfStatement.Condition = TranslateExpression(ifStatement.ConditionExpression, mapper, compilation);

				if (ifStatement.ThenBody == null)
				{
					// Block IF/ELSEIF/ELSE/END IF

					var block = translatedIfStatement;
					var subsequence = new Sequence();

					translatedIfStatement.ThenBody = subsequence;

					var blame = statement;

					advance();

					while (haveCurrentStatement())
					{
						if (statement is CodeModel.Statements.EndIfStatement)
						{
							blame = null;
							break;
						}

						switch (statement)
						{
							case CodeModel.Statements.ElseStatement:
							{
								if (block.ElseBody != null)
									throw new RuntimeException(statement, "ELSE without IF");

								blame = statement;

								subsequence = new Sequence();

								block.ElseBody = subsequence;

								if (!advance())
									throw CompilerException.BlockIfWithoutEndIf(blame);

								break;
							}
							case CodeModel.Statements.ElseIfStatement elseIfStatement:
							{
								// Transform: ELSEIF becomes an IF statement in an ELSE block.
								//
								// The block variable points at the IF statement that owns the
								// current ThenBody. With this transform, block switches to
								// pointing at the nested IfStatement.

								blame = statement;

								var elseBody = new Sequence();

								block.ElseBody = elseBody;

								block = new IfStatement();
								block.Condition = TranslateExpression(elseIfStatement.ConditionExpression, mapper, compilation);

								elseBody.Append(block);

								subsequence = new Sequence();

								block.ThenBody = subsequence;

								if (elseIfStatement.ThenBody != null)
								{
									// Weird syntax: ELSEIF can have an inline THEN block. The statements are
									// just part of the multi-line THEN block up to the next ELSEIF/ELSE/END IF.
									int idx = 0;

									while (idx < elseIfStatement.ThenBody.Count)
										TranslateStatement(elementType, elseIfStatement.ThenBody, ref idx, subsequence, mapper, compilation);
								}

								if (!advance())
									throw CompilerException.BlockIfWithoutEndIf(blame);

								break;
							}

							default:
							{
								TranslateStatement(elementType, ref statement, advance, haveCurrentStatement, subsequence, mapper, compilation, out nextStatementInfo);

								if (nextStatementInfo != null)
								{
									throw CompilerException.NextWithoutFor(
										nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
								}

								break;
							}
						}
					}

					if (blame != null)
						throw CompilerException.BlockIfWithoutEndIf(blame);
				}
				else
				{
					// Inline IF statement.

					var thenBody = new Sequence();

					int idx = 0;

					while (idx < ifStatement.ThenBody.Count)
						TranslateStatement(elementType, ifStatement.ThenBody, ref idx, thenBody, mapper, compilation);

					translatedIfStatement.ThenBody = thenBody;

					if (ifStatement.ElseBody != null)
					{
						var elseBody = new Sequence();

						idx = 0;

						while (idx < ifStatement.ThenBody.Count)
							TranslateStatement(elementType, ifStatement.ElseBody, ref idx, elseBody, mapper, compilation);

						translatedIfStatement.ElseBody = elseBody;
					}
				}

				container.Append(translatedIfStatement);

				break;
			}
			case CodeModel.Statements.ForStatement forStatement:
			{
				var iteratorVariableIndex = mapper.ResolveVariable(forStatement.CounterVariable);

				var fromExpression = TranslateExpression(forStatement.StartExpression, mapper, compilation);
				var toExpression = TranslateExpression(forStatement.EndExpression, mapper, compilation);
				var stepExpression = TranslateExpression(forStatement.StepExpression, mapper, compilation);

				if (fromExpression == null)
					throw new Exception("ForStatement with no StartExpression");
				if (toExpression == null)
					throw new Exception("ForStatement with no EndExpression");

				var body = new Sequence();

				advance();

				while (haveCurrentStatement())
				{
					if (statement is CodeModel.Statements.NextStatement nextStatement)
					{
						if (nextStatement.CounterExpressions.Count > 0)
						{
							if (nextStatement.CounterExpressions[0] is not CodeModel.Expressions.IdentifierExpression identifierExpression)
								throw new BadModelException("NextStatement has a CounterExpression that is not an IdentifierExpression");

							if (mapper.ResolveVariable(identifierExpression.Identifier) != iteratorVariableIndex)
								throw CompilerException.NextWithoutFor(identifierExpression.Token);

							if (nextStatement.CounterExpressions.Count > 1)
							{
								nextStatementInfo = new NextStatementInfo(nextStatement);
								nextStatementInfo.LoopsMatched = 1;
							}
						}

						break;
					}

					TranslateStatement(elementType, ref statement, advance, haveCurrentStatement, body, mapper, compilation, out nextStatementInfo);

					if (nextStatementInfo != null)
					{
						nextStatement = nextStatementInfo.Statement;

						int idx = nextStatementInfo.LoopsMatched;

						if (nextStatement.CounterExpressions[idx] is not CodeModel.Expressions.IdentifierExpression identifierExpression)
							throw new BadModelException("NextStatement has a CounterExpression that is not an IdentifierExpression");

						if (mapper.ResolveVariable(identifierExpression.Identifier) != iteratorVariableIndex)
							throw CompilerException.NextWithoutFor(identifierExpression.Token);

						if (idx + 1 < nextStatement.CounterExpressions.Count)
							nextStatementInfo.LoopsMatched++;
						else
							nextStatementInfo = null;
					}
				}

				var translatedForStatement = ForStatement.Construct(
					iteratorVariableIndex,
					mapper.GetTypeForIdentifier(forStatement.CounterVariable),
					fromExpression,
					toExpression,
					stepExpression,
					body);

				container.Append(translatedForStatement);

				break;
			}
			case CodeModel.Statements.PixelSetStatement pixelSetStatement:
			{
				var translatedPixelSetStatement = new PixelSetStatement();

				translatedPixelSetStatement.StepCoordinates = pixelSetStatement.StepCoordinates;
				translatedPixelSetStatement.XExpression = TranslateExpression(pixelSetStatement.XExpression, mapper, compilation);
				translatedPixelSetStatement.YExpression = TranslateExpression(pixelSetStatement.YExpression, mapper, compilation);
				translatedPixelSetStatement.ColourExpression = TranslateExpression(pixelSetStatement.ColourExpression, mapper, compilation);
				translatedPixelSetStatement.UseForegroundColour =
					(pixelSetStatement.DefaultColour == CodeModel.Statements.PixelSetDefaultColour.Foreground);

				container.Append(translatedPixelSetStatement);

				break;
			}
			case CodeModel.Statements.PrintStatement printStatement:
			{
				if (printStatement.FileNumberExpression != null)
					throw new NotImplementedException("TODO");

				PrintArgument TranslatePrintArgument(CodeModel.Statements.PrintArgument argument)
				{
					var translatedArgument = new PrintArgument();

					translatedArgument.Expression = TranslateExpression(argument.Expression, mapper, compilation);
					translatedArgument.CursorAction =
						argument.CursorAction switch
						{
							CodeModel.Statements.PrintCursorAction.None => PrintCursorAction.None,
							CodeModel.Statements.PrintCursorAction.NextZone => PrintCursorAction.NextZone,
							CodeModel.Statements.PrintCursorAction.NextLine => PrintCursorAction.NextLine,

							_ => throw new Exception("Internal error")
						};

					return translatedArgument;
				}


				if (printStatement.UsingExpression == null)
				{
					var translatedPrintStatement = new UnformattedPrintStatement();

					foreach (var argument in printStatement.Arguments)
					{
						translatedPrintStatement.Arguments.Add(
							TranslatePrintArgument(argument));
					}

					container.Append(translatedPrintStatement);
				}
				else
				{
					var translatedPrintStatement = new FormattedPrintStatement();

					translatedPrintStatement.Format = TranslateExpression(printStatement.UsingExpression, mapper, compilation);

					if (printStatement.Arguments.Count > 0)
					{
						translatedPrintStatement.EmitNewLine =
							(printStatement.Arguments.Last().CursorAction == CodeModel.Statements.PrintCursorAction.NextLine);

						foreach (var argument in printStatement.Arguments)
						{
							translatedPrintStatement.Arguments.Add(
								TranslatePrintArgument(argument));
						}
					}

					container.Append(translatedPrintStatement);
				}

				break;
			}
			case CodeModel.Statements.ScreenStatement screenStatement:
			{
				var translatedScreenStatement = new ScreenStatement();

				translatedScreenStatement.ModeExpression = TranslateExpression(screenStatement.ModeExpression, mapper, compilation);
				translatedScreenStatement.ColourSwitchExpression = TranslateExpression(screenStatement.ColourSwitchExpression, mapper, compilation);
				translatedScreenStatement.ActivePageExpression = TranslateExpression(screenStatement.ActivePageExpression, mapper, compilation);
				translatedScreenStatement.VisiblePageExpression = TranslateExpression(screenStatement.VisiblePageExpression, mapper, compilation);

				container.Append(translatedScreenStatement);

				break;
			}
			case CodeModel.Statements.TypeStatement typeStatement:
			{
				// TODO: track whether we are in a DEF FN
				if (elementType != CodeModel.CompilationElementType.Main)
					throw CompilerException.IllegalInSubFunctionOrDefFn(statement);

				// Types are gathered in a separate pass, since they need to be known before
				// SUB and FUNCTION parameters are processed. Here, we just skip over them.

				while (advance())
				{
					if ((statement is CodeModel.Statements.EmptyStatement) || (statement is CodeModel.Statements.CommentStatement))
						continue;
					if (statement is CodeModel.Statements.EndTypeStatement)
						break;

					if (statement is not CodeModel.Statements.TypeElementStatement typeElementStatement)
						throw CompilerException.StatementIllegalInTypeBlock(statement);
				}

				if (statement is not CodeModel.Statements.EndTypeStatement)
					throw new RuntimeException(typeStatement, "Unterminated TYPE definition");

				break;
			}

			default: throw new NotImplementedException("Statement not implemented: " + statement.Type);
		}

		advance();
	}

	private IEvaluable? TranslateExpression(CodeModel.Expressions.Expression? expression, Mapper mapper, Compilation compilation, bool createImplicitArray = false)
	{
		if (expression == null)
			return null;

		switch (expression)
		{
			case CodeModel.Expressions.LiteralExpression literal:
				return LiteralValue.ConstructFromCodeModel(literal);

			case CodeModel.Expressions.IdentifierExpression identifier:
			{
				string qualifiedIdentifier = mapper.QualifyIdentifier(identifier.Identifier);
				string unqualifiedIdentifier = mapper.StripTypeCharacter(identifier.Identifier);

				if (mapper.TryResolveConstant(qualifiedIdentifier, out var literal))
					return literal;

				if (compilation.Functions.TryGetValue(unqualifiedIdentifier, out var function))
				{
					var returnType = function.ReturnType ?? throw new Exception("Internal error: function with no return type");

					string qualifiedFunction = mapper.QualifyIdentifier(function.Name, function.ReturnType);

					if (qualifiedIdentifier != qualifiedFunction)
						throw CompilerException.DuplicateDefinition(expression.Token);

					if (function.ParameterTypes.Count > 0)
						throw CompilerException.ArgumentCountMismatch(expression.Token);
				}

				int variableIndex = mapper.ResolveVariable(identifier.Identifier);
				var variableType = mapper.GetVariableType(variableIndex);

				return new IdentifierExpression(variableIndex, variableType);
			}

			case CodeModel.Expressions.KeywordFunctionExpression keywordFunction:
			{
				switch (keywordFunction.Function)
				{
					case TokenType.RND: return RndFunction.NoParameterInstance;

					default: throw new NotImplementedException("Keyword function: " + keywordFunction.Function);
				}
			}

			case CodeModel.Expressions.UnaryExpression unaryExpression:
			{
				var right = TranslateExpression(unaryExpression.Child, mapper, compilation);

				if (right == null)
					throw new Exception("Internal error: Unary expression operand translated to null");

				switch (unaryExpression.Operator)
				{
					case CodeModel.Expressions.Operator.Negate: return Negation.Construct(right);
					case CodeModel.Expressions.Operator.Not: return Not.Construct(right);

					default: throw new Exception("Internal error: Unrecognized unary expression operator " + unaryExpression.Operator);
				}
			}

			case CodeModel.Expressions.BinaryExpression binaryExpression:
			{
				if (binaryExpression.Operator == CodeModel.Expressions.Operator.Field)
				{
					string? dottedIdentifier = CollapseDottedIdentifierExpression(binaryExpression);

					if (dottedIdentifier != null)
					{
						if (mapper.TryResolveConstant(dottedIdentifier, out var literal))
							return literal;

						int variableIndex = mapper.ResolveVariable(dottedIdentifier);
						var variableType = mapper.GetVariableType(variableIndex);

						return new IdentifierExpression(variableIndex, variableType);
					}
					else
					{
						var subjectExpression = TranslateExpression(binaryExpression.Left, mapper, compilation);

						if (binaryExpression.Right is not CodeModel.Expressions.IdentifierExpression identifierExpression)
							throw new Exception("Member access expressions require the right-hand operand to be an identifier");

						string identifier = identifierExpression.Identifier;

						return FieldAccessExpression.Construct(subjectExpression, identifier);
					}
				}

				var left = TranslateExpression(binaryExpression.Left, mapper, compilation);
				var right = TranslateExpression(binaryExpression.Right, mapper, compilation);

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

					case CodeModel.Expressions.Operator.Equals: return Compiled.RelationalOperators.Equals.Construct(left, right);
					case CodeModel.Expressions.Operator.NotEquals: return NotEquals.Construct(left, right);
					case CodeModel.Expressions.Operator.LessThan: return LessThan.Construct(left, right);
					case CodeModel.Expressions.Operator.LessThanOrEquals: return LessThanOrEquals.Construct(left, right);
					case CodeModel.Expressions.Operator.GreaterThan: return GreaterThan.Construct(left, right);
					case CodeModel.Expressions.Operator.GreaterThanOrEquals: return GreaterThanOrEquals.Construct(left, right);

					case CodeModel.Expressions.Operator.And: return And.Construct(left, right);
					case CodeModel.Expressions.Operator.Or: return Or.Construct(left, right);
					case CodeModel.Expressions.Operator.ExclusiveOr: return ExclusiveOr.Construct(left, right);
					case CodeModel.Expressions.Operator.Equivalent: return Equivalent.Construct(left, right);
					case CodeModel.Expressions.Operator.Implies: return Implies.Construct(left, right);

					default: throw new Exception("Internal error: Unrecognized binary expression operator " + binaryExpression.Operator);
				}
			}
		}

		throw new Exception("Internal error: Can't translate expression");
	}

	internal string? CollapseDottedIdentifierExpression(CodeModel.Expressions.BinaryExpression binaryExpression)
	{
		StringBuilder? builder = null;

		CollapseDottedIdentifierExpression(binaryExpression, ref builder);

		return builder?.ToString();
	}

	void CollapseDottedIdentifierExpression(CodeModel.Expressions.BinaryExpression binaryExpression, ref StringBuilder? identifierBuilder)
	{
		// The specific pattern we're looking for is a left tree of field access expressions where
		// every leaf is an identifier. If we identify that the tree has the correct operator and
		// an identifier on the right, then we can just recursively process the left subtree.

		if (binaryExpression.Operator != CodeModel.Expressions.Operator.Field)
			return;

		if (binaryExpression.Right is not CodeModel.Expressions.IdentifierExpression rightIdentifier)
			return;

		switch (binaryExpression.Left)
		{
			case CodeModel.Expressions.IdentifierExpression leftIdentifier:
				identifierBuilder = new StringBuilder(leftIdentifier.Identifier);
				break;
			case CodeModel.Expressions.BinaryExpression leftBinary:
				CollapseDottedIdentifierExpression(leftBinary, ref identifierBuilder);
				break;
		}

		if (identifierBuilder != null)
			identifierBuilder.Append('.').Append(rightIdentifier.Identifier);
	}
}
