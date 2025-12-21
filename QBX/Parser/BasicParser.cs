using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;

namespace QBX.Parser;

public class BasicParser
{
	public CompilationUnit Parse(IEnumerable<Token> tokenStream)
	{
		var unit = new CompilationUnit();

		var mainElement = new CompilationElement();

		mainElement.Type = CompilationElementType.Main;

		unit.Elements.Add(mainElement);

		var element = mainElement;

		var comments = new List<CodeLine>();

		tokenStream = tokenStream.Where(token => token.Type != TokenType.Whitespace);

		foreach (var line in ParseCodeLines(tokenStream))
		{
			if (line.IsCommentLine && (element == mainElement))
				comments.Add(line);
			else
			{
				if (line.Statements.Any())
				{
					var firstStatement = line.Statements.First();
					var lastStatement = line.Statements.Last();

					if ((firstStatement.Type == StatementType.Sub) || (firstStatement.Type == StatementType.Function))
					{
						element = new CompilationElement();
						unit.Elements.Add(element);

						element.AddLines(comments);
						comments.Clear();

						switch (firstStatement.Type)
						{
							case StatementType.Sub: element.Type = CompilationElementType.Sub; break;
							case StatementType.Function: element.Type = CompilationElementType.Function; break;
						}

						element.AddLine(line);

						continue;
					}
					else if (lastStatement.Type == StatementType.EndScope)
					{
						element.AddLine(line);
						element = mainElement;

						continue;
					}
				}

				element.AddLines(comments);
				comments.Clear();

				element.AddLine(line);
			}
		}

		mainElement.AddLines(comments);

		return unit;
	}

	IEnumerable<CodeLine> ParseCodeLines(IEnumerable<Token> tokenStream)
	{
		var enumerator = tokenStream.GetEnumerator();

		var line = new CodeLine();
		var buffer = new List<Token>();
		bool haveContent = false;
		bool inType = false;
		int sourceLineNumber = 1;

		while (enumerator.MoveNext())
		{
			var token = enumerator.Current;

			if (token.Type == TokenType.NewLine)
			{
				line.Statements.Add(ParseStatement(buffer, colonAfter: false, ref inType));
				buffer.Clear();
				yield return line;
				line = new CodeLine();
				haveContent = false;
				sourceLineNumber++;
			}
			else if ((token.Type == TokenType.Whitespace) && !line.Statements.Any())
				line.Indentation += token.Value;
			else if ((token.Type == TokenType.Number) && !line.Statements.Any())
			{
				if (line.LineNumber != null)
					throw new SyntaxErrorException(token, "Expected: statement");

				line.Indentation = "";
				line.LineNumber = token.Value;
			}
			else
			{
				if (token.Type == TokenType.Colon)
				{
					if (!line.Statements.Any()
					 && (buffer.Count == 1)
					 && (buffer[0].Type == TokenType.Identifier)
					 && (buffer[0].Value is string labelName)
					 && (labelName.Length > 0)
					 && !char.IsSymbol(labelName.Last()))
					{
						line.Label =
							new Label()
							{
								Indentation = line.Indentation,
								Name = labelName,
							};

						line.Indentation = "";
					}
					else
					{
						line.Statements.Add(ParseStatement(buffer, colonAfter: true, ref inType));
						haveContent = true;
						buffer.Clear();
					}
				}
				else
					buffer.Add(token);
			}
		}

		if (buffer.Any() || haveContent)
			line.Statements.Add(ParseStatement(buffer, colonAfter: false, ref inType));

		if (!line.IsEmpty)
			yield return line;

		if (inType)
		{
			var errorLocation = new Token(sourceLineNumber + 1, 1, TokenType.Empty, "");

			throw new SyntaxErrorException(errorLocation, "Expected: END TYPE");
		}
	}

	internal Statement ParseStatement(ListRange<Token> tokens, bool colonAfter, ref bool inType)
	{
		if (!tokens.Where(token => token.Type != TokenType.Whitespace).Any())
			return new EmptyStatement();

		var tokenHandler = new TokenHandler(tokens);

		if (inType)
		{
			if (tokenHandler.NextTokenIs(TokenType.END))
			{
				tokenHandler.Expect(TokenType.END);
				tokenHandler.Expect(TokenType.TYPE);
				tokenHandler.ExpectEndOfTokens();

				return new EndTypeStatement();
			}

			var typeElement = new TypeElementStatement();

			typeElement.Name = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);

			tokenHandler.Expect(TokenType.AS);

			typeElement.ElementType = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);

			tokenHandler.ExpectEndOfTokens();

			return typeElement;
		}

		var token = tokenHandler.NextToken;

		tokenHandler.Advance();

		switch (token.Type)
		{
			case TokenType.Comment:
			{
				tokenHandler.ExpectEndOfTokens("Internal error: Additional tokens between Comment and Newline");

				string commentText = token.Value ?? "";

				var commentType = CommentStatementType.Apostrophe;

				if (commentText.StartsWith("'"))
					commentText = commentText.Substring(1);
				else if (commentText.StartsWith("REM "))
				{
					commentType = CommentStatementType.REM;
					commentText = commentText.Substring(4);
				}
				else if (commentText == "REM")
				{
					commentType = CommentStatementType.REM;
					commentText = commentText.Substring(3);
				}
				else
					throw new Exception("Internal error: Unrecognized comment style");

				return new CommentStatement(commentType, commentText);
			}

			case TokenType.CALL:
			{
				string targetName = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);

				ExpressionList? arguments = null;

				if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
				{
					var argumentTokens = tokenHandler.ExpectParenthesizedTokens();

					arguments = ParseExpressionList(argumentTokens, tokenHandler.PreviousToken);
				}

				tokenHandler.ExpectEndOfTokens();

				return new CallStatement(CallStatementType.Explicit, targetName, arguments);
			}

			case TokenType.CASE:
			{
				var expressions = ParseExpressionList(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				return new CaseStatement(expressions);
			}

			case TokenType.CLS:
			{
				if (tokenHandler.HasMoreTokens)
				{
					var mode = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

					return new ClsStatement(mode);
				}
				else
					return new ClsStatement();
			}

			case TokenType.COLOR:
			{
				if (!tokenHandler.HasMoreTokens)
					return new ColorStatement(); // this is a runtime error but should parse
				else
				{
					var arguments = ParseExpressionList(tokenHandler.RemainingTokens, tokenHandler.EndToken);

					if (arguments.Expressions.Count > 3)
						throw new SyntaxErrorException(tokenHandler.NextToken, "Expected no more than 3 arguments");

					return new ColorStatement(arguments);
				}
			}

			case TokenType.CONST:
			{
				var declarationSyntax = ParseExpressionList(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				var declarations = new List<ConstDeclaration>();

				for (int i = 0; i < declarationSyntax.Expressions.Count; i++)
				{
					var syntax = declarationSyntax.Expressions[i];

					if ((syntax is not BinaryExpression binarySyntax)
					 || (binarySyntax.Operator != Operator.Equals)
					 || (binarySyntax.Left is not IdentifierExpression identifierSyntax))
						throw new SyntaxErrorException(syntax.Token ?? tokenHandler.NextToken, "Expected: name = value");

					declarations.Add(new ConstDeclaration(identifierSyntax.Identifier, binarySyntax.Right));
				}

				return new ConstStatement(declarations);
			}

			case TokenType.DATA:
			{
				var dataItems = new List<Token>();

				for (int i = 1; i < tokens.Count; i++)
				{
					if ((tokens[i].Type == TokenType.Number) || (tokens[i].Type == TokenType.String))
					{
						dataItems.Add(tokens[i]);

						if ((i + 1 < tokens.Count) && (tokens[i + 1].Type == TokenType.Comma))
							i++;
					}
					else if (tokens[i].Type == TokenType.Comma)
						dataItems.Add(new Token(tokens[i].Line, tokens[i].Column, TokenType.Empty, ""));
					else
						throw new SyntaxErrorException(tokens[i], "Expected: string or numeric literal");
				}

				return new DataStatement(dataItems);
			}

			case TokenType.DECLARE:
			{
				var declarationType = tokenHandler.ExpectOneOf(TokenType.SUB, TokenType.FUNCTION);

				var name = tokenHandler.ExpectIdentifier(allowTypeCharacter: true);

				ParameterList? parameters = null;

				if (tokenHandler.HasMoreTokens)
				{
					var parameterTokens = tokenHandler.ExpectParenthesizedTokens();

					parameters = ParseParameterList(parameterTokens);
				}

				tokenHandler.ExpectEndOfTokens();

				return new DeclareStatement(declarationType, name, parameters);
			}

			case TokenType.DEF:
			{
				tokenHandler.ExpectMoreTokens();

				if (tokenHandler.NextToken.Type == TokenType.SEG)
				{
					tokenHandler.Advance();

					var defSeg = new DefSegStatement();

					if (tokenHandler.HasMoreTokens && (tokenHandler.NextToken.Type == TokenType.Equals))
					{
						tokenHandler.Advance();

						defSeg.SegmentExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);
					}

					return defSeg;
				}
				else if (tokenHandler.NextToken.Type == TokenType.Identifier)
				{
					var identifier = tokenHandler.ExpectIdentifier(allowTypeCharacter: true, out var identifierToken);

					if (!identifier.StartsWith("FN", StringComparison.OrdinalIgnoreCase))
						throw new SyntaxErrorException(identifierToken, "DEF function name must begin with FN");

					var defFn = new DefFnStatement();

					defFn.Name = identifier;

					if (tokenHandler.HasMoreTokens && (tokenHandler.NextToken.Type == TokenType.OpenParenthesis))
					{
						var parameterListTokens = tokenHandler.ExpectParenthesizedTokens();

						defFn.Parameters = ParseParameterList(parameterListTokens, allowByVal: false);
					}

					if (tokenHandler.HasMoreTokens && (tokenHandler.NextToken.Type == TokenType.Equals))
					{
						defFn.ExpressionBody = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);
						tokenHandler.AdvanceToEnd();
					}

					tokenHandler.ExpectEndOfTokens();

					return defFn;
				}
				else
					throw new SyntaxErrorException(tokenHandler.NextToken, "Expected DEF SEG or DEF FN");
			}

			case TokenType.DEFCUR:
			case TokenType.DEFDBL:
			case TokenType.DEFINT:
			case TokenType.DEFLNG:
			case TokenType.DEFSNG:
			case TokenType.DEFSTR:
			{
				var defType = new DefTypeStatement();

				switch (token.Type)
				{
					case TokenType.DEFCUR: defType.DataType = DataType.CURRENCY; break;
					case TokenType.DEFDBL: defType.DataType = DataType.DOUBLE; break;
					case TokenType.DEFINT: defType.DataType = DataType.INTEGER; break;
					case TokenType.DEFLNG: defType.DataType = DataType.LONG; break;
					case TokenType.DEFSNG: defType.DataType = DataType.SINGLE; break;
					case TokenType.DEFSTR: defType.DataType = DataType.STRING; break;
				}

				string? rangeStart = null;
				string? rangeEnd = null;

				rangeStart = tokenHandler.ExpectIdentifier(allowTypeCharacter: false, out var identifierToken);

				if (rangeStart.Length != 1)
					throw new SyntaxErrorException(identifierToken, "Expected: letter");

				defType.RangeStart = char.ToUpperInvariant(rangeStart[0]);

				if (tokenHandler.HasMoreTokens)
				{
					tokenHandler.Expect(TokenType.Equals);
					tokenHandler.ExpectMoreTokens();

					rangeEnd = tokenHandler.ExpectIdentifier(allowTypeCharacter: false, out identifierToken);

					if (rangeEnd.Length != 1)
						throw new SyntaxErrorException(identifierToken, "Expected: letter");

					defType.RangeEnd = char.ToUpperInvariant(rangeEnd[0]);
				}

				if (defType.RangeStart == defType.RangeEnd)
					defType.RangeEnd = null;
				else if (defType.RangeStart > defType.RangeEnd)
					(defType.RangeStart, defType.RangeEnd) = (defType.RangeEnd, defType.RangeStart);

				tokenHandler.ExpectEndOfTokens();

				return defType;
			}

			case TokenType.DIM:
			{
				var dim = new DimStatement();

				tokenHandler.ExpectMoreTokens();

				if (tokenHandler.NextToken.Type == TokenType.SHARED)
				{
					dim.Shared = true;
					tokenHandler.Advance();
				}

				var endTokenRef = new TokenRef();

				foreach (var range in SplitCommaDelimitedList(tokenHandler.RemainingTokens, endTokenRef))
					dim.Declarations.Add(ParseVariableDeclaration(range, endTokenRef.Token ?? tokenHandler.EndToken));

				return dim;
			}
			case TokenType.DO:
			case TokenType.LOOP:
			{
				var statement =
					tokenHandler.NextToken.Type switch
					{
						TokenType.DO => new DoStatement(),
						TokenType.LOOP => new LoopStatement(),

						_ => throw new Exception("Internal error")
					};

				if (tokenHandler.HasMoreTokens)
				{
					var conditionTypeToken = tokenHandler.ExpectOneOf(TokenType.WHILE, TokenType.UNTIL);

					switch (conditionTypeToken.Type)
					{
						case TokenType.WHILE: statement.ConditionType = DoConditionType.While; break;
						case TokenType.UNTIL: statement.ConditionType = DoConditionType.Until; break;
					}

					statement.Expression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);
				}

				return statement;
			}

			case TokenType.ELSE:
			{
				tokenHandler.ExpectEndOfTokens();

				return new ElseStatement();
			}

			case TokenType.END:
			{
				// One of:
				//   END DEF
				//   END FUNCTION
				//   END IF
				//   END SELECT
				//   END SUB
				//   END TYPE
				//   END [expression]

				if (!tokenHandler.HasMoreTokens)
					return new EndStatement();

				Statement endBlock;

				switch (tokenHandler.NextToken.Type)
				{
					case TokenType.DEF: endBlock = new EndDefStatement(); break;
					case TokenType.IF: endBlock = new EndIfStatement(); break;
					case TokenType.SELECT: endBlock = new EndSelectStatement(); break;

					case TokenType.SUB: endBlock = new EndScopeStatement() { ScopeType = ScopeType.Sub }; break;
					case TokenType.FUNCTION: endBlock = new EndScopeStatement() { ScopeType = ScopeType.Function }; break;

					case TokenType.TYPE: throw new Exception("END TYPE without TYPE");

					default:
					{
						// Skip the common tail for this case.
						return
							new EndStatement()
							{
								Expression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken)
							};
					}
				}

				tokenHandler.Advance();
				tokenHandler.ExpectEndOfTokens();

				return endBlock;
			}

			case TokenType.FOR:
			{
				var forStatement = new ForStatement();

				forStatement.CounterVariable = tokenHandler.ExpectIdentifier(allowTypeCharacter: true);

				var clauses = SplitDelimitedList(tokenHandler.RemainingTokens, TokenType.STEP).ToList();

				var rangeExpressions = SplitDelimitedList(clauses[0], TokenType.TO).ToList();

				if (rangeExpressions.Count < 2)
					throw new SyntaxErrorException(rangeExpressions[0].Last(), "Expected: TO");

				if (rangeExpressions.Count > 2)
				{
					var range = rangeExpressions[2].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: STEP or end of statement");
				}

				var midToken = tokens[rangeExpressions[1].Unwrap().Offset - 1];
				var endToken = clauses.Count == 1
					? tokenHandler.EndToken
					: tokens[clauses[1].Unwrap().Offset - 1];

				forStatement.StartExpression = ParseExpression(rangeExpressions[0], midToken);
				forStatement.EndExpression = ParseExpression(rangeExpressions[1], endToken);

				if (clauses.Count > 2)
					throw new SyntaxErrorException(clauses[2].First(), "Expected: end of statement");

				if (clauses.Count == 2)
					forStatement.StepExpression = ParseExpression(clauses[1], tokenHandler.EndToken);

				return forStatement;
			}

			case TokenType.GOTO:
			case TokenType.GOSUB:
			case TokenType.RESTORE:
			case TokenType.RETURN:
			{
				var statement =
					tokenHandler.NextToken.Type switch
					{
						TokenType.GOTO => new GoToStatement(),
						TokenType.GOSUB => new GoSubStatement(),
						TokenType.RESTORE => new RestoreStatement(),
						TokenType.RETURN => new ReturnStatement(),

						_ => default(TargetLineStatement) ?? throw new Exception("Internal error")
					};

				tokenHandler.Advance();
				tokenHandler.ExpectMoreTokens();

				switch (tokenHandler.NextToken.Type)
				{
					case TokenType.Number:
						statement.TargetLineNumber = tokenHandler.NextToken.Value;
						break;

					case TokenType.Identifier:
						string labelName = tokenHandler.NextToken.Value ?? throw new Exception("Internal error: Identifier token with no value");

						if (labelName.Length == 0)
							throw new Exception("Internal error: Identifier token with empty string");

						if (char.IsSymbol(labelName.Last()))
							throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: label");

						statement.TargetLabel = labelName;

						break;
				}

				tokenHandler.Advance();
				tokenHandler.ExpectEndOfTokens();

				return statement;
			}

			case TokenType.IF:
			case TokenType.ELSEIF:
			{
				var statement =
					tokenHandler.NextToken.Type switch
					{
						TokenType.IF => new IfStatement(),
						TokenType.ELSEIF => new ElseIfStatement(),

						_ => throw new Exception("Internal error")
					};

				if (tokens.Last().Type != TokenType.THEN)
					throw new SyntaxErrorException(tokens.Last(), "Expected: THEN");

				statement.ConditionExpression = ParseExpression(tokens.Slice(1, tokens.Count - 2), tokens.Last());

				return statement;
			}

			case TokenType.INPUT:
			{
				// One of:
				//   INPUT [;] [prompt {;|,}] variable[, variable[..]]
				//   INPUT #filenumber, variable[, [variable[..]]

				var input = new InputStatement();

				if (tokenHandler.NextTokenIs(TokenType.NumberSign))
				{
					tokenHandler.Advance();

					var fileNumberToken = tokenHandler.RemainingTokens.Slice(0, 1);

					tokenHandler.ExpectOneOf(TokenType.Number, TokenType.Identifier);

					input.FileNumberExpression = ParseExpression(fileNumberToken, tokenHandler.RemainingTokens.Skip(1).FirstOrDefault() ?? tokenHandler.EndToken);

					tokenHandler.Expect(TokenType.Comma);
				}
				else
				{
					if (tokenHandler.NextTokenIs(TokenType.Semicolon))
					{
						input.EchoNewLine = false;
						tokenHandler.Advance();
					}

					if (tokenHandler.NextTokenIs(TokenType.String))
					{
						input.PromptString = tokenHandler.NextToken.Value;

						tokenHandler.Advance();

						var questionMarkToken = tokenHandler.ExpectOneOf(TokenType.Semicolon, TokenType.Comma);

						input.PromptQuestionMark = (questionMarkToken.Type == TokenType.Semicolon);
					}
				}

				var variables = SplitCommaDelimitedList(tokenHandler.RemainingTokens);

				foreach (var variable in variables)
				{
					if (variable.Count == 0)
					{
						var range = variable.Unwrap();

						if (range.Offset >= tokens.Count)
							range.Offset--;

						throw new SyntaxErrorException(tokens[range.Offset], "Expected: identifier");
					}

					if (variable[0].Type != TokenType.Identifier)
						throw new SyntaxErrorException(variable[0], "Expected: identifier");

					if (variable.Count > 1)
						throw new SyntaxErrorException(variable[1], "Expected: comma or end of statement");

					input.Variables.Add(variable[0].Value ?? throw new Exception("Internal error: Identifier with no value"));
				}

				return input;
			}

			case TokenType.LINE:
			{
				// One of:
				//  LINE [ [STEP] (x1, y1) ] - [STEP] (x2, y2) [, [color] [, [B[F]] [, style]]]
				//  LINE INPUT [;] ["promptstring";] stringvariable
				//  LINE INPUT #filenumber, stringvariable

				if (tokenHandler.NextTokenIs(TokenType.INPUT))
				{
					var lineInput = new LineInputStatement();

					tokenHandler.Advance();

					if (tokenHandler.NextTokenIs(TokenType.NumberSign))
					{
						tokenHandler.Advance();

						var fileNumberToken = tokenHandler.RemainingTokens.Slice(0, 1);

						tokenHandler.ExpectOneOf(TokenType.Number, TokenType.Identifier);

						lineInput.FileNumberExpression = ParseExpression(fileNumberToken, tokenHandler.RemainingTokens.Skip(1).FirstOrDefault() ?? tokenHandler.EndToken);

						tokenHandler.Expect(TokenType.Comma);
					}

					if (tokenHandler.NextTokenIs(TokenType.Semicolon))
					{
						lineInput.EchoNewLine = false;
						tokenHandler.Advance();
					}
					else
					{
						if (tokenHandler.NextTokenIs(TokenType.String))
						{
							lineInput.PromptString = tokenHandler.NextToken.Value;

							tokenHandler.Advance();
							tokenHandler.Expect(TokenType.Semicolon);
						}
					}

					lineInput.Variable = tokenHandler.ExpectIdentifier(allowTypeCharacter: true);

					tokenHandler.ExpectEndOfTokens();

					return lineInput;
				}
				else
				{
					//  LINE [ [STEP] (x1, y1) ] - [STEP] (x2, y2) [, [color] [, [B[F]] [, style]]]

					var lineStatement = new LineStatement();

					if (tokenHandler.NextTokenIs(TokenType.STEP))
					{
						lineStatement.FromStep = true;
						tokenHandler.Advance();

						if (!tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
							throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: (");
					}

					if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
					{
						var fromTokens = tokenHandler.ExpectParenthesizedTokens();

						if (fromTokens.Count == 0)
						{
							var range = fromTokens.Unwrap();

							throw new SyntaxErrorException(tokens[range.Offset], "Expected: expression");
						}

						var fromExpressions = SplitCommaDelimitedList(fromTokens).ToList();

						if (fromExpressions.Count == 1)
						{
							var range = fromTokens.Unwrap();

							throw new SyntaxErrorException(tokens[range.Offset + range.Count], "Expected: ,");
						}

						if (fromExpressions.Count > 2)
						{
							var range = fromExpressions[1].Unwrap();

							throw new SyntaxErrorException(tokens[range.Offset + range.Count], "Expected: )");
						}

						var midToken = tokens[fromExpressions[1].Unwrap().Offset - 1];
						var endToken = tokenHandler.PreviousToken;

						lineStatement.FromXExpression = ParseExpression(fromExpressions[0], midToken);
						lineStatement.FromYExpression = ParseExpression(fromExpressions[1], endToken);
					}

					tokenHandler.Expect(TokenType.Hyphen);

					if (tokenHandler.NextTokenIs(TokenType.STEP))
					{
						lineStatement.ToStep = true;
						tokenHandler.Advance();
					}

					var toTokens = tokenHandler.ExpectParenthesizedTokens();

					if (toTokens.Count == 0)
					{
						var range = toTokens.Unwrap();

						throw new SyntaxErrorException(tokens[range.Offset], "Expected: expression");
					}

					var toExpressions = SplitCommaDelimitedList(toTokens).ToList();

					if (toExpressions.Count == 1)
					{
						var range = toTokens.Unwrap();

						throw new SyntaxErrorException(tokens[range.Offset + range.Count], "Expected: ,");
					}

					if (toExpressions.Count > 2)
					{
						var range = toExpressions[1].Unwrap();

						throw new SyntaxErrorException(tokens[range.Offset + range.Count], "Expected: )");
					}

					{
						var midToken = tokens[toExpressions[1].Unwrap().Offset - 1];
						var endToken = tokenHandler.PreviousToken;

						lineStatement.ToXExpression = ParseExpression(toExpressions[0], midToken);
						lineStatement.ToYExpression = ParseExpression(toExpressions[1], endToken);
					}

					// [, [color] [, [B[F]] [, style]]]
					if (tokenHandler.NextTokenIs(TokenType.Comma))
					{
						tokenHandler.Advance();
						tokenHandler.ExpectMoreTokens();

						var options = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

						var colourEndToken = options.Count > 1
							? tokens[options[1].Unwrap().Offset - 1]
							: tokenHandler.EndToken;

						lineStatement.ColourExpression = ParseExpression(options[0], colourEndToken);

						if (options.Count > 1)
						{
							var drawStyle = options[1];

							if (drawStyle.Any())
							{
								if ((drawStyle.Count > 1)
								 || (drawStyle[0].Type != TokenType.Identifier))
									throw new SyntaxErrorException(drawStyle[0], "Expected: B or BF");

								string drawStyleString = drawStyle[0].Value ?? throw new Exception("Internal error: Identifier token with no value");

								bool isBox = drawStyleString.Equals("B", StringComparison.OrdinalIgnoreCase);
								bool isFilledBox = drawStyleString.Equals("BF", StringComparison.OrdinalIgnoreCase);

								if (isBox)
									lineStatement.DrawStyle = LineDrawStyle.Box;
								else if (isFilledBox)
									lineStatement.DrawStyle = LineDrawStyle.FilledBox;
								else
									throw new SyntaxErrorException(drawStyle[0], "Expected: B or BF");
							}

							if (options.Count > 2)
							{
								var styleEndToken = options.Count > 2
									? tokens[options[2].Unwrap().Offset - 1]
									: tokenHandler.EndToken;

								lineStatement.StyleExpression = ParseExpression(options[2], styleEndToken);

								if (options.Count > 3)
								{
									var range = options[3].Unwrap();

									throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
								}
							}
						}
					}

					tokenHandler.ExpectEndOfTokens();

					return lineStatement;
				}
			}

			case TokenType.LOCATE:
			{
				var locate = new LocateStatement();

				var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

				var rowEndToken = arguments.Count > 1
					? tokens[arguments[1].Unwrap().Offset - 1]
					: tokenHandler.EndToken;

				locate.RowExpression = ParseExpression(arguments[0], rowEndToken);

				if ((arguments.Count > 1) && arguments[1].Any())
				{
					var columnEndToken = arguments.Count > 2
						? tokens[arguments[2].Unwrap().Offset - 1]
						: tokenHandler.EndToken;

					locate.ColumnExpression = ParseExpression(arguments[1], columnEndToken);
				}

				if ((arguments.Count > 2) && arguments[2].Any())
				{
					var cursorVisibilityEndToken = arguments.Count > 3
						? tokens[arguments[3].Unwrap().Offset - 1]
						: tokenHandler.EndToken;

					locate.CursorVisibilityExpression = ParseExpression(arguments[2], cursorVisibilityEndToken);
				}

				if (arguments.Count > 3)
				{
					var endToken = arguments.Count > 4
						? tokens[arguments[4].Unwrap().Offset - 1]
						: tokenHandler.EndToken;

					locate.CursorStartExpression = ParseExpression(arguments[3], endToken);
				}

				if (arguments.Count > 4)
				{
					var endToken = arguments.Count > 5
						? tokens[arguments[5].Unwrap().Offset - 1]
						: tokenHandler.EndToken;

					locate.CursorEndExpression = ParseExpression(arguments[4], endToken);
				}

				if (arguments.Count > 5)
				{
					var range = arguments[5].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
				}

				return locate;
			}

			case TokenType.LPRINT:
			{
				var lprint = new LPrintStatement();

				while (tokenHandler.HasMoreTokens)
				{
					var nextSeparatorIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.Semicolon, TokenType.Comma);

					if (nextSeparatorIndex >= 0)
					{
						var arg = new PrintArgument();

						if (nextSeparatorIndex > 0)
							arg.Expression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, nextSeparatorIndex), tokenHandler[nextSeparatorIndex]);

						tokenHandler.Advance(nextSeparatorIndex);

						switch (tokenHandler.NextToken.Type)
						{
							case TokenType.Semicolon: arg.CursorAction = PrintCursorAction.None; break;
							case TokenType.Comma: arg.CursorAction = PrintCursorAction.NextZone; break;
						}

						lprint.Arguments.Add(arg);

						tokenHandler.Advance();
					}
					else
					{
						var arg = new PrintArgument();

						arg.Expression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);
						arg.CursorAction = PrintCursorAction.NextLine;

						lprint.Arguments.Add(arg);

						tokenHandler.AdvanceToEnd();
					}
				}

				return lprint;
			}

			case TokenType.NEXT:
			{
				var next = new NextStatement();

				if (tokenHandler.HasMoreTokens)
				{
					var counters = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

					for (int i = 0; i < counters.Count; i++)
					{
						var endToken = (i + 1 < counters.Count)
							? tokens[counters[i + 1].Unwrap().Offset - 1]
							: tokenHandler.EndToken;

						var counter = ParseExpression(counters[i], endToken);

						if (counter is not IdentifierExpression)
							throw new SyntaxErrorException(counters[i][0], "Expected: identifier");

						next.CounterExpressions.Add(counter);
					}
				}

				return next;
			}

			case TokenType.ON:
			{
				var on = new OnStatement();

				switch (tokenHandler.NextToken.Type)
				{
					case TokenType.COM: on.EventType = EventType.Com; break;
					case TokenType.KEY: on.EventType = EventType.Key; break;
					case TokenType.PEN: on.EventType = EventType.Pen; break;
					case TokenType.PLAY: on.EventType = EventType.Play; break;
					case TokenType.SIGNAL: on.EventType = EventType.OS2Signal; break;
					case TokenType.STRIG: on.EventType = EventType.JoystickTrigger; break;
					case TokenType.TIMER: on.EventType = EventType.Timer; break;
					case TokenType.UEVENT: on.EventType = EventType.UserDefinedEvent; break;

					default: throw new SyntaxErrorException(tokenHandler.NextToken, "Syntax error");
				}

				tokenHandler.Advance();

				var action = ParseStatement(tokenHandler.RemainingTokens, colonAfter, ref inType);

				if (action is GoSubStatement goSubAction)
					on.Action = goSubAction;
				else
					throw new SyntaxErrorException(tokens[2], "Expected: GOSUB");

				return on;
			}

			case TokenType.PLAY:
			{
				// One of:
				//   PLAY stringexpression
				//   PLAY {ON|OFF|STOP}

				if (tokenHandler.NextTokenIs(TokenType.ON)
				 || tokenHandler.NextTokenIs(TokenType.OFF)
				 || tokenHandler.NextTokenIs(TokenType.STOP))
				{
					var eventControl = new EventControlStatement();

					eventControl.EventType = EventType.Play;
					eventControl.Action =
						tokenHandler.NextToken.Type switch
						{
							TokenType.ON => EventControlAction.Enable,
							TokenType.OFF => EventControlAction.Disable,
							TokenType.STOP => EventControlAction.Suspend,

							_ => throw new Exception("Internal error")
						};

					return eventControl;
				}
				else
				{
					var play = new PlayStatement();

					play.CommandExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

					return play;
				}
			}

			case TokenType.POKE:
			{
				var poke = new PokeStatement();

				tokenHandler.ExpectMoreTokens();

				var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

				if (arguments.Count < 2)
					throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: address, byte");

				if (arguments.Count > 2)
				{
					var range = arguments[2].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
				}

				var addressEndToken = tokens[arguments[1].Unwrap().Offset - 1];
				var valueEndToken = tokenHandler.PreviousToken;

				poke.AddressExpression = ParseExpression(arguments[0], addressEndToken);
				poke.ValueExpression = ParseExpression(arguments[1], valueEndToken);

				return poke;
			}

			case TokenType.PRINT:
			{
				var print = new PrintStatement();

				if (tokenHandler.NextTokenIs(TokenType.NumberSign))
				{
					tokenHandler.Advance();

					var separatorIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.Comma);

					if (separatorIndex < 0)
						throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: ,");

					print.FileNumberExpression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, separatorIndex), tokenHandler[separatorIndex]);

					tokenHandler.Advance(separatorIndex + 1);
				}

				if (tokenHandler.NextTokenIs(TokenType.USING))
				{
					tokenHandler.Advance();

					var separatorIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.Semicolon);

					if (separatorIndex < 0)
					{
						var commaIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.Comma);

						if (commaIndex >= 0)
						{
							while (commaIndex > 0)
								tokenHandler.Advance();

							throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: ,");
						}
						else
							throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: ,");
					}

					print.UsingExpression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, separatorIndex), tokenHandler[separatorIndex]);

					tokenHandler.Advance(separatorIndex + 1);
				}

				while (tokenHandler.HasMoreTokens)
				{
					var nextSeparatorIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.Semicolon, TokenType.Comma);

					if (nextSeparatorIndex >= 0)
					{
						var arg = new PrintArgument();

						if (nextSeparatorIndex > 0)
							arg.Expression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, nextSeparatorIndex), tokenHandler[nextSeparatorIndex]);

						tokenHandler.Advance(nextSeparatorIndex);

						switch (tokenHandler.NextToken.Type)
						{
							case TokenType.Semicolon: arg.CursorAction = PrintCursorAction.None; break;
							case TokenType.Comma: arg.CursorAction = PrintCursorAction.NextZone; break;
						}

						print.Arguments.Add(arg);

						tokenHandler.Advance();
					}
					else
					{
						var arg = new PrintArgument();

						arg.Expression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);
						arg.CursorAction = PrintCursorAction.NextLine;

						print.Arguments.Add(arg);

						tokenHandler.AdvanceToEnd();
					}
				}

				return print;
			}

			case TokenType.PSET:
			case TokenType.PRESET:
			{
				var pixel = new PixelSetStatement();

				pixel.DefaultColour =
					token.Type switch
					{
						TokenType.PSET => PixelSetDefaultColour.Foreground,
						TokenType.PRESET => PixelSetDefaultColour.Background,

						_ => throw new Exception("Internal error")
					};

				if (tokenHandler.NextTokenIs(TokenType.STEP))
				{
					pixel.StepCoordinates = true;
					tokenHandler.Advance();
				}

				var coordinateTokens = tokenHandler.ExpectParenthesizedTokens();

				var coordinates = SplitCommaDelimitedList(coordinateTokens).ToList();

				if (coordinates.Count < 2)
					throw new SyntaxErrorException(tokenHandler.PreviousToken, "Expected: expression");
				else if (coordinates.Count > 2)
				{
					var range = coordinates[2].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: )");
				}

				var xEndToken = tokens[coordinates[1].Unwrap().Offset - 1];
				var yEndToken = tokenHandler.PreviousToken;

				pixel.XExpression = ParseExpression(coordinates[0], xEndToken);
				pixel.YExpression = ParseExpression(coordinates[1], yEndToken);

				if (tokenHandler.HasMoreTokens)
				{
					tokenHandler.Expect(TokenType.Comma);

					pixel.ColourExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

					tokenHandler.AdvanceToEnd();
				}

				tokenHandler.ExpectEndOfTokens();

				return pixel;
			}

			case TokenType.RANDOMIZE:
			{
				var randomize = new RandomizeStatement();

				if (tokenHandler.HasMoreTokens)
					randomize.Expression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				return randomize;
			}

			case TokenType.READ:
			{
				var read = new ReadStatement();

				tokenHandler.ExpectMoreTokens();

				var variables = SplitCommaDelimitedList(tokenHandler.RemainingTokens);

				foreach (var variable in variables)
				{
					if (variable.Count == 0)
					{
						var range = variable.Unwrap();

						if (range.Offset >= tokens.Count)
							range.Offset--;

						throw new SyntaxErrorException(tokens[range.Offset], "Expected: identifier");
					}

					if (variable[0].Type != TokenType.Identifier)
						throw new SyntaxErrorException(variable[0], "Expected: identifier");

					if (variable.Count > 1)
						throw new SyntaxErrorException(variable[1], "Expected: comma or end of statement");

					read.Variables.Add(variable[0].Value ?? throw new Exception("Internal error: Identifier with no value"));
				}

				return read;
			}

			case TokenType.SCREEN:
			{
				var screen = new ScreenStatement();

				tokenHandler.ExpectMoreTokens();

				var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

				var modeEndToken = arguments.Count > 1
					? tokens[arguments[1].Unwrap().Offset - 1]
					: tokenHandler.EndToken;

				screen.ModeExpression = ParseExpression(arguments[0], modeEndToken);

				if (arguments.Count > 1)
				{
					if (arguments[1].Any())
					{
						var endToken = arguments.Count > 2
							? tokens[arguments[2].Unwrap().Offset - 1]
							: tokenHandler.EndToken;

						screen.ColourSwitchExpression = ParseExpression(arguments[1], endToken);
					}

					if (arguments.Count > 2)
					{
						if (arguments[2].Any())
						{
							var endToken = arguments.Count > 3
								? tokens[arguments[3].Unwrap().Offset - 1]
								: tokenHandler.EndToken;

							screen.ActivePageExpression = ParseExpression(arguments[2], endToken);
						}

						if (arguments.Count > 3)
						{
							if (!arguments[3].Any())
							{
								if (arguments.Count > 4)
								{
									var range = arguments[4].Unwrap();

									throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: expression");
								}
								else
									throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: expression");
							}

							var endToken = arguments.Count > 4
								? tokens[arguments[4].Unwrap().Offset - 1]
								: tokenHandler.EndToken;

							screen.VisiblePageExpression = ParseExpression(arguments[3], endToken);

							if (arguments.Count > 4)
							{
								var range = arguments[4].Unwrap();

								throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
							}
						}
					}
				}

				return screen;
			}

			case TokenType.SELECT:
			{
				var selectCase = new SelectCaseStatement();

				tokenHandler.Expect(TokenType.CASE);

				selectCase.Expression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				return selectCase;
			}

			case TokenType.SHARED:
			case TokenType.STATIC:
			{
				var scopeStatement = new VariableScopeStatement();

				switch (tokenHandler.NextToken.Type)
				{
					case TokenType.SHARED: scopeStatement.ScopeType = VariableScopeType.Shared; break;
					case TokenType.STATIC: scopeStatement.ScopeType = VariableScopeType.Static; break;
				}

				tokenHandler.ExpectMoreTokens();

				foreach (var declarationTokens in SplitCommaDelimitedList(tokenHandler.RemainingTokens))
				{
					if (declarationTokens.Count == 0)
					{
						var range = declarationTokens.Unwrap();

						throw new SyntaxErrorException(tokens[range.Offset], "Expected: identifier");
					}

					var declarationHandler = new TokenHandler(declarationTokens);

					var declaration = new VariableScopeDeclaration();

					declaration.Name = declarationHandler.ExpectIdentifier(allowTypeCharacter: true);

					if (declarationHandler.NextTokenIs(TokenType.OpenParenthesis))
					{
						declarationHandler.Expect(TokenType.OpenParenthesis);
						declarationHandler.Expect(TokenType.CloseParenthesis);

						declaration.IsArray = true;
					}

					if (declarationHandler.NextTokenIs(TokenType.AS))
					{
						if (char.IsSymbol(declaration.Name.Last()))
							throw new SyntaxErrorException(declarationTokens[0], "Identifier cannot end with %, &, !, #, $ or @");

						declarationHandler.Advance();
						declaration.Type = declarationHandler.ExpectIdentifier(allowTypeCharacter: false);
					}

					declarationHandler.ExpectEndOfTokens();

					scopeStatement.Declarations.Add(declaration);
				}

				return scopeStatement;
			}

			case TokenType.SUB:
			case TokenType.FUNCTION:
			{
				var statement =
					tokenHandler.NextToken.Type switch
					{
						TokenType.SUB => new SubStatement(),
						TokenType.FUNCTION => new FunctionStatement(),

						_ => default(SubroutineOpeningStatement) ?? throw new Exception("Internal error")
					};

				statement.Name = tokenHandler.ExpectIdentifier(allowTypeCharacter: statement is FunctionStatement);

				if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
				{
					var parameterListTokens = tokenHandler.ExpectParenthesizedTokens();

					statement.Parameters = ParseParameterList(parameterListTokens, allowByVal: true);
				}

				if (tokenHandler.HasMoreTokens)
				{
					tokenHandler.Expect(TokenType.STATIC);
					statement.IsStatic = true;
				}

				tokenHandler.ExpectEndOfTokens();

				return statement;
			}

			case TokenType.COM:
			case TokenType.KEY:
			case TokenType.PEN:
			case TokenType.SIGNAL:
			case TokenType.STRIG:
			case TokenType.TIMER:
			case TokenType.UEVENT:
			{
				var eventControl = new EventControlStatement();

				bool needSourceExpression = true;

				switch (token.Type)
				{
					case TokenType.COM: eventControl.EventType = EventType.Com; break;
					case TokenType.KEY: eventControl.EventType = EventType.Key; break;
					case TokenType.PEN: eventControl.EventType = EventType.Pen; needSourceExpression = false; break;
					case TokenType.PLAY: eventControl.EventType = EventType.Play; needSourceExpression = false; break;
					case TokenType.SIGNAL: eventControl.EventType = EventType.OS2Signal; break;
					case TokenType.STRIG: eventControl.EventType = EventType.JoystickTrigger; break;
					case TokenType.TIMER: eventControl.EventType = EventType.Timer; break;
					case TokenType.UEVENT: eventControl.EventType = EventType.UserDefinedEvent; needSourceExpression = false; break;
				}

				if (needSourceExpression)
				{
					var sourceExpressionTokens = tokenHandler.ExpectParenthesizedTokens();

					eventControl.SourceExpression = ParseExpression(sourceExpressionTokens, tokenHandler.PreviousToken);
				}

				var modeToken = tokenHandler.ExpectOneOf(TokenType.ON, TokenType.OFF, TokenType.STOP);

				eventControl.Action =
					modeToken.Type switch
					{
						TokenType.ON => EventControlAction.Enable,
						TokenType.OFF => EventControlAction.Disable,
						TokenType.STOP => EventControlAction.Suspend,

						_ => throw new Exception("Internal error")
					};

				return eventControl;
			}

			case TokenType.TYPE:
			{
				var type = new TypeStatement();

				type.Name = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);

				tokenHandler.ExpectEndOfTokens();

				inType = true; // switch modes for subsequent statements

				return type;
			}

			case TokenType.VIEW:
			{
				var view = new ViewStatement();

				if (tokenHandler.NextTokenIs(TokenType.SCREEN))
				{
					view.AbsoluteCoordinates = true;
					tokenHandler.Advance();
				}

				var fromCoordinates = SplitCommaDelimitedList(tokenHandler.ExpectParenthesizedTokens()).ToList();

				if (fromCoordinates.Count < 2)
					throw new SyntaxErrorException(tokenHandler.PreviousToken, "Expected: expression");
				else if (fromCoordinates.Count > 2)
				{
					var range = fromCoordinates[2].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: )");
				}

				var fromXEndToken = tokens[fromCoordinates[1].Unwrap().Offset - 1];
				var fromYEndToken = tokenHandler.PreviousToken;

				view.FromXExpression = ParseExpression(fromCoordinates[0], fromXEndToken);
				view.FromYExpression = ParseExpression(fromCoordinates[1], fromYEndToken);

				tokenHandler.Expect(TokenType.Hyphen);

				var toCoordinates = SplitCommaDelimitedList(tokenHandler.ExpectParenthesizedTokens()).ToList();

				if (toCoordinates.Count < 2)
					throw new SyntaxErrorException(tokenHandler.PreviousToken, "Expected: expression");
				else if (toCoordinates.Count > 2)
				{
					var range = toCoordinates[2].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: )");
				}

				var toXEndToken = tokens[toCoordinates[1].Unwrap().Offset - 1];
				var toYEndToken = tokenHandler.PreviousToken;

				view.ToXExpression = ParseExpression(toCoordinates[0], toXEndToken);
				view.ToYExpression = ParseExpression(toCoordinates[1], toYEndToken);

				if (tokenHandler.NextTokenIs(TokenType.Comma))
				{
					tokenHandler.Advance();

					int separator = tokenHandler.FindNextUnparenthesizedOf(TokenType.Comma);

					if (separator > 0)
					{
						var expressionTokens = tokenHandler.RemainingTokens;
						var endToken = tokenHandler.EndToken;

						if (separator >= 0)
						{
							expressionTokens = expressionTokens.Slice(0, separator);
							endToken = tokenHandler[separator];
						}

						view.FillColourExpression = ParseExpression(expressionTokens, endToken);

						tokenHandler.Advance(separator);
					}

					if (tokenHandler.NextTokenIs(TokenType.Comma))
					{
						tokenHandler.Advance();

						view.BorderColourExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);
					}
				}

				return view;
			}

			case TokenType.WEND:
			{
				tokenHandler.ExpectEndOfTokens();
				return new WEndStatement();
			}

			case TokenType.WHILE:
			{
				var whileStatement = new WhileStatement();

				whileStatement.Condition = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				return whileStatement;
			}

			case TokenType.WIDTH:
			{
				// One of:
				//   WIDTH #filenumber, width
				//   WIDTH device$, width
				//   WIDTH LPRINT width
				//   WIDTH screenwidth
				//   WIDTH [screenwidth], screenheight

				if (tokenHandler.NextTokenIs(TokenType.NumberSign))
				{
					var width = new FileWidthStatement();

					tokenHandler.Advance();

					var fileWidthArguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

					if (fileWidthArguments.Count < 2)
						throw new SyntaxErrorException(tokens.Last(), "Expected: ,");

					if (fileWidthArguments.Count > 2)
					{
						var range = fileWidthArguments[2].Unwrap();

						throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
					}

					var fileWidthMidToken = tokens[fileWidthArguments[1].Unwrap().Offset - 1];

					width.FileNumberExpression = ParseExpression(fileWidthArguments[0], fileWidthMidToken);
					width.WidthExpression = ParseExpression(fileWidthArguments[1], tokenHandler.EndToken);

					return width;
				}

				if (tokenHandler.NextTokenIs(TokenType.LPRINT))
				{
					var width = new LPrintWidthStatement();

					tokenHandler.Advance();

					width.WidthExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

					return width;
				}

				var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

				if (arguments.Count == 1)
				{
					var width = new ScreenWidthStatement();

					width.WidthExpression = ParseExpression(arguments[0], tokenHandler.EndToken);

					return width;
				}

				if (arguments.Count > 2)
				{
					var range = arguments[2].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
				}

				var unresolvedWidth = new UnresolvedWidthStatement();

				var midToken = tokens[arguments[1].Unwrap().Offset - 1];

				unresolvedWidth.Expression1 = ParseExpression(arguments[0], midToken);
				unresolvedWidth.Expression2 = ParseExpression(arguments[1], tokenHandler.EndToken);

				return unresolvedWidth;
			}
		}

		// If not one of the above, then one of:
		//   subname argumentlist
		//   variablename = expression

		if (tokenHandler.NextTokenIs(TokenType.Equals))
		{
			var assignment = new AssignmentStatement();

			tokenHandler.Reset();

			assignment.Variable = tokenHandler.ExpectIdentifier(allowTypeCharacter: true);

			tokenHandler.Expect(TokenType.Equals);

			assignment.Expression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

			return assignment;
		}
		else
		{
			tokenHandler.Reset();

			var targetName = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);

			ExpressionList? arguments = null;

			if (tokenHandler.HasMoreTokens)
				arguments = ParseExpressionList(tokenHandler.RemainingTokens, tokenHandler.EndToken);

			return new CallStatement(CallStatementType.Implicit, targetName, arguments);
		}
	}

	class TokenRef
	{
		public Token? Token;
	}

	IEnumerable<ListRange<Token>> SplitCommaDelimitedList(ListRange<Token> tokens)
		=> SplitCommaDelimitedList(tokens, new TokenRef());

	IEnumerable<ListRange<Token>> SplitCommaDelimitedList(ListRange<Token> tokens, TokenRef endTokenRef)
		=> SplitDelimitedList(tokens, TokenType.Comma, endTokenRef);

	IEnumerable<ListRange<Token>> SplitDelimitedList(ListRange<Token> tokens, TokenType delimiterType)
		=> SplitDelimitedList(tokens, delimiterType, new TokenRef());

	IEnumerable<ListRange<Token>> SplitDelimitedList(ListRange<Token> tokens, TokenType delimiterType, TokenRef endTokenRef)
	{
		int nesting = 0;
		int itemStart = 0;

		for (int i = 0; i < tokens.Count; i++)
		{
			if (tokens[i].Type == delimiterType)
			{
				if (nesting == 0)
				{
					endTokenRef.Token = tokens[i];
					yield return tokens.Slice(itemStart, i - itemStart);
					itemStart = i + 1;
				}
			}
			else
			{
				switch (tokens[i].Type)
				{
					case TokenType.OpenParenthesis: nesting++; break;
					case TokenType.CloseParenthesis: nesting--; break;
				}
			}
		}

		endTokenRef.Token = null;
		yield return tokens.Slice(itemStart);
	}

	VariableDeclaration ParseVariableDeclaration(ListRange<Token> tokens, Token endToken)
	{
		var tokenHandler = new TokenHandler(tokens);

		var declaration = new VariableDeclaration();

		tokenHandler.ExpectMoreTokens("Expected variable declaration");

		declaration.Name = tokenHandler.ExpectIdentifier(allowTypeCharacter: true);

		if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
		{
			var subscriptTokens = tokenHandler.ExpectParenthesizedTokens();

			declaration.Subscripts = new VariableDeclarationSubscriptList();

			var endTokenRef = new TokenRef();

			foreach (var subscript in SplitCommaDelimitedList(subscriptTokens, endTokenRef))
				declaration.Subscripts.Add(ParseVariableDeclarationSubscript(subscript, endTokenRef.Token ?? endToken));
		}

		return declaration;
	}

	private VariableDeclarationSubscript ParseVariableDeclarationSubscript(ListRange<Token> subscriptTokens, Token endToken)
	{
		var boundExpressions = SplitDelimitedList(subscriptTokens, TokenType.TO).ToList();

		if (boundExpressions.Count > 2)
		{
			var range = boundExpressions[2].Unwrap();

			throw new SyntaxErrorException(range.List[range.Offset - 1], "Expected: )");
		}

		var subscript = new VariableDeclarationSubscript();

		var bound2Range = boundExpressions[1].Unwrap();

		var midToken = bound2Range.List[bound2Range.Offset - 1];

		subscript.Bound1 = ParseExpression(boundExpressions[0], midToken);
		subscript.Bound2 = ParseExpression(boundExpressions[1], endToken);

		return subscript;
	}

	ParameterList ParseParameterList(ListRange<Token> tokens, bool allowByVal = true)
	{
		var list = new ParameterList();

		foreach (var range in SplitCommaDelimitedList(tokens))
			list.Parameters.Add(ParseParameterDefinition(range, allowByVal));

		return list;
	}

	ParameterDefinition ParseParameterDefinition(ListRange<Token> tokens, bool allowByVal)
	{
		var param = new ParameterDefinition();

		try
		{
			int tokenIndex = 0;

			if (tokens[tokenIndex].Type == TokenType.BYVAL)
			{
				if (!allowByVal)
					throw new SyntaxErrorException(tokens[tokenIndex], "BYVAL is not permitted in this context");

				param.IsByVal = true;
				tokenIndex++;
			}

			if (tokens[tokenIndex].Type != TokenType.Identifier)
				throw new SyntaxErrorException(tokens[tokenIndex], "Expected identifier");

			param.Name = tokens[tokenIndex].Value ?? throw new Exception("Internal error: identifier token with no value");

			tokenIndex++;

			char lastChar = param.Name.Last();

			if (TypeCharacter.TryParse(lastChar, out var typeCharacter))
				param.ActualName = param.Name;
			else if (tokenIndex < tokens.Count)
			{
				if (tokens[tokenIndex].Type != TokenType.AS)
					throw new SyntaxErrorException(tokens[tokenIndex], "Expected AS");

				tokenIndex++;

				if (!tokens[tokenIndex].IsDataType)
					throw new SyntaxErrorException(tokens[tokenIndex], "Expected data type");

				param.Type = DataTypeConverter.FromToken(tokens[tokenIndex]);
				param.ActualName = param.Name + new TypeCharacter(param.Type.Value).Character;

				tokenIndex++;
			}

			if (tokenIndex < tokens.Count)
				throw new SyntaxErrorException(tokens[tokenIndex], "Expected end of parameter definition");
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new SyntaxErrorException(tokens.Last(), "Unexpected end of parameter declaration");
		}

		return param;
	}

	ExpressionList ParseExpressionList(ListRange<Token> tokens, Token endToken)
	{
		var list = new ExpressionList();

		var endTokenRef = new TokenRef();

		foreach (var range in SplitCommaDelimitedList(tokens, endTokenRef))
			list.Expressions.Add(ParseExpression(range, endTokenRef.Token ?? endToken));

		return list;
	}

	internal Expression ParseExpression(ListRange<Token> tokens, Token endToken)
	{
		int lastOperatorIndex = -1;
		int lastOperatorPrecedence = -1;
		int level = 0;
		bool entirelyParenthesized = tokens.Count > 2;

		for (int i = 0; i < tokens.Count; i++)
		{
			if (level == 0)
			{
				// Allow for "identifier(...)".
				if ((i > 1) && (i + 1 < tokens.Count))
					entirelyParenthesized = false;

				if (IsOperator(tokens[i], out var op))
				{
					var precedence = op.GetPrecedence();

					if ((lastOperatorIndex < 0) || (lastOperatorPrecedence <= precedence))
					{
						lastOperatorIndex = i;
						lastOperatorPrecedence = precedence;
					}
				}
			}

			switch (tokens[i].Type)
			{
				case TokenType.OpenParenthesis: level++; break;
				case TokenType.CloseParenthesis: level--; break;
			}

			if (level < 0)
				throw new SyntaxErrorException(tokens[i], "Expected: end of statement");
		}

		if (lastOperatorIndex > 0)
		{
			return new BinaryExpression(
				ParseExpression(tokens.Slice(0, lastOperatorIndex), tokens[lastOperatorIndex]),
				tokens[lastOperatorIndex],
				ParseExpression(tokens.Slice(lastOperatorIndex + 1), endToken));
		}
		else
		{
			if ((tokens.Count == 2)
			 && (tokens[0].Type == TokenType.Minus)
			 && (tokens[1].Type == TokenType.Number))
			{
				// Coalesce "-" and a number into a single token for a negative number.
				return new LiteralExpression(
					new Token(
						tokens[0].Line,
						tokens[0].Column,
						tokens[1].Type,
						"-" + tokens[1].Value,
						tokens[1].DataType));
			}

			if (lastOperatorIndex == 0)
				throw new SyntaxErrorException(tokens[0], "Expected: expression");

			if ((tokens[0].Type == TokenType.Minus) || (tokens[0].Type == TokenType.NOT))
				return new UnaryExpression(tokens[0], ParseExpression(tokens.Slice(1), endToken));

			if (entirelyParenthesized)
			{
				if (tokens[0].Type == TokenType.OpenParenthesis)
					return new ParenthesizedExpression(tokens[0], ParseExpression(tokens.Slice(1, tokens.Count - 2), tokens.Last()));
				if (tokens[0].Type == TokenType.Identifier)
					return new CallOrIndexExpression(tokens[0], ParseExpressionList(tokens.Slice(2, tokens.Count - 3), tokens.Last()));
			}

			if (tokens.Count == 1)
			{
				if ((tokens[0].Type == TokenType.Number) || (tokens[0].Type == TokenType.String))
					return new LiteralExpression(tokens[0]);
				if (tokens[0].Type == TokenType.Identifier)
					return new IdentifierExpression(tokens[0]);
			}

			throw new SyntaxErrorException(tokens[0], "Expected: expression");
		}
	}

	private bool IsOperator(Token token, out Operator op)
	{
		switch (token.Type)
		{
			case TokenType.Plus: op = Operator.Add; return true;
			case TokenType.Minus: op = Operator.Subtract; return true;
			case TokenType.Asterisk: op = Operator.Multiply; return true;
			case TokenType.Slash: op = Operator.Divide; return true;
			case TokenType.Caret: op = Operator.Exponentiate; return true;
			case TokenType.Backslash: op = Operator.IntegerDivide; return true;
			case TokenType.MOD: op = Operator.Modulo; return true;

			case TokenType.Equals: op = Operator.Equals; return true;
			case TokenType.NotEquals: op = Operator.NotEquals; return true;
			case TokenType.LessThan: op = Operator.LessThan; return true;
			case TokenType.LessThanOrEquals: op = Operator.LessThanOrEquals; return true;
			case TokenType.GreaterThan: op = Operator.GreaterThan; return true;
			case TokenType.GreaterThanOrEquals: op = Operator.GreaterThanOrEquals; return true;

			case TokenType.AND: op = Operator.And; return true;
			case TokenType.OR: op = Operator.Or; return true;
			case TokenType.XOR: op = Operator.ExclusiveOr; return true;
			case TokenType.EQV: op = Operator.Equivalent; return true;
			case TokenType.IMP: op = Operator.Implies; return true;
		}

		op = default;
		return false;
	}
}
