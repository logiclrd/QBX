using Microsoft.VisualBasic.FileIO;
using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using System;
using System.Reflection.Metadata;

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

		while (enumerator.MoveNext())
		{
			var token = enumerator.Current;

			if (token.Type == TokenType.NewLine)
			{
				line.Statements.Add(ParseStatement(buffer, colonAfter: false));
				buffer.Clear();
				yield return line;
				line = new CodeLine();
				haveContent = false;
			}
			else if ((token.Type == TokenType.Whitespace) && !line.Statements.Any())
				line.Indentation += token.Value;
			else if ((token.Type == TokenType.Number) && !line.Statements.Any())
			{
				if (line.LineNumber.HasValue)
					throw new SyntaxErrorException(token, "Expected: statement");

				line.Indentation = "";
				line.LineNumber = token.NumericValue;
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
						line.Statements.Add(ParseStatement(buffer, colonAfter: true));
						haveContent = true;
						buffer.Clear();
					}
				}
				else
					buffer.Add(token);
			}
		}

		if (buffer.Any() || haveContent)
			line.Statements.Add(ParseStatement(buffer, colonAfter: false));

		if (!line.IsEmpty)
			yield return line;
	}

	Statement ParseStatement(ListRange<Token> tokens, bool colonAfter)
	{
		if (!tokens.Where(token => token.Type != TokenType.Whitespace).Any())
			return new EmptyStatement();

		var tokenHandler = new TokenHandler(tokens);

		var token = tokenHandler.NextToken;

		tokenHandler.Advance();

		switch (token.Type)
		{
			case TokenType.Comment:
			{
				tokenHandler.ExpectEndOfStatement("Internal error: Additional tokens between Comment and Newline");

				string commentText = tokenHandler.NextToken.Value ?? "";

				var commentType = CommentStatementType.Apostrophe;

				if (commentText.StartsWith("'"))
					commentText = commentText.Substring(1);
				else if (commentText.StartsWith("REM"))
				{
					commentType = CommentStatementType.REM;
					commentText = commentText.Substring(3);
				}

				return new CommentStatement(commentType, commentText);
			}

			case TokenType.CALL:
			{
				string targetName = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);

				ExpressionList? arguments = null;

				if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
				{
					var argumentTokens = tokenHandler.ExpectParenthesizedTokens();

					arguments = ParseExpressionList(argumentTokens);
				}

				tokenHandler.ExpectEndOfStatement();

				return new CallStatement(CallStatementType.Explicit, targetName, arguments);
			}

			case TokenType.CASE:
			{
				var expressions = ParseExpressionList(tokenHandler.RemainingTokens);

				return new CaseStatement(expressions);
			}

			case TokenType.CLS:
			{
				if (tokenHandler.HasMoreTokens)
				{
					var mode = ParseExpression(tokenHandler.RemainingTokens);

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
					var arguments = ParseExpressionList(tokenHandler.RemainingTokens);

					if (arguments.Expressions.Count > 3)
						throw new SyntaxErrorException(tokenHandler.NextToken, "Expected no more than 3 arguments");

					return new ColorStatement(arguments);
				}
			}

			case TokenType.CONST:
			{
				var declarationSyntax = ParseExpressionList(tokenHandler.RemainingTokens);

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

				tokenHandler.ExpectEndOfStatement();

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

						defSeg.SegmentExpression = ParseExpression(tokenHandler.RemainingTokens);
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
						defFn.ExpressionBody = ParseExpression(tokenHandler.RemainingTokens);
						tokenHandler.AdvanceToEnd();
					}

					tokenHandler.ExpectEndOfStatement();

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

				tokenHandler.ExpectEndOfStatement();

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

				foreach (var range in SplitCommaDelimitedList(tokenHandler.RemainingTokens))
					dim.Declarations.Add(ParseVariableDeclaration(range));

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

					statement.Expression = ParseExpression(tokenHandler.RemainingTokens);
				}

				return statement;
			}

			case TokenType.ELSE:
			{
				tokenHandler.ExpectEndOfStatement();

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
					case TokenType.TYPE: endBlock = new EndTypeStatement(); break;

					case TokenType.SUB: endBlock = new EndScopeStatement() { ScopeType = ScopeType.Sub }; break;
					case TokenType.FUNCTION: endBlock = new EndScopeStatement() { ScopeType = ScopeType.Function }; break;

					default:
					{
						// Skip the common tail for this case.
						return
							new EndStatement()
							{
								Expression = ParseExpression(tokenHandler.RemainingTokens)
							};
					}
				}

				tokenHandler.Advance();
				tokenHandler.ExpectEndOfStatement();

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

				forStatement.StartExpression = ParseExpression(rangeExpressions[0]);
				forStatement.EndExpression = ParseExpression(rangeExpressions[1]);

				if (clauses.Count > 2)
					throw new SyntaxErrorException(clauses[2].First(), "Expected: end of statement");

				if (clauses.Count == 2)
					forStatement.StepExpression = ParseExpression(clauses[1]);

				return forStatement;
			}

			case TokenType.GOTO:
			case TokenType.GOSUB:
			{
				var statement =
					tokenHandler.NextToken.Type switch
					{
						TokenType.GOTO => new GoToStatement(),
						TokenType.GOSUB => new GoSubStatement(),

						_ => throw new Exception("Internal error")
					};

				tokenHandler.Advance();
				tokenHandler.ExpectMoreTokens();

				switch (tokenHandler.NextToken.Type)
				{
					case TokenType.Number:
						statement.TargetLineNumber = tokenHandler.NextToken.NumericValue;
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
				tokenHandler.ExpectEndOfStatement();

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

				statement.ConditionExpression = ParseExpression(tokens.Slice(1, tokens.Count - 2));

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

					input.FileNumberExpression = ParseExpression(fileNumberToken);

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

						lineInput.FileNumberExpression = ParseExpression(fileNumberToken);

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

					lineInput.Variable = tokenHandler.Expect(TokenType.Identifier);

					tokenHandler.ExpectEndOfStatement();

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

					if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis)
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

						lineStatement.FromXExpression = ParseExpression(fromExpressions[0]);
						lineStatement.ToYExpression = ParseExpression(fromExpressions[0]);
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

					lineStatement.ToXExpression = ParseExpression(toExpressions[0]);
					lineStatement.ToYExpression = ParseExpression(toExpressions[0]);

					// [, [color] [, [B[F]] [, style]]]
					if (tokenHandler.NextTokenIs(TokenType.Comma))
					{
						tokenHandler.Advance();
						tokenHandler.ExpectMoreTokens();

						var options = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

						lineStatement.ColorExpression = ParseExpression(options[0]);

						if (options.Count > 1)
						{
							var drawStyle = options[1];

							if (drawStyle.Any())
							{
								if ((drawStyle.Count > 1)
								 || (drawStyle[0].Type != TokenType.Identifier)
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
								lineStatement.StyleExpression = ParseExpression(options[2]);

								if (options.Count > 3)
								{
									var range = options[3].Unwrap();

									throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
								}
							}
						}
					}

					tokenHandler.ExpectEndOfStatement();

					return lineStatement;
				}
			}

			case TokenType.LOCATE:
			{
				var locate = new LocateStatement();

				var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

				locate.RowExpression = ParseExpression(arguments[0]);

				if ((arguments.Count > 1) && arguments[1].Any())
					locate.ColumnExpression = ParseExpression(arguments[1]);

				if ((arguments.Count > 2) && arguments[2].Any())
					locate.CursorVisibilityExpression = ParseExpression(arguments[2]);

				if (arguments.Count > 3)
					locate.CursorStartExpression = ParseExpression(arguments[3]);
				if (arguments.Count > 4)
					locate.CursorEndExpression = ParseExpression(arguments[4]);

				if (arguments.Count > 5)
				{
					var range = arguments[5].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
				}

				return locate;
			}

			case TokenType.NEXT:
			{
				var next = new NextStatement();

				if (tokenHandler.HasMoreTokens)
				{
					var counters = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

					for (int i = 0; i < counters.Count; i++)
					{
						var counter = ParseExpression(counters[i]);

						if (counter is not IdentifierExpression)
							throw new SyntaxErrorException(counters[i][0], "Expected: identifier");

						next.CounterExpressions.Add(counter);
					}
				}

				return next;
			}

			case TokenType.PLAY:
			{
				var play = new PlayStatement();

				play.CommandExpression = ParseExpression(tokenHandler.RemainingTokens);

				return play;
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

				poke.AddressExpression = ParseExpression(arguments[0]);
				poke.ValueExpression = ParseExpression(arguments[1]);

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

					print.FileNumberExpression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, separatorIndex));

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

					print.UsingExpression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, separatorIndex));

					tokenHandler.Advance(separatorIndex + 1);
				}

				while (tokenHandler.HasMoreTokens)
				{
					var nextSeparatorIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.Semicolon, TokenType.Comma);

					if (nextSeparatorIndex >= 0)
					{
						var arg = new PrintArgument();

						if (nextSeparatorIndex > 0)
							arg.Expression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, nextSeparatorIndex));

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

						arg.Expression = ParseExpression(tokenHandler.RemainingTokens);
						arg.CursorAction = PrintCursorAction.NextLine;

						print.Arguments.Add(arg);

						tokenHandler.AdvanceToEnd();
					}
				}

				return print;
			}

			case TokenType.RANDOMIZE:
			{
				var randomize = new RandomizeStatement();

				if (tokenHandler.HasMoreTokens)
					randomize.Expression = ParseExpression(tokenHandler.RemainingTokens);

				return randomize;
			}
		}

	/*
	case TokenType.READ,
	case TokenType.RESTORE,
	case TokenType.RETURN,
	case TokenType.SCREEN,
	case TokenType.SELECT,
	case TokenType.SHARED,
	case TokenType.STATIC,
	case TokenType.SUB,
	case TokenType.FUNCTION,
	case TokenType.TIMER,
	case TokenType.TYPE,
	case TokenType.VIEW,
	case TokenType.WEND,
	case TokenType.WHILE,
	case TokenType.WIDTH,
	*/

		// If not one of the above, then one of:
		//   subname argumentlist
		//   variablename = expression
		//   identifier:
	}

	IEnumerable<ListRange<Token>> SplitCommaDelimitedList(ListRange<Token> tokens)
		=> SplitDelimitedList(tokens, TokenType.Comma);

	IEnumerable<ListRange<Token>> SplitDelimitedList(ListRange<Token> tokens, TokenType delimiterType)
	{
		int nesting = 0;
		int itemStart = 0;

		for (int i = 0; i < tokens.Count; i++)
		{
			if (tokens[i].Type == delimiterType)
			{
				if (nesting == 0)
				{
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

			yield return tokens.Slice(itemStart);
		}
	}

	VariableDeclaration ParseVariableDeclaration(ListRange<Token> tokens)
	{
		var tokenHandler = new TokenHandler(tokens);

		var declaration = new VariableDeclaration();

		tokenHandler.ExpectMoreTokens("Expected variable declaration");

		declaration.Name = tokenHandler.ExpectIdentifier(allowTypeCharacter: true);

		if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
		{
			var subscriptTokens = tokenHandler.ExpectParenthesizedTokens();

			declaration.Subscripts = new VariableDeclarationSubscriptList();

			foreach (var subscript in SplitCommaDelimitedList(subscriptTokens))
				declaration.Subscripts.Add(ParseVariableDeclarationSubscript(subscript));
		}

		return declaration;
	}

	private VariableDeclarationSubscript ParseVariableDeclarationSubscript(ListRange<Token> subscriptTokens)
	{
		var boundExpressions = SplitDelimitedList(subscriptTokens, TokenType.TO).ToList();

		if (boundExpressions.Count > 2)
		{
			var range = boundExpressions[2].Unwrap();

			throw new SyntaxErrorException(range.List[range.Offset - 1], "Expected: )");
		}

		var subscript = new VariableDeclarationSubscript();

		subscript.Bound1 = ParseExpression(boundExpressions[0]);
		subscript.Bound2 = ParseExpression(boundExpressions[1]);

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
			{
				param.TypeCharacter = typeCharacter;
				param.Name = param.Name.Remove(param.Name.Length - 1);
			}
			else if (tokenIndex < tokens.Count)
			{
				if (tokens[tokenIndex].Type != TokenType.AS)
					throw new SyntaxErrorException(tokens[tokenIndex], "Expected AS");

				tokenIndex++;

				if (!tokens[tokenIndex].IsDataType)
					throw new SyntaxErrorException(tokens[tokenIndex], "Expected data type");

				param.Type = DataTypeConverter.FromToken(tokens[tokenIndex]);

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

	ExpressionList ParseExpressionList(ListRange<Token> tokens)
	{
		var list = new ExpressionList();

		foreach (var range in SplitCommaDelimitedList(tokens))
			list.Expressions.Add(ParseExpression(range));

		return list;
	}

	Expression ParseExpression(ListRange<Token> tokens)
	{
		throw new Exception("TODO");
	}
}
