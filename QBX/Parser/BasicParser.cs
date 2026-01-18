using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Numbers;

namespace QBX.Parser;

public class BasicParser
{
	public CompilationUnit Parse(IEnumerable<Token> tokenStream, bool ignoreErrors = false)
	{
		var unit = new CompilationUnit();

		var mainElement = new CompilationElement(unit);

		mainElement.Type = CompilationElementType.Main;

		unit.Elements.Add(mainElement);

		var element = mainElement;
		bool betweenElements = false;

		var prelude = new List<CodeLine>();

		foreach (var line in ParseCodeLines(tokenStream, ignoreErrors))
		{
			if (((element == mainElement) || betweenElements)
			 && (line.IsCommentLine || line.IsDefTypeLine))
				prelude.Add(line);
			else
			{
				if (betweenElements && line.IsEmpty)
				{
					if (prelude.Any())
					{
						mainElement.AddLines(prelude);
						prelude.Clear();
					}

					continue;
				}

				if (line.Statements.Any())
				{
					var firstStatement = line.Statements.First();
					var lastStatement = line.Statements.Last();

					if ((firstStatement.Type == StatementType.Sub) || (firstStatement.Type == StatementType.Function))
					{
						betweenElements = false;

						element = new CompilationElement(unit);
						unit.Elements.Add(element);

						element.AddLines(prelude);
						prelude.Clear();

						switch (firstStatement)
						{
							case SubStatement sub:
								element.Type = CompilationElementType.Sub;
								element.Name = sub.Name;
								break;
							case FunctionStatement function:
								element.Type = CompilationElementType.Function;
								element.Name = function.Name; // TODO: strip type character
								break;
						}

						element.AddLine(line);

						continue;
					}
					else if (lastStatement.Type == StatementType.EndScope)
					{
						element.AddLine(line);
						element = mainElement;

						betweenElements = true;

						continue;
					}
				}

				element.AddLines(prelude);
				prelude.Clear();

				element.AddLine(line);
			}
		}

		mainElement.AddLines(prelude);

		return unit;
	}

	internal IEnumerable<CodeLine> ParseCodeLines(IEnumerable<Token> tokenStream, bool ignoreErrors = false)
	{
		var enumerator = tokenStream.GetEnumerator();
		bool tokenPeeked = false;

		var line = new CodeLine();
		var buffer = new List<Token>();
		bool haveContent = false;
		int sourceLineNumber = 1;
		Token? precedingWhitespaceToken = null;
		bool startsWithDATA = false;
		bool lineConsumed = false;
		Token? lastToken = null;

		while (tokenPeeked || enumerator.MoveNext())
		{
			tokenPeeked = false;

			var token = enumerator.Current;

			if (token.Type == TokenType.NewLine)
			{
				if (precedingWhitespaceToken != null)
				{
					buffer.Add(precedingWhitespaceToken);
					precedingWhitespaceToken = null;
				}

				if ((line.EndOfLineComment == null) && !lineConsumed)
				{
					line.AppendStatement(
						ParseStatementWithIndentation(buffer, isNested: false, token, ignoreErrors));
				}

				buffer.Clear();
				yield return line;
				line = new CodeLine();
				haveContent = false;
				sourceLineNumber++;
				precedingWhitespaceToken = null;
				startsWithDATA = false;
				lineConsumed = false;
			}
			else if ((token.Type == TokenType.Number) &&
			         (line.LineNumber == null) &&
			         (line.Label == null) &&
			         !line.Statements.Any() &&
			         !buffer.Any())
			{
				if (precedingWhitespaceToken != null)
				{
					buffer.Add(precedingWhitespaceToken);
					precedingWhitespaceToken = null;
				}

				if (line.LineNumber != null)
					throw new SyntaxErrorException(token, "Expected: statement");

				line.LineNumber = token.Value;

				precedingWhitespaceToken = null;
			}
			else
			{
				if (token.Type == TokenType.Colon)
				{
					int labelIndex = 0;
					string whitespace = "";

					if ((labelIndex < buffer.Count)
					 && (buffer[labelIndex].Type == TokenType.Whitespace))
					{
						whitespace = buffer[labelIndex].Value ?? "";
						labelIndex++;
					}

					if (!line.Statements.Any()
					 && (line.Label == null)
					 && (buffer.Count == labelIndex + 1)
					 && (buffer[labelIndex].Type == TokenType.Identifier)
					 && (buffer[labelIndex].Value is string labelName)
					 && (labelName.Length > 0)
					 && !char.IsSymbol(labelName.Last()))
					{
						line.Label =
							new Label()
							{
								Indentation = whitespace,
								Name = labelName,
							};

						buffer.Clear();
					}
					else
					{
						IEnumerable<Token> ConsumeTokensToEndOfLine()
						{
							yield return token;

							while (enumerator.MoveNext())
							{
								token = enumerator.Current;

								if (token.Type == TokenType.NewLine)
								{
									tokenPeeked = true;
									break;
								}

								yield return token;
							}

							lineConsumed = true;
						}

						line.AppendStatement(
							ParseStatementWithIndentation(buffer, ConsumeTokensToEndOfLine, isNested: false, precedingWhitespaceToken ?? token, ignoreErrors));
						buffer.Clear();
					}

					haveContent = true;
					precedingWhitespaceToken = null;
					startsWithDATA = false;
				}
				else if ((token.Type == TokenType.Comment) && (token.Value != null) && token.Value.StartsWith("'"))
				{
					if (buffer.Any() || line.Statements.Any())
					{
						line.AppendStatement(ParseStatementWithIndentation(buffer, precedingWhitespaceToken ?? token, ignoreErrors));
						buffer.Clear();
					}

					line.EndOfLineComment = precedingWhitespaceToken?.Value + token.Value;
					haveContent = true;
					precedingWhitespaceToken = null;
				}
				else if (token.Type == TokenType.Whitespace)
					precedingWhitespaceToken = token;
				else
				{
					bool willStartWithDATA = startsWithDATA;

					if (!buffer.Any() && (token.Type == TokenType.DATA))
						willStartWithDATA = true;

					if (precedingWhitespaceToken != null)
					{
						if (startsWithDATA || (token.Type == TokenType.AS))
							token.PrecedingWhitespace = precedingWhitespaceToken.Value!;
						else
							buffer.Add(precedingWhitespaceToken);

						precedingWhitespaceToken = null;
					}

					startsWithDATA = willStartWithDATA;

					buffer.Add(token);
				}
			}

			lastToken = token;
		}

		if (buffer.Any() || haveContent)
		{
			if (lastToken == null)
				throw new Exception("Internal error: Arrived at tail with non-empty buffer and/or haveContent but without seeing any tokens");

			var endToken = new Token(
				lastToken.Line,
				lastToken.Column + lastToken.Length,
				TokenType.Empty,
				"");

			line.AppendStatement(ParseStatementWithIndentation(buffer, isNested: false, endToken, ignoreErrors));
		}

		if (!line.IsEmpty)
			yield return line;
	}

	internal Statement ParseStatementWithIndentation(ListRange<Token> tokens, Token endToken, bool ignoreErrors = false)
	{
		return ParseStatementWithIndentation(tokens, consumeTokensToEndOfLine: () => Array.Empty<Token>(), isNested: false, endToken, ignoreErrors);
	}

	internal Statement ParseStatementWithIndentation(ListRange<Token> tokens, bool isNested, Token endToken, bool ignoreErrors)
	{
		return ParseStatementWithIndentation(tokens, consumeTokensToEndOfLine: () => Array.Empty<Token>(), isNested, endToken, ignoreErrors);
	}

	internal Statement ParseStatementWithIndentation(ListRange<Token> tokens, Func<IEnumerable<Token>> consumeTokensToEndOfLine, bool isNested, Token endToken, bool ignoreErrors)
	{
		var indentation = "";

		if ((tokens.Count > 0) && (tokens[0].Type == TokenType.Whitespace))
		{
			indentation = tokens[0].Value ?? "";
			tokens = tokens.Slice(1);
		}

		try
		{
			var statement = ParseStatement(tokens, consumeTokensToEndOfLine, isNested, ignoreErrors);

			statement.Indentation = indentation;

			int lastTokenIndex = tokens.Count - 1;

			while ((lastTokenIndex >= 0) && tokens[lastTokenIndex].Type == TokenType.Whitespace)
				lastTokenIndex--;

			statement.FirstToken = tokens.Any() ? tokens[0] : endToken;
			statement.SourceLength = tokens.Take(lastTokenIndex).Sum(token => token.Length);

			return statement;
		}
		catch when (ignoreErrors)
		{
			return new UnparsedStatement(indentation, tokens);
		}
	}

	internal Statement ParseStatement(ListRange<Token> tokens, bool ignoreErrors)
	{
		return ParseStatement(tokens, consumeTokensToEndOfLine: () => Array.Empty<Token>(), isNested: false, ignoreErrors);
	}

	internal Statement ParseStatement(ListRange<Token> tokens, bool isNested, bool ignoreErrors)
	{
		return ParseStatement(tokens, consumeTokensToEndOfLine: () => Array.Empty<Token>(), isNested, ignoreErrors);
	}

	internal Statement ParseStatement(ListRange<Token> tokens, Func<IEnumerable<Token>> consumeTokensToEndOfLine, bool isNested, bool ignoreErrors)
	{
		// TODO: DRAW
		// TODO: GET, PUT (graphics)
		// TODO: OPTION BASE
		// TODO: PAINT
		// TODO: SWAP a, b
		// TODO: WINDOW
		// TODO: NAME a AS b
		// TODO: SOUND a, b
		//       => make a list
		//
		// TODO: functions, such as: ABS, COS, INKEY$, INPUT$(n), CHR$()
		//       => go find/make a list of them

		if (!tokens.Any(token => token.Type != TokenType.Whitespace))
			return new EmptyStatement();

		if (tokens.Any(token => token.Type == TokenType.Whitespace))
			tokens = tokens.Where(token => token.Type != TokenType.Whitespace).ToList();

		var tokenHandler = new TokenHandler(tokens);

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
				if (tokenHandler.NextTokenIs(TokenType.ELSE))
				{
					tokenHandler.Advance();
					tokenHandler.ExpectEndOfTokens();

					return new CaseStatement() { MatchElse = true };
				}
				else
				{
					var expressions = ParseCaseExpressionList(tokenHandler.RemainingTokens, tokenHandler.EndToken);

					return new CaseStatement(expressions);
				}
			}

			case TokenType.CIRCLE:
			{
				var circle = new CircleStatement();

				if (tokenHandler.NextTokenIs(TokenType.STEP))
				{
					circle.Step = true;
					tokenHandler.Advance();
				}

				var coordinateTokens = tokenHandler.ExpectParenthesizedTokens();

				var coordinates = SplitCommaDelimitedList(coordinateTokens).ToList();

				if (coordinates.Count == 1)
					throw new SyntaxErrorException(tokenHandler.PreviousToken, "Expected: expression");
				if (coordinates.Count > 2)
				{
					var blame = tokens[coordinates[2].Unwrap().Offset - 1];

					throw new SyntaxErrorException(blame, "Expected: )");
				}

				var midToken = tokens[coordinates[1].Unwrap().Offset - 1];

				circle.XExpression = ParseExpression(coordinates[0], midToken);
				circle.YExpression = ParseExpression(coordinates[1], tokenHandler.PreviousToken);

				tokenHandler.Expect(TokenType.Comma);

				if (!tokenHandler.HasMoreTokens)
					throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: expression");

				var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

				if (!arguments.Last().Any())
					throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: expression");

				midToken = arguments.Count > 1
					? tokens[arguments[1].Unwrap().Offset - 1]
					: tokenHandler.EndToken;

				circle.RadiusExpression = ParseExpression(arguments[0], midToken);

				if (arguments.Count > 1)
				{
					if (arguments[1].Any())
					{
						midToken = arguments.Count > 2
							? tokens[arguments[2].Unwrap().Offset - 1]
							: tokenHandler.EndToken;

						circle.ColourExpression = ParseExpression(arguments[1], midToken);
					}

					if (arguments.Count > 2)
					{
						if (arguments[2].Any())
						{
							midToken = arguments.Count > 3
								? tokens[arguments[3].Unwrap().Offset - 1]
								: tokenHandler.EndToken;

							circle.StartExpression = ParseExpression(arguments[2], midToken);
						}

						if (arguments.Count > 3)
						{
							if (arguments[3].Any())
							{
								midToken = arguments.Count > 4
									? tokens[arguments[4].Unwrap().Offset - 1]
									: tokenHandler.EndToken;

								circle.EndExpression = ParseExpression(arguments[3], midToken);
							}

							if (arguments.Count > 5)
							{
								var blame = tokens[arguments[5].Unwrap().Offset - 1];

								throw new SyntaxErrorException(blame, "Expected: end of statement");
							}

							if (arguments.Count == 5)
								circle.AspectExpression = ParseExpression(arguments[4], tokenHandler.EndToken);
						}
					}
				}

				return circle;
			}

			case TokenType.CLOSE:
			{
				var closeStatement = new CloseStatement();

				if (tokenHandler.HasMoreTokens)
				{
					var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

					for (int i = 0; i < arguments.Count; i++)
					{
						var midToken =
							(i + 1 < arguments.Count)
							? tokens[arguments[i + 1].Unwrap().Offset - 1]
							: tokenHandler.EndToken;

						if (arguments[i].Count == 0)
							throw new SyntaxErrorException(midToken, "Expected: expression");

						if (arguments[i].First().Type == TokenType.NumberSign)
							arguments[i] = arguments[i].Slice(1);

						closeStatement.FileNumberExpressions.Add(
							ParseExpression(arguments[i], midToken));
					}
				}

				return closeStatement;
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
				var color = new ColorStatement();

				if (tokenHandler.HasMoreTokens) // "COLOR" is a runtime error but should parse
				{
					var endTokenRef = new TokenRef();

					foreach (var range in SplitCommaDelimitedList(tokenHandler.RemainingTokens, endTokenRef))
					{
						if (range.Any())
							color.Arguments.Add(ParseExpression(range, endTokenRef.Token ?? tokenHandler.EndToken));
						else
							color.Arguments.Add(null);
					}

					if (color.Arguments.Count > 3)
						throw new SyntaxErrorException(tokenHandler.NextToken, "Expected no more than 3 arguments");
				}

				return color;
			}

			case TokenType.CONST:
			{
				var declarationSyntax = ParseExpressionList(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				var definitions = new List<ConstDefinition>();

				for (int i = 0; i < declarationSyntax.Expressions.Count; i++)
				{
					var syntax = declarationSyntax.Expressions[i];

					if ((syntax is not BinaryExpression binarySyntax)
					 || (binarySyntax.Operator != Operator.Equals)
					 || (binarySyntax.Left is not IdentifierExpression identifierSyntax))
						throw new SyntaxErrorException(syntax.Token ?? tokenHandler.NextToken, "Expected: name = value");

					definitions.Add(new ConstDefinition(identifierSyntax.Identifier, binarySyntax.Right));
				}

				return new ConstStatement(definitions);
			}

			case TokenType.DATA:
			{
				string dataString = "";

				if (tokenHandler.HasMoreTokens)
				{
					var rawString = tokenHandler.Expect(TokenType.RawString);

					dataString = rawString.Value;
				}

				tokenHandler.ExpectEndOfTokens();

				return new DataStatement(dataString);
			}

			case TokenType.DECLARE:
			{
				if (isNested)
					throw new SyntaxErrorException(token, "COMMON and DECLARE must precede executable statements");

				var declarationType = tokenHandler.ExpectOneOf(TokenType.SUB, TokenType.FUNCTION);

				var name = tokenHandler.ExpectIdentifier(allowTypeCharacter: true);

				ParameterList? parameters = null;

				bool isCDecl = false;
				string? alias = null;

				if (tokenHandler.NextTokenIs(TokenType.CDECL))
				{
					isCDecl = true;
					tokenHandler.Advance();
				}

				if (tokenHandler.NextTokenIs(TokenType.ALIAS))
				{
					tokenHandler.Advance();

					if (!tokenHandler.NextTokenIs(TokenType.String))
						throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: string constant");

					alias = tokenHandler.NextToken.StringLiteralValue;

					tokenHandler.Advance();
				}

				if (tokenHandler.HasMoreTokens)
				{
					var parameterTokens = tokenHandler.ExpectParenthesizedTokens();

					parameters = ParseParameterList(parameterTokens);
				}

				tokenHandler.ExpectEndOfTokens();

				var declare = new DeclareStatement(declarationType, name, parameters);

				declare.IsCDecl = isCDecl;
				declare.Alias = alias;

				return declare;
			}

			case TokenType.DEF:
			{
				if (isNested)
					throw new SyntaxErrorException(token, "Syntax error: DEF FN may not be nested");

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

						defFn.Parameters = ParseParameterList(parameterListTokens, allowByVal: false, allowArray: false);
					}

					if (tokenHandler.HasMoreTokens && (tokenHandler.NextToken.Type == TokenType.Equals))
					{
						tokenHandler.Advance();
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

				TokenRef endTokenRef = new TokenRef();

				foreach (var rangeTokens in SplitCommaDelimitedList(tokenHandler.RemainingTokens, endTokenRef))
				{
					var rangeTokenHandler = new TokenHandler(rangeTokens);

					var range = new DefTypeRange();

					string? rangeStart = null;
					string? rangeEnd = null;

					rangeStart = rangeTokenHandler.ExpectIdentifier(allowTypeCharacter: false, out var identifierToken);

					if (rangeStart.Length != 1)
						throw new SyntaxErrorException(identifierToken, "Expected: letter");

					range.Start = char.ToUpperInvariant(rangeStart[0]);

					if (rangeTokenHandler.HasMoreTokens)
					{
						rangeTokenHandler.Expect(TokenType.Minus);
						rangeTokenHandler.ExpectMoreTokens();

						rangeEnd = rangeTokenHandler.ExpectIdentifier(allowTypeCharacter: false, out identifierToken);

						if (rangeEnd.Length != 1)
							throw new SyntaxErrorException(identifierToken, "Expected: letter");

						range.End = char.ToUpperInvariant(rangeEnd[0]);
					}

					range.Normalize();

					if (endTokenRef.Token != null)
						rangeTokenHandler.ExpectEndOfTokens("Expected: ,");
					else
						rangeTokenHandler.ExpectEndOfTokens("Expected: end of statement");

					defType.AddRange(range);
				}

				return defType;
			}

			case TokenType.DIM:
			case TokenType.REDIM:
			{
				DimStatement dim;

				tokenHandler.ExpectMoreTokens();

				bool requireSubscripts = false;

				switch (token.Type)
				{
					case TokenType.DIM: dim = new DimStatement(); break;
					case TokenType.REDIM:
					{
						requireSubscripts = true;

						var redim = new RedimStatement();

						if (tokenHandler.NextTokenIs(TokenType.PRESERVE))
						{
							redim.Preserve = true;
							tokenHandler.Advance();
						}

						dim = redim;

						break;
					}

					default: throw new Exception("Internal error");
				}

				if (tokenHandler.NextTokenIs(TokenType.SHARED))
				{
					dim.Shared = true;
					tokenHandler.Advance();
				}

				var endTokenRef = new TokenRef();

				foreach (var range in SplitCommaDelimitedList(tokenHandler.RemainingTokens, endTokenRef))
					dim.Declarations.Add(ParseVariableDeclaration(range, endTokenRef.Token ?? tokenHandler.EndToken, requireSubscripts));

				return dim;
			}
			case TokenType.DO:
			case TokenType.LOOP:
			{
				var statement =
					token.Type switch
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
				if (isNested)
					throw new SyntaxErrorException(token, "Syntax error: ELSE must be first statement in line");

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

					case TokenType.TYPE: endBlock = new EndTypeStatement(); break;

					default:
					{
						// Skip the common tail for this case.
						return
							new EndStatement()
							{
								ExitCodeExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken)
							};
					}
				}

				if (isNested)
					throw new SyntaxErrorException(token, $"Syntax error: END {tokenHandler.NextToken.Type} may not be nested");

				tokenHandler.Advance();
				tokenHandler.ExpectEndOfTokens();

				return endBlock;
			}

			case TokenType.ENDIF:
			{
				tokenHandler.ExpectEndOfTokens();
				return new EndIfStatement();
			}

			case TokenType.EXIT:
			{
				// One of:
				//   EXIT DEF
				//   EXIT SUB
				//   EXIT FUNCTION
				//   EXIT DO
				//   EXIT FOR

				var scopeType = tokenHandler.ExpectOneOf(
					TokenType.DEF,
					TokenType.SUB,
					TokenType.FUNCTION,
					TokenType.DO,
					TokenType.FOR);

				tokenHandler.ExpectEndOfTokens();

				var exitScope = new ExitScopeStatement();

				exitScope.ScopeType =
					scopeType.Type switch
					{
						TokenType.DEF => ScopeType.Def,
						TokenType.SUB => ScopeType.Sub,
						TokenType.FUNCTION => ScopeType.Function,
						TokenType.DO => ScopeType.Do,
						TokenType.FOR => ScopeType.For,

						_ => throw new Exception("Internal error")
					};

				return exitScope;
			}

			case TokenType.FIELD:
			{
				var field = new FieldStatement();

				if (tokenHandler.NextTokenIs(TokenType.NumberSign))
					tokenHandler.Advance();

				bool isFileNumberArgument = true;

				var endTokenRef = new TokenRef();

				foreach (var argument in SplitCommaDelimitedList(tokenHandler.RemainingTokens, endTokenRef))
				{
					if (isFileNumberArgument)
						field.FileNumberExpression = ParseExpression(argument, endTokenRef.Token ?? tokenHandler.EndToken);
					else
						field.FieldDefinitions.Add(ParseFieldDefinition(argument, endTokenRef.Token ?? tokenHandler.EndToken));

					isFileNumberArgument = false;
				}

				if (field.FieldDefinitions.Count == 0)
					throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: ,");

				return field;
			}

			case TokenType.FOR:
			{
				var forStatement = new ForStatement();

				forStatement.CounterVariable = tokenHandler.ExpectIdentifier(allowTypeCharacter: true);

				tokenHandler.Expect(TokenType.Equals);

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

			case TokenType.GET:
			case TokenType.PUT:
			{
				if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis)
				 || tokenHandler.NextTokenIs(TokenType.STEP))
				{
					switch (token.Type)
					{
						case TokenType.GET:
						{
							var getSpriteStatement = new GetSpriteStatement();

							if (tokenHandler.NextTokenIs(TokenType.STEP))
							{
								getSpriteStatement.FromStep = true;
								tokenHandler.Advance();

								if (!tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
									throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: (");
							}

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

							{
								var midToken = tokens[fromExpressions[1].Unwrap().Offset - 1];
								var endToken = tokenHandler.PreviousToken;

								getSpriteStatement.FromXExpression = ParseExpression(fromExpressions[0], midToken);
								getSpriteStatement.FromYExpression = ParseExpression(fromExpressions[1], endToken);
							}

							tokenHandler.Expect(TokenType.Hyphen);

							if (tokenHandler.NextTokenIs(TokenType.STEP))
							{
								getSpriteStatement.ToStep = true;
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

								getSpriteStatement.ToXExpression = ParseExpression(toExpressions[0], midToken);
								getSpriteStatement.ToYExpression = ParseExpression(toExpressions[1], endToken);
							}

							// , array[(offset)]
							tokenHandler.Expect(TokenType.Comma);

							getSpriteStatement.TargetExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

							return getSpriteStatement;
						}
						case TokenType.PUT:
						{
							var putSpriteStatement = new PutSpriteStatement();

							if (tokenHandler.NextTokenIs(TokenType.STEP))
							{
								putSpriteStatement.Step = true;
								tokenHandler.Advance();

								if (!tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
									throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: (");
							}

							var coordinateTokens = tokenHandler.ExpectParenthesizedTokens();

							if (coordinateTokens.Count == 0)
							{
								var range = coordinateTokens.Unwrap();

								throw new SyntaxErrorException(tokens[range.Offset], "Expected: expression");
							}

							var coordinateExpressions = SplitCommaDelimitedList(coordinateTokens).ToList();

							if (coordinateExpressions.Count == 1)
							{
								var range = coordinateTokens.Unwrap();

								throw new SyntaxErrorException(tokens[range.Offset + range.Count], "Expected: ,");
							}

							if (coordinateExpressions.Count > 2)
							{
								var range = coordinateExpressions[1].Unwrap();

								throw new SyntaxErrorException(tokens[range.Offset + range.Count], "Expected: )");
							}

							{
								var midToken = tokens[coordinateExpressions[1].Unwrap().Offset - 1];
								var endToken = tokenHandler.PreviousToken;

								putSpriteStatement.XExpression = ParseExpression(coordinateExpressions[0], midToken);
								putSpriteStatement.YExpression = ParseExpression(coordinateExpressions[1], endToken);
							}

							// , array[(offset)][, actionverb]
							tokenHandler.Expect(TokenType.Comma);

							int separator = tokenHandler.FindNextUnparenthesizedOf(TokenType.Comma);

							if (separator < 0)
								putSpriteStatement.SourceExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);
							else
							{
								putSpriteStatement.SourceExpression = ParseExpression(
									tokenHandler.RemainingTokens.Slice(0, separator),
									tokenHandler[separator]);

								tokenHandler.Advance(separator);
								tokenHandler.Expect(TokenType.Comma);

								var actionVerbToken = tokenHandler.NextToken ?? tokenHandler.EndToken;

								Exception ThrowForActionVerb()
									=> throw new SyntaxErrorException(actionVerbToken, "Expected: AND or OR or PRESENT or PSET or XOR");

								if (!tokenHandler.HasMoreTokens)
									ThrowForActionVerb();

								switch (actionVerbToken.Type)
								{
									case TokenType.AND: putSpriteStatement.ActionVerb = PutSpriteAction.And; break;
									case TokenType.OR: putSpriteStatement.ActionVerb = PutSpriteAction.Or; break;
									case TokenType.PRESET: putSpriteStatement.ActionVerb = PutSpriteAction.PixelSetInverted; break;
									case TokenType.PSET: putSpriteStatement.ActionVerb = PutSpriteAction.PixelSet; break;
									case TokenType.XOR: putSpriteStatement.ActionVerb = PutSpriteAction.ExclusiveOr; break;

									default: throw ThrowForActionVerb();
								}

								tokenHandler.Advance();
								tokenHandler.ExpectEndOfTokens();
							}

							return putSpriteStatement;
						}
					}
				}
				else
				{
					FileBlockOperationStatement statement;

					switch (token.Type)
					{
						case TokenType.GET: statement = new GetStatement(); break;
						case TokenType.PUT: statement = new PutStatement(); break;

						default: throw new Exception("Internal error");
					}

					if (tokenHandler.NextTokenIs(TokenType.NumberSign))
						tokenHandler.Advance();

					var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

					if (arguments.Count == 0)
						throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: expression");

					if (arguments.Count == 1)
						statement.FileNumberExpression = ParseExpression(arguments[0], tokenHandler.EndToken);
					else if (arguments.Count == 2)
					{
						var midToken = tokens[arguments[1].Unwrap().Offset - 1];

						statement.FileNumberExpression = ParseExpression(arguments[0], midToken);
						statement.RecordNumberExpression = ParseExpression(arguments[1], tokenHandler.EndToken);
					}
					else if (arguments.Count == 3)
					{
						var midToken = tokens[arguments[1].Unwrap().Offset - 1];

						statement.FileNumberExpression = ParseExpression(arguments[0], midToken);

						if (arguments[1].Any())
						{
							midToken = tokens[arguments[2].Unwrap().Offset - 1];

							statement.RecordNumberExpression = ParseExpression(arguments[1], midToken);
						}

						statement.TargetExpression = ParseExpression(arguments[2], tokenHandler.EndToken);
					}
					else
					{
						var blame = tokens[arguments[3].Unwrap().Offset - 1];

						throw new SyntaxErrorException(blame, "Expected: end of statement");
					}

					return statement;
				}

				break;
			}

			case TokenType.GOTO:
			case TokenType.GOSUB:
			case TokenType.RESTORE:
			case TokenType.RETURN:
			{
				var statement =
					token.Type switch
					{
						TokenType.GOTO => new GoToStatement(),
						TokenType.GOSUB => new GoSubStatement(),
						TokenType.RESTORE => new RestoreStatement(),
						TokenType.RETURN => new ReturnStatement(),

						_ => default(TargetLineStatement) ?? throw new Exception("Internal error")
					};

				if (statement.CanBeParameterless)
				{
					if (!tokenHandler.HasMoreTokens)
						return statement;
				}

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
				tokens = tokens.Concat(consumeTokensToEndOfLine()).ToList();

				tokenHandler = new TokenHandler(tokens);
				tokenHandler.Advance();

				var statement =
					token.Type switch
					{
						TokenType.IF => new IfStatement(),
						TokenType.ELSEIF => new ElseIfStatement(),

						_ => throw new Exception("Internal error")
					};

				var thenIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.THEN);

				if (thenIndex >= 0)
				{
					statement.ConditionExpression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, thenIndex), tokenHandler[thenIndex]);

					tokenHandler.Advance(thenIndex);
					tokenHandler.Expect(TokenType.THEN);
				}
				else
				{
					var gotoIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.GOTO);

					if (gotoIndex < 0)
						throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: THEN");

					statement.OmitThen = true;
					statement.ConditionExpression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, gotoIndex), tokenHandler[gotoIndex]);

					tokenHandler.Advance(gotoIndex);
				}

				if (isNested && !tokenHandler.HasMoreTokens)
					throw new SyntaxErrorException(token, "Syntax error: Block IF/ELSEIF must be first statement in line");

				if (tokenHandler.HasMoreTokens)
				{
					statement.ThenBody = new List<Statement>();

					void DoParseStatement(List<Statement> list, ListRange<Token> tokens, bool isNested, Token endToken)
					{
						if ((list.Count == 0)
						 && (tokens.Count == 1)
						 && (tokens[0].Type == TokenType.Number))
						{
							// Special legacy syntax:
							//   IF condition THEN linenumber
							//   IF condition THEN ... ELSE linenumber

							var statement = new BareLineNumberGoToStatement();

							statement.TargetLineNumber = tokens[0].Value;

							list.Add(statement);
						}
						else
						{
							var statement = ParseStatementWithIndentation(tokens, isNested, endToken, ignoreErrors);

							if (list.Count == 0)
								statement.Indentation = "";

							list.Add(statement);
						}
					}

					while (true)
					{
						int separator = tokenHandler.FindNextUnparenthesizedOf(TokenType.Colon, TokenType.ELSE);

						if ((separator < 0) || tokenHandler.NextTokenIs(TokenType.IF))
						{
							var endToken = tokenHandler.HasMoreTokens ? tokenHandler.NextToken : tokenHandler.EndToken;

							DoParseStatement(statement.ThenBody, tokenHandler.RemainingTokens, isNested: true, endToken);
							break;
						}

						DoParseStatement(statement.ThenBody, tokenHandler.RemainingTokens.Slice(0, separator), isNested: true, tokenHandler[separator]);

						tokenHandler.Advance(separator);

						if (tokenHandler.NextTokenIs(TokenType.ELSE))
						{
							if (statement is ElseIfStatement)
								throw new SyntaxErrorException(tokenHandler.NextToken, "Syntax error: ELSE after ELSEIF must be first statement on line");

							tokenHandler.Advance();

							statement.ElseBody = new List<Statement>();

							if (tokenHandler.HasMoreTokens)
							{
								while (true)
								{
									separator = tokenHandler.FindNextUnparenthesizedOf(TokenType.Colon);

									if ((separator < 0) || tokenHandler.NextTokenIs(TokenType.IF))
									{
										var endToken = tokenHandler.HasMoreTokens ? tokenHandler.NextToken : tokenHandler.EndToken;

										DoParseStatement(statement.ElseBody, tokenHandler.RemainingTokens, isNested: true, endToken);
										break;
									}

									DoParseStatement(statement.ElseBody, tokenHandler.RemainingTokens.Slice(0, separator), isNested: true, tokenHandler[separator]);

									tokenHandler.Advance(separator);
									tokenHandler.Expect(TokenType.Colon);
								}
							}

							break;
						}

						tokenHandler.Advance();
					}
				}

				return statement;
			}

			case TokenType.INPUT:
			{
				// One of:
				//   INPUT [;] [prompt {;|,}] target[, target[..]]
				//   INPUT #filenumber, target[, [target[..]]

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

				var endTokenRef = new TokenRef();

				foreach (var targetTokens in SplitCommaDelimitedList(tokenHandler.RemainingTokens, endTokenRef))
				{
					if (targetTokens.Count == 0)
					{
						var range = targetTokens.Unwrap();

						if (range.Offset >= tokens.Count)
							range.Offset--;

						throw new SyntaxErrorException(tokens[range.Offset], "Expected: expression");
					}

					var target = ParseExpression(targetTokens, endTokenRef.Token ?? tokenHandler.EndToken);

					if (!target.IsValidAssignmentTarget())
						throw new SyntaxErrorException(targetTokens[0], "Cannot assign to this expression");

					input.Targets.Add(target);
				}

				return input;
			}

			case TokenType.KEY:
			{
				// One of:
				//   KEY n%, s$ => KeyConfigStatement
				//   KEY ON/OFF => KeyControlStatement
				//   KEY LIST => KeyListStatement
				//   KEY(n%) ON/OFF/STOP => EventControlStatement

				tokenHandler.ExpectMoreTokens();

				if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
				{
					var eventControl = new EventControlStatement();

					eventControl.EventType = EventType.Key;

					var sourceExpressionTokens = tokenHandler.ExpectParenthesizedTokens();

					eventControl.SourceExpression = ParseExpression(sourceExpressionTokens, tokenHandler.PreviousToken);

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
				else
				{
					Statement statement;

					switch (tokenHandler.NextToken.Type)
					{
						case TokenType.ON:
							statement = new SoftKeyControlStatement() { Enable = true };
							tokenHandler.Advance();
							break;
						case TokenType.OFF:
							statement = new SoftKeyControlStatement() { Enable = false };
							tokenHandler.Advance();
							break;
						case TokenType.LIST:
							statement = new SoftKeyListStatement();
							tokenHandler.Advance();
							break;
						default:
						{
							var argumentExpressions = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

							if (argumentExpressions.Count == 1)
								throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: ,");

							if (argumentExpressions.Count > 2)
							{
								var range = argumentExpressions[1].Unwrap();

								throw new SyntaxErrorException(tokens[range.Offset + range.Count], "Expected: )");
							}

							var midToken = tokens[argumentExpressions[1].Unwrap().Offset - 1];
							var endToken = tokenHandler.PreviousToken;

							var keyConfig = new SoftKeyConfigStatement();

							keyConfig.KeyExpression = ParseExpression(argumentExpressions[0], midToken);
							keyConfig.MacroExpression = ParseExpression(argumentExpressions[1], endToken);

							statement = keyConfig;

							tokenHandler.AdvanceToEnd();

							break;
						}
					}

					tokenHandler.ExpectEndOfTokens();

					return statement;
				}
			}

			case TokenType.LET:
			{
				var letStatement = new LetStatement();

				int separator = tokenHandler.FindNextUnparenthesizedOf(TokenType.Equals);

				if (separator < 0)
					throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: =");

				var midToken = tokens[separator];

				letStatement.TargetExpression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, separator), midToken);

				tokenHandler.Advance(separator);
				tokenHandler.Expect(TokenType.Equals);

				letStatement.ValueExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				return letStatement;
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
					if (tokenHandler.HasMoreTokens)
					{
						if (!tokenHandler.NextTokenIs(TokenType.Comma))
							throw new SyntaxErrorException(tokenHandler[0], "Expected: ,");
						else
						{
							tokenHandler.Advance();
							tokenHandler.ExpectMoreTokens();

							var options = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

							var colourEndToken = options.Count > 1
								? tokens[options[1].Unwrap().Offset - 1]
								: tokenHandler.EndToken;

							if (options[0].Any())
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
					}

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

				if (arguments[0].Count > 0)
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

				if ((locate.RowExpression == null)
				 && (locate.ColumnExpression == null)
				 && (locate.CursorVisibilityExpression == null)
				 && (locate.CursorStartExpression == null)
				 && (locate.CursorEndExpression == null))
					throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: expression");

				return locate;
			}

			case TokenType.LOCK:
			case TokenType.UNLOCK:
			{
				FileByteRangeStatement statement;

				switch (token.Type)
				{
					case TokenType.LOCK: statement = new LockStatement(); break;
					case TokenType.UNLOCK: statement = new UnlockStatement(); break;

					default: throw new Exception("Internal error");
				}

				if (tokenHandler.NextTokenIs(TokenType.NumberSign))
					tokenHandler.Advance();

				int comma = tokenHandler.FindNextUnparenthesizedOf(TokenType.Comma);

				var fileNumberTokens = tokenHandler.RemainingTokens;

				var midToken = tokenHandler.EndToken;

				if (comma > 0)
				{
					midToken = fileNumberTokens[comma];
					fileNumberTokens = fileNumberTokens.Slice(0, comma);
				}

				statement.FileNumberExpression = ParseExpression(fileNumberTokens, midToken);

				tokenHandler.Advance(fileNumberTokens.Count);

				if (tokenHandler.HasMoreTokens)
				{
					tokenHandler.Expect(TokenType.Comma);

					var bounds = SplitDelimitedList(tokenHandler.RemainingTokens, TokenType.TO).ToList();

					if (bounds.Count == 1)
						statement.RangeStartExpression = ParseExpression(bounds[0], tokenHandler.EndToken);
					else if (bounds.Count > 2)
					{
						var blame = tokens[bounds[2].Unwrap().Offset - 1];

						throw new SyntaxErrorException(blame, "Expected: end of statement");
					}
					else
					{
						midToken = tokens[bounds[1].Unwrap().Offset - 1];

						statement.RangeStartExpression = ParseExpression(bounds[0], midToken);
						statement.RangeEndExpression = ParseExpression(bounds[1], tokenHandler.EndToken);
					}
				}

				return statement;
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
				if (tokenHandler.NextTokenIs(TokenType.ERROR) || tokenHandler.NextTokenIs(TokenType.LOCAL))
				{
					var onError = new OnErrorStatement();

					if (tokenHandler.NextTokenIs(TokenType.LOCAL))
					{
						onError.LocalErrorsOnly = true;
						tokenHandler.Advance();
					}

					tokenHandler.Expect(TokenType.ERROR);

					if (tokenHandler.NextTokenIs(TokenType.GOTO))
					{
						onError.Action = OnErrorAction.GoToHandler;

						tokenHandler.Expect(TokenType.GOTO);
						tokenHandler.ExpectMoreTokens();

						switch (tokenHandler.NextToken.Type)
						{
							case TokenType.Number:
								if (int.TryParse(tokenHandler.NextToken.Value, out var parsedLineNumber)
								 && (parsedLineNumber == 0))
									onError.Action = OnErrorAction.DoNotHandle;
								else
									onError.TargetLineNumber = tokenHandler.NextToken.Value;

								break;

							case TokenType.Identifier:
								string labelName = tokenHandler.NextToken.Value ?? throw new Exception("Internal error: Identifier token with no value");

								if (labelName.Length == 0)
									throw new Exception("Internal error: Identifier token with empty string");

								if (char.IsSymbol(labelName.Last()))
									throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: label");

								onError.TargetLabel = labelName;

								break;
						}
					}
					else if (tokenHandler.NextTokenIs(TokenType.RESUME))
					{
						tokenHandler.Expect(TokenType.RESUME);
						tokenHandler.Expect(TokenType.NEXT);
						tokenHandler.ExpectEndOfTokens();

						onError.Action = OnErrorAction.ResumeNext;
					}
					else
						throw new SyntaxErrorException(tokenHandler.NextToken ?? tokenHandler.EndToken, "Expected: GOTO or RESUME");

					return onError;
				}
				else
				{
					var onEvent = new OnEventStatement();

					switch (tokenHandler.NextToken.Type)
					{
						case TokenType.COM: onEvent.EventType = EventType.Com; break;
						case TokenType.KEY: onEvent.EventType = EventType.Key; break;
						case TokenType.PEN: onEvent.EventType = EventType.Pen; break;
						case TokenType.PLAY: onEvent.EventType = EventType.Play; break;
						case TokenType.SIGNAL: onEvent.EventType = EventType.OS2Signal; break;
						case TokenType.STRIG: onEvent.EventType = EventType.JoystickTrigger; break;
						case TokenType.TIMER: onEvent.EventType = EventType.Timer; break;
						case TokenType.UEVENT: onEvent.EventType = EventType.UserDefinedEvent; break;

						default: throw new SyntaxErrorException(tokenHandler.NextToken, "Syntax error");
					}

					tokenHandler.Advance();

					var action = ParseStatement(tokenHandler.RemainingTokens, ignoreErrors);

					if (action is GoSubStatement goSubAction)
						onEvent.Action = goSubAction;
					else
						throw new SyntaxErrorException(tokens[2], "Expected: GOSUB");

					return onEvent;
				}
			}

			case TokenType.OPEN:
			{
				var commaIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.Comma);

				if (commaIndex < 0)
				{
					// New syntax
					var open = new OpenStatement();

					var parts = SplitDelimitedList(tokenHandler.RemainingTokens, TokenType.AS).ToList();

					if (parts.Count < 2)
						throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: AS");

					if (parts.Count > 2)
					{
						var range = parts[2].Unwrap();

						throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
					}

					// Now we have:
					//  parts[0]     filename$ [FOR mode] [ACCESS access] [lock]
					//  parts[1]     [#]filenumber% [LEN=reclen%]

					var filenamePart = parts[0];
					var fileNumberPart = parts[1];

					if (filenamePart.Count == 0)
					{
						var range = parts[1].Unwrap();

						throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: expression");
					}

					var filenameEndToken = tokens[fileNumberPart.Unwrap().Offset - 1];

					// Peel [lock] off the end.
					int lockIndex = filenamePart.Index()
						.FirstOrDefault<(int Index, Token Token)>(
							t => t.Token.Type == TokenType.LOCK, (-1, tokens[0]))
						.Index;

					Token? lockToken = null;

					if (lockIndex >= 0)
					{
						lockToken = filenamePart[lockIndex];

						var lockArgument = filenamePart.Slice(lockIndex + 1);

						if (lockArgument.Count == 0)
						{
							var midToken = tokens[parts[1].Unwrap().Offset - 1];

							throw new SyntaxErrorException(midToken, "Expected: READ or WRITE");
						}

						if (lockArgument.Count == 1)
						{
							if (lockArgument[0].Type == TokenType.READ)
								open.LockMode = LockMode.LockRead;
							else if (lockArgument[0].Type == TokenType.WRITE)
								open.LockMode = LockMode.LockWrite;
							else
								throw new SyntaxErrorException(lockArgument[0], "Expected: READ or WRITE");
						}

						if (lockArgument.Count >= 2)
						{
							if (lockArgument[0].Type == TokenType.READ)
							{
								if (lockArgument[1].Type == TokenType.WRITE)
									open.LockMode = LockMode.LockReadWrite;
								else
									throw new SyntaxErrorException(lockArgument[1], "Expected: AS");
							}
							else if (lockArgument[0].Type == TokenType.WRITE)
								throw new SyntaxErrorException(lockArgument[1], "Expected: AS");
							else
								throw new SyntaxErrorException(lockArgument[0], "Expected: READ or WRITE");

							if (lockArgument.Count > 2)
								throw new SyntaxErrorException(lockArgument[2], "Expected: AS");
						}

						filenameEndToken = filenamePart[lockIndex];
						filenamePart = filenamePart.Slice(0, lockIndex);
					}

					if (filenamePart.Last().Type == TokenType.SHARED)
					{
						if (lockToken != null)
							throw new SyntaxErrorException(lockToken, "Expected: AS");

						open.LockMode = LockMode.Shared;
						filenameEndToken = filenamePart[filenamePart.Count - 1];
						filenamePart = filenamePart.Slice(0, filenamePart.Count - 1);
					}

					// Peel [ACCESS access] off the end.
					int accessIndex = filenamePart.Index()
						.FirstOrDefault<(int Index, Token Token)>(
							t => t.Token.Type == TokenType.ACCESS, (-1, tokens[0]))
						.Index;

					if (accessIndex >= 0)
					{
						var accessArgument = filenamePart.Slice(accessIndex + 1);

						if (accessArgument.Count == 0)
						{
							var midToken = tokens[parts[1].Unwrap().Offset - 1];

							throw new SyntaxErrorException(midToken, "Expected: READ or WRITE");
						}

						if (accessArgument.Count == 1)
						{
							if (accessArgument[0].Type == TokenType.READ)
								open.AccessMode = AccessMode.Read;
							else if (accessArgument[0].Type == TokenType.WRITE)
								open.AccessMode = AccessMode.Write;
							else
								throw new SyntaxErrorException(accessArgument[0], "Expected: READ or WRITE");
						}

						if (accessArgument.Count >= 2)
						{
							if (accessArgument[0].Type == TokenType.READ)
							{
								if (accessArgument[1].Type == TokenType.WRITE)
									open.AccessMode = AccessMode.ReadWrite;
								else
									throw new SyntaxErrorException(accessArgument[1], "Expected: AS");
							}
							else if (accessArgument[0].Type == TokenType.WRITE)
								throw new SyntaxErrorException(accessArgument[1], "Expected: AS");
							else
								throw new SyntaxErrorException(accessArgument[0], "Expected: READ or WRITE");

							if (accessArgument.Count > 2)
								throw new SyntaxErrorException(accessArgument[2], "Expected: AS");
						}

						filenameEndToken = filenamePart[accessIndex];
						filenamePart = filenamePart.Slice(0, accessIndex);
					}

					// Peel [FOR mode] off the end.
					int forIndex = filenamePart.Index()
						.FirstOrDefault<(int Index, Token Token)>(
							t => t.Token.Type == TokenType.FOR, (-1, tokens[0]))
						.Index;

					if (forIndex >= 0)
					{
						var forArgument = filenamePart.Slice(forIndex + 1);

						var openMode = OpenMode.Invalid;

						if (forArgument.Count >= 1)
						{
							switch (forArgument[0].Type)
							{
								case TokenType.RANDOM: openMode = OpenMode.Random; break;
								case TokenType.BINARY: openMode = OpenMode.Binary; break;
								case TokenType.INPUT: openMode = OpenMode.Input; break;
								case TokenType.OUTPUT: openMode = OpenMode.Output; break;
								case TokenType.APPEND: openMode = OpenMode.Append; break;
							}
						}

						if (openMode == OpenMode.Invalid)
						{
							var blame =
								forArgument.Any()
								? forArgument[0]
								: tokens[fileNumberPart.Unwrap().Offset - 1];

							throw new SyntaxErrorException(blame, "Expected: RANDOM, BINARY, INPUT, OUTPUT or APPEND");
						}

						open.OpenMode = openMode;

						filenameEndToken = filenamePart[forIndex];
						filenamePart = filenamePart.Slice(0, forIndex);
					}

					open.FileNameExpression = ParseExpression(filenamePart, filenameEndToken);

					// Part 2: (AS) [#]filenumber% [LEN=reclen%]
					int lenIndex = fileNumberPart.Index()
						.FirstOrDefault<(int Index, Token Token)>(
							t => t.Token.Type == TokenType.LEN, (-1, tokens[0]))
						.Index;

					var fileNumberEndToken = tokenHandler.EndToken;

					if (lenIndex >= 0)
					{
						var lenArgs = fileNumberPart.Slice(lenIndex + 1);

						if ((lenArgs.Count == 0) || (lenArgs[0].Type != TokenType.Equals))
						{
							var blame = lenArgs.Count > 0
								? lenArgs[0]
								: tokenHandler.EndToken;

							throw new SyntaxErrorException(blame, "Expected: =");
						}

						lenArgs = lenArgs.Slice(1);

						open.RecordLengthExpression = ParseExpression(lenArgs, tokenHandler.EndToken);

						fileNumberEndToken = fileNumberPart[lenIndex];
						fileNumberPart = fileNumberPart.Slice(0, lenIndex);
					}

					if (fileNumberPart.Count == 0)
						throw new SyntaxErrorException(fileNumberEndToken, "Expected: expression");

					if (fileNumberPart[0].Type == TokenType.NumberSign)
						fileNumberPart = fileNumberPart.Slice(1);

					open.FileNumberExpression = ParseExpression(fileNumberPart, fileNumberEndToken);

					return open;
				}
				else
				{
					// Old syntax

					var open = new LegacyOpenStatement();

					var parts = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

					if (parts.Count < 3)
						throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: ,");

					if (parts.Count > 4)
					{
						var blame = tokens[parts[4].Unwrap().Offset - 1];

						throw new SyntaxErrorException(blame, "Expected: end of statement");
					}

					var midToken = tokens[parts[1].Unwrap().Offset - 1];

					open.ModeExpression = ParseExpression(parts[0], midToken);

					if ((parts[1].Count > 0) && (parts[1][0].Type == TokenType.NumberSign))
						parts[1] = parts[1].Slice(1);

					midToken = tokens[parts[2].Unwrap().Offset - 1];

					open.FileNumberExpression = ParseExpression(parts[1], midToken);

					midToken = parts.Count > 3
						? tokens[parts[3].Unwrap().Offset - 1]
						: tokenHandler.EndToken;

					open.FileNameExpression = ParseExpression(parts[2], midToken);

					if (parts.Count == 4)
						open.RecordLengthExpression = ParseExpression(parts[3], tokenHandler.EndToken);

					return open;
				}
			}

			case TokenType.PAINT:
			{
				var paint = new PaintStatement();

				if (tokenHandler.NextTokenIs(TokenType.STEP))
				{
					paint.Step = true;
					tokenHandler.Advance();
				}

				var coordinateTokens = tokenHandler.ExpectParenthesizedTokens();

				var coordinates = SplitCommaDelimitedList(coordinateTokens).ToList();

				if (coordinates.Count == 1)
					throw new SyntaxErrorException(tokenHandler.PreviousToken, "Expected: expression");
				if (coordinates.Count > 2)
				{
					var blame = tokens[coordinates[2].Unwrap().Offset - 1];

					throw new SyntaxErrorException(blame, "Expected: )");
				}

				var midToken = tokens[coordinates[1].Unwrap().Offset - 1];

				paint.XExpression = ParseExpression(coordinates[0], midToken);
				paint.YExpression = ParseExpression(coordinates[1], tokenHandler.PreviousToken);

				if (tokenHandler.HasMoreTokens)
				{
					tokenHandler.Expect(TokenType.Comma);

					if (!tokenHandler.HasMoreTokens)
						throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: expression");

					var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

					if (!arguments.Last().Any())
						throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: expression");

					if (arguments[0].Any())
					{
						midToken = arguments.Count > 1
							? tokens[arguments[1].Unwrap().Offset - 1]
							: tokenHandler.EndToken;

						paint.PaintExpression = ParseExpression(arguments[0], midToken);
					}

					if (arguments.Count > 1)
					{
						if (arguments[1].Any())
						{
							midToken = arguments.Count > 2
								? tokens[arguments[2].Unwrap().Offset - 1]
								: tokenHandler.EndToken;

							paint.BorderColourExpression = ParseExpression(arguments[1], midToken);
						}

						if (arguments.Count > 2)
						{
							if (arguments[2].Any())
							{
								midToken = arguments.Count > 3
									? tokens[arguments[3].Unwrap().Offset - 1]
									: tokenHandler.EndToken;

								paint.BackgroundExpression = ParseExpression(arguments[2], midToken);
							}

							if (arguments.Count > 3)
							{
								var blame = tokens[arguments[3].Unwrap().Offset - 1];

								throw new SyntaxErrorException(blame, "Expected: end of statement");
							}
						}
					}
				}

				return paint;
			}

			case TokenType.PALETTE:
			{
				var palette = new PaletteStatement();

				if (tokenHandler.NextTokenIs(TokenType.USING))
				{
					tokenHandler.Advance();

					palette.ArrayExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);
				}
				else if (tokenHandler.HasMoreTokens)
				{
					var endTokenRef = new TokenRef();

					var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens, endTokenRef);

					var enumerator = arguments.GetEnumerator();

					enumerator.MoveNext();

					palette.AttributeExpression = ParseExpression(enumerator.Current, endTokenRef.Token ?? tokenHandler.EndToken);

					if (!enumerator.MoveNext())
						throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: ,");

					var endToken = endTokenRef.Token ?? tokenHandler.EndToken;

					palette.ColourExpression = ParseExpression(enumerator.Current, endToken);

					if (enumerator.MoveNext())
						throw new SyntaxErrorException(endToken, "Expected: end of statement");
				}

				return palette;
			}

			case TokenType.PCOPY:
			{
				var pcopy = new PageCopyStatement();

				tokenHandler.ExpectMoreTokens();

				var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

				if (arguments.Count == 1)
					throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: ,");

				if (arguments.Count > 2)
				{
					var range = arguments[2].Unwrap();

					throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
				}

				var midToken = tokens[arguments[1].Unwrap().Offset - 1];

				pcopy.SourcePageExpression = ParseExpression(arguments[0], midToken);
				pcopy.DestinationPageExpression = ParseExpression(arguments[1], tokenHandler.EndToken);

				return pcopy;
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

							throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: ;");
						}
						else
							throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: ;");
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

						ListRange<Token> expressionTokens = tokenHandler.RemainingTokens.Slice(0, nextSeparatorIndex);

						if (nextSeparatorIndex > 0)
						{
							bool isTab = tokenHandler.NextTokenIs(TokenType.TAB);
							bool isSpace = tokenHandler.NextTokenIs(TokenType.SPC);

							if (isTab || isSpace)
							{
								if (isTab)
									arg.ExpressionType = PrintExpressionType.Tab;
								if (isSpace)
									arg.ExpressionType = PrintExpressionType.Space;

								var separatorToken = tokenHandler[nextSeparatorIndex];

								tokenHandler.Advance();

								expressionTokens = tokenHandler.ExpectParenthesizedTokens();

								nextSeparatorIndex = 0;
							}

							arg.Expression = ParseExpression(expressionTokens, tokenHandler[nextSeparatorIndex]);

							tokenHandler.Advance(nextSeparatorIndex);
						}

						if (arg.ExpressionType != PrintExpressionType.Value)
						{
							arg.CursorAction = PrintCursorAction.None;

							if (tokenHandler.NextTokenIs(TokenType.Semicolon))
								tokenHandler.Advance();
						}
						else
						{
							switch (tokenHandler.NextToken.Type)
							{
								case TokenType.Semicolon:
									arg.CursorAction = PrintCursorAction.None;
									tokenHandler.Advance();
									break;
								case TokenType.Comma:
									arg.CursorAction = PrintCursorAction.NextZone;
									tokenHandler.Advance();
									break;
							}
						}

						print.Arguments.Add(arg);
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
					randomize.ArgumentExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				return randomize;
			}

			case TokenType.READ:
			{
				var read = new ReadStatement();

				tokenHandler.ExpectMoreTokens();

				var endTokenRef = new TokenRef();

				foreach (var target in SplitCommaDelimitedList(tokenHandler.RemainingTokens, endTokenRef))
				{
					if (target.Count == 0)
					{
						var range = target.Unwrap();

						if (range.Offset >= tokens.Count)
							range.Offset--;

						throw new SyntaxErrorException(tokens[range.Offset], "Expected: expression");
					}

					var targetExpression = ParseExpression(target, endTokenRef.Token ?? tokenHandler.EndToken);

					if (!targetExpression.IsValidAssignmentTarget())
						throw new SyntaxErrorException(target[0], "Expected: valid assignment target");

					read.Targets.Add(targetExpression);
				}

				return read;
			}

			case TokenType.RESET:
			{
				tokenHandler.ExpectEndOfTokens();

				return new ResetStatement();
			}

			case TokenType.RESUME:
			{
				var resume = new ResumeStatement();

				if (tokenHandler.HasMoreTokens)
				{
					switch (tokenHandler.NextToken.Type)
					{
						case TokenType.NEXT:
							resume.NextStatement = true;
							break;

						case TokenType.Number:
							resume.TargetLineNumber = tokenHandler.NextToken.Value;
							break;

						case TokenType.Identifier:
							string labelName = tokenHandler.NextToken.Value ?? throw new Exception("Internal error: Identifier token with no value");

							if (labelName.Length == 0)
								throw new Exception("Internal error: Identifier token with empty string");

							if (char.IsSymbol(labelName.Last()))
								throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: label");

							resume.TargetLabel = labelName;

							break;
					}

					tokenHandler.Advance();

					tokenHandler.ExpectEndOfTokens();
				}

				return resume;
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
				if (isNested)
					throw new SyntaxErrorException(token, $"{token.Type} may not be nested");

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

						if (declarationHandler.NextTokenIs(TokenType.Identifier))
							declaration.UserType = declarationHandler.ExpectIdentifier(allowTypeCharacter: false);
						else
						{
							if (!declarationHandler.NextToken.IsDataType)
								throw new SyntaxErrorException(declarationHandler.NextToken, "Expected data type");

							declaration.Type = DataTypeConverter.FromToken(declarationHandler.NextToken);
							//declaration.ActualName = declaration.Name + new TypeCharacter(declaration.Type.Value).Character;

							declarationHandler.Advance();
						}
					}

					declarationHandler.ExpectEndOfTokens();

					scopeStatement.Declarations.Add(declaration);
				}

				return scopeStatement;
			}

			case TokenType.SOUND:
			{
				var sound = new SoundStatement();

				var arguments = SplitCommaDelimitedList(tokenHandler.RemainingTokens).ToList();

				if (arguments.Count == 1)
					throw new SyntaxErrorException(tokenHandler.PreviousToken, "Expected: expression");
				if (arguments.Count > 2)
				{
					var blame = tokens[arguments[2].Unwrap().Offset - 1];

					throw new SyntaxErrorException(blame, "Expected: end of statement");
				}

				var midToken = tokens[arguments[1].Unwrap().Offset - 1];

				sound.FrequencyExpression = ParseExpression(arguments[0], midToken);
				sound.DurationExpression = ParseExpression(arguments[1], tokenHandler.PreviousToken);

				return sound;
			}

			case TokenType.SUB:
			case TokenType.FUNCTION:
			{
				if (isNested)
					throw new SyntaxErrorException(token, $"{token.Type} may not be nested");

				var statement =
					token.Type switch
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

				return type;
			}

			case TokenType.VIEW:
			{
				if (tokenHandler.NextTokenIs(TokenType.PRINT))
				{
					var viewport = new TextViewportStatement();

					tokenHandler.Advance();

					if (tokenHandler.HasMoreTokens)
					{
						var arguments = SplitDelimitedList(tokenHandler.RemainingTokens, TokenType.TO).ToList();

						if (arguments.Count < 2)
							throw new SyntaxErrorException(tokenHandler.EndToken, "Expected: TO");

						if (arguments.Count > 2)
						{
							var range = arguments[2].Unwrap();

							throw new SyntaxErrorException(tokens[range.Offset - 1], "Expected: end of statement");
						}

						var midToken = tokens[arguments[1].Unwrap().Offset - 1];

						viewport.TopExpression = ParseExpression(arguments[0], midToken);
						viewport.BottomExpression = ParseExpression(arguments[1], tokenHandler.EndToken);
					}

					return viewport;
				}
				else
				{
					var view = new GraphicsViewportStatement();

					if (tokenHandler.HasMoreTokens)
					{
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
					}

					return view;
				}
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

				if ((arguments.Count == 1)
				 || ((arguments.Count == 2) && (arguments[0].Count == 0)))
				{
					var width = new ScreenWidthStatement();

					var screenWidthMidToken = arguments.Count == 1
						? tokenHandler.EndToken
						: tokens[arguments[1].Unwrap().Offset - 1];

					if (arguments[0].Any())
						width.WidthExpression = ParseExpression(arguments[0], screenWidthMidToken);
					if (arguments.Count > 1)
						width.HeightExpression = ParseExpression(arguments[1], tokenHandler.EndToken);

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
		//   identifier AS type
		//   array(subscripts) AS type
		//   assignmenttarget = value

		tokenHandler.Reset();

		int equalsSign = tokenHandler.FindNextUnparenthesizedOf(TokenType.Equals);

		if (equalsSign > 0)
		{
			Expression? targetExpression = null;

			try
			{
				targetExpression = ParseExpression(tokenHandler.RemainingTokens.Slice(0, equalsSign), tokenHandler[equalsSign]);
			}
			catch (SyntaxErrorException) { }

			if ((targetExpression != null) && targetExpression.IsValidAssignmentTarget())
			{
				var assignment = new AssignmentStatement();

				assignment.TargetExpression = targetExpression;

				tokenHandler.Advance(equalsSign);
				tokenHandler.Expect(TokenType.Equals);

				assignment.ValueExpression = ParseExpression(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				return assignment;
			}
		}
		else
		{
			tokenHandler.Reset();

			int asTokenIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.AS);

			if (asTokenIndex >= 0)
			{
				var typeElement = new TypeElementStatement();

				typeElement.Name = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);

				if (tokenHandler.NextTokenIs(TokenType.OpenParenthesis))
				{
					typeElement.Subscripts = new VariableDeclarationSubscriptList();

					var subscriptTokens = tokenHandler.ExpectParenthesizedTokens();

					var endTokenRef = new TokenRef();

					foreach (var subscript in SplitCommaDelimitedList(subscriptTokens, endTokenRef))
					{
						typeElement.Subscripts.Add(ParseVariableDeclarationSubscript(
							subscript,
							endTokenRef.Token ??
							(tokenHandler.HasMoreTokens ? tokenHandler.NextToken : tokenHandler.EndToken)));
					}

					if (!typeElement.Subscripts.Any())
						throw new SyntaxErrorException(tokenHandler.PreviousToken, "Expected: expression");
				}

				var asToken = tokenHandler.Expect(TokenType.AS);

				typeElement.AlignmentWhitespace = asToken.PrecedingWhitespace;

				switch (tokenHandler.NextToken.Type)
				{
					case TokenType.INTEGER:
					case TokenType.LONG:
					case TokenType.SINGLE:
					case TokenType.DOUBLE:
					case TokenType.STRING:
					case TokenType.CURRENCY:
					{
						switch (tokenHandler.NextToken.Type)
						{
							case TokenType.INTEGER: typeElement.ElementType = DataType.INTEGER; break;
							case TokenType.LONG: typeElement.ElementType = DataType.LONG; break;
							case TokenType.SINGLE: typeElement.ElementType = DataType.SINGLE; break;
							case TokenType.DOUBLE: typeElement.ElementType = DataType.DOUBLE; break;
							case TokenType.STRING: typeElement.ElementType = DataType.STRING; break;
							case TokenType.CURRENCY: typeElement.ElementType = DataType.CURRENCY; break;
						}

						tokenHandler.Advance();

						if (typeElement.ElementType == DataType.STRING)
						{
							tokenHandler.Expect(TokenType.Asterisk);

							var fixedStringLength = tokenHandler.Expect(TokenType.Number);

							if (fixedStringLength.Value.StartsWith("-"))
							{
								throw new SyntaxErrorException(
									new Token(
										fixedStringLength.Line,
										fixedStringLength.Column,
										TokenType.Minus,
										"-"),
									"Syntax error");
							}

							if (!NumberParser.TryAsInteger(fixedStringLength.Value, out var fixedStringLengthValue))
								throw new SyntaxErrorException(fixedStringLength, "Invalid constant");

							typeElement.FixedStringLength = fixedStringLengthValue;
						}

						break;
					}

					default:
						typeElement.ElementUserType = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);
						break;
				}

				tokenHandler.ExpectEndOfTokens();

				return typeElement;
			}
			else
			{
				var targetName = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);

				ExpressionList? arguments = null;

				if (tokenHandler.HasMoreTokens)
					arguments = ParseExpressionList(tokenHandler.RemainingTokens, tokenHandler.EndToken);

				return new CallStatement(CallStatementType.Implicit, targetName, arguments);
			}
		}

		throw new SyntaxErrorException(tokens[0], "Syntax error");
	}

	private FieldDefinition ParseFieldDefinition(ListRange<Token> argument, Token endToken)
	{
		var tokenHandler = new TokenHandler(argument);

		var midTokenIndex = tokenHandler.FindNextUnparenthesizedOf(TokenType.AS);

		if (midTokenIndex < 0)
			throw new SyntaxErrorException(endToken, "Expected: AS");

		var fieldWidthExpression = ParseExpression(
			tokenHandler.RemainingTokens.Slice(0, midTokenIndex),
			tokenHandler[midTokenIndex]);

		var targetExpression = ParseExpression(
			tokenHandler.RemainingTokens.Slice(midTokenIndex + 1),
			endToken);

		return new FieldDefinition(fieldWidthExpression, targetExpression);
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

	VariableDeclaration ParseVariableDeclaration(ListRange<Token> tokens, Token endToken, bool requireSubscripts)
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
		else if (requireSubscripts)
		{
			throw new SyntaxErrorException(
				tokenHandler.HasMoreTokens ? tokenHandler.NextToken : tokenHandler.EndToken,
				"Expected: (");
		}

		if (tokenHandler.NextTokenIs(TokenType.AS))
		{
			tokenHandler.Advance();

			if (tokenHandler.NextTokenIs(TokenType.Identifier))
				declaration.UserType = tokenHandler.ExpectIdentifier(allowTypeCharacter: false);
			else
			{
				if (!tokenHandler.NextToken.IsDataType)
					throw new SyntaxErrorException(tokenHandler.NextToken, "Expected data type");

				declaration.Type = DataTypeConverter.FromToken(tokenHandler.NextToken);
				//declaration.ActualName = declaration.Name + new TypeCharacter(declaration.Type.Value).Character;

				tokenHandler.Advance();
			}
		}

		tokenHandler.ExpectEndOfTokens();

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

		switch (boundExpressions.Count)
		{
			case 1:
			{
				subscript.Bound1 = ParseExpression(boundExpressions[0], endToken);

				break;
			}
			case 2:
			{
				var bound2Range = boundExpressions[1].Unwrap();

				var midToken = bound2Range.List[bound2Range.Offset - 1];

				subscript.Bound1 = ParseExpression(boundExpressions[0], midToken);
				subscript.Bound2 = ParseExpression(boundExpressions[1], endToken);

				break;
			}
		}

		return subscript;
	}

	ParameterList ParseParameterList(ListRange<Token> tokens, bool allowByVal = true, bool allowArray = true)
	{
		var list = new ParameterList();

		if (tokens.Any())
		{
			foreach (var range in SplitCommaDelimitedList(tokens))
				list.Parameters.Add(ParseParameterDefinition(range, allowByVal, allowArray));
		}

		return list;
	}

	ParameterDefinition ParseParameterDefinition(ListRange<Token> tokens, bool allowByVal, bool allowArray)
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

			var nameToken = tokens[tokenIndex];

			if (nameToken.Type != TokenType.Identifier)
				throw new SyntaxErrorException(nameToken, "Expected identifier");

			param.Name = nameToken.Value ?? throw new Exception("Internal error: identifier token with no value");
			param.NameToken = nameToken;

			tokenIndex++;

			char lastChar = param.Name.Last();

			if (TypeCharacter.TryParse(lastChar, out var typeCharacter))
			{
				param.ActualName = param.Name;
				param.TypeToken = nameToken;
			}

			if (tokenIndex < tokens.Count)
			{
				if (tokens[tokenIndex].Type == TokenType.OpenParenthesis)
				{
					if (!allowArray)
						throw new SyntaxErrorException(tokens[tokenIndex], "Expected: , or )");

					if (tokens[tokenIndex + 1].Type != TokenType.CloseParenthesis)
						throw new SyntaxErrorException(tokens[tokenIndex + 1], "Expected: )");

					param.IsArray = true;

					tokenIndex += 2;
				}

				if (tokenIndex < tokens.Count)
				{
					if (tokens[tokenIndex].Type != TokenType.AS)
						throw new SyntaxErrorException(tokens[tokenIndex], "Expected AS");

					if (param.TypeToken != null)
					{
						// Specifying the type both with a type character and an AS clause.
						throw new SyntaxErrorException(param.TypeToken, "Identifier cannot end with %, &, !, #, $, or @");
					}

					tokenIndex++;

					var typeToken = tokens[tokenIndex];

					if (typeToken.Type == TokenType.ANY)
						param.AnyType = true;
					else if (typeToken.Type == TokenType.Identifier)
					{
						param.UserType = typeToken.Value;

						if (!char.IsAsciiLetterOrDigit(param.UserType!.Last()))
							throw new SyntaxErrorException(typeToken, "Type name may only contain letters and digits");
					}
					else
					{
						if (!typeToken.IsDataType)
							throw new SyntaxErrorException(tokens[tokenIndex], "Expected data type");

						param.Type = DataTypeConverter.FromToken(typeToken);
						param.ActualName = param.Name + new TypeCharacter(param.Type).Character;
					}

					param.TypeToken = typeToken;

					tokenIndex++;
				}
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

	CaseExpressionList ParseCaseExpressionList(ListRange<Token> tokens, Token endToken)
	{
		var list = new CaseExpressionList();

		var endTokenRef = new TokenRef();

		foreach (var range in SplitCommaDelimitedList(tokens, endTokenRef))
			list.Expressions.Add(ParseCaseExpression(range, endTokenRef.Token ?? endToken));

		return list;
	}

	CaseExpression ParseCaseExpression(ListRange<Token> tokens, Token endToken)
	{
		CaseExpression caseExpression = new CaseExpression();

		var tokenHandler = new TokenHandler(tokens);

		if (!tokenHandler.HasMoreTokens)
			throw new SyntaxErrorException(endToken, "Expected: case expression");

		if (tokenHandler.NextTokenIs(TokenType.IS))
		{
			tokenHandler.Advance();
			tokenHandler.ExpectMoreTokens();

			if (!IsOperator(tokenHandler.NextToken, out var op))
				throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: relational operator");

			caseExpression.RelationToExpression = (RelationalOperator)op;

			if (!Enum.IsDefined(caseExpression.RelationToExpression.Value))
				throw new SyntaxErrorException(tokenHandler.NextToken, "Expected: relational operator");

			tokenHandler.Advance();
		}

		var expressions = SplitDelimitedList(tokenHandler.RemainingTokens, TokenType.TO).ToList();

		if (expressions.Count > 2)
		{
			var range = expressions[2].Unwrap();

			throw new SyntaxErrorException(tokens[range.Offset - 1], "Syntax error");
		}

		var midToken = expressions.Count == 1
			? endToken
			: tokens[expressions[1].Unwrap().Offset - 1];

		caseExpression.Expression = ParseExpression(expressions[0], midToken);

		if (expressions.Count == 2)
			caseExpression.RangeEndExpression = ParseExpression(expressions[1], endToken);

		return caseExpression;
	}

	ExpressionList ParseExpressionList(ListRange<Token> tokens, Token endToken, int minCount = 0, int maxCount = int.MaxValue)
	{
		// TODO: file number parameters (allow preceding '#')

		var list = new ExpressionList();

		if (tokens.Count > 0)
		{
			var endTokenRef = new TokenRef();

			foreach (var range in SplitCommaDelimitedList(tokens, endTokenRef))
			{
				list.Expressions.Add(ParseExpression(range, endTokenRef.Token ?? endToken));

				if ((list.Expressions.Count == maxCount)
				 && (endTokenRef.Token != null))
					throw new SyntaxErrorException(endTokenRef.Token, "Expected: )");
			}

			if (list.Expressions.Count < minCount)
				throw new SyntaxErrorException(endToken, "Expected: ,");
		}

		return list;
	}

	internal Expression ParseExpression(ListRange<Token> tokens, Token endToken)
	{
		if (tokens.Count == 0)
			throw new SyntaxErrorException(endToken, "Expected: expression");

		int level = 0;

		bool tailParenthesized = (tokens.Count > 0) && (tokens.Last().Type == TokenType.CloseParenthesis);
		int openParenthesisIndex = -1;

		if (tailParenthesized)
		{
			for (int i = tokens.Count - 1; i >= 0; i--)
			{
				switch (tokens[i].Type)
				{
					case TokenType.CloseParenthesis: level++; break;
					case TokenType.OpenParenthesis: level--; break;
				}

				if (level == 0)
				{
					openParenthesisIndex = i;
					break;
				}
			}

			if (openParenthesisIndex > 0)
			{
				if ((openParenthesisIndex == 1)
				 && (tokens[0].KeywordFunctionAttribute is KeywordFunctionAttribute config))
				{
					var expressionList = ParseExpressionList(
						tokens.Slice(openParenthesisIndex + 1,
						tokens.Count - openParenthesisIndex - 2),
						endToken,
						config.MinimumParameterCount,
						config.MaximumParameterCount);

					// TODO: isAssignable

					return new KeywordFunctionExpression(tokens[0], expressionList);
				}

				Expression? subjectExpression = TestParseSubjectExpression(tokens, openParenthesisIndex);

				if ((subjectExpression != null) && subjectExpression.IsValidIndexSubject())
				{
					return new CallOrIndexExpression(
						tokens[openParenthesisIndex],
						subjectExpression,
						ParseExpressionList(tokens.Slice(openParenthesisIndex + 1, tokens.Count - openParenthesisIndex - 2), tokens.Last()));
				}
			}
		}

		int lastOperatorIndex = -1;
		int lastOperatorPrecedence = -1;

		level = 0;

		for (int i = 0; i < tokens.Count; i++)
		{
			if (level == 0)
			{
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
			var leftExpression = ParseExpression(tokens.Slice(0, lastOperatorIndex), tokens[lastOperatorIndex]);
			var rightExpression = ParseExpression(tokens.Slice(lastOperatorIndex + 1), endToken);

			if ((tokens[lastOperatorIndex].Type == TokenType.Period)
			 && !leftExpression.IsValidFieldSubject())
				throw new SyntaxErrorException(tokens[lastOperatorIndex + 1], "Expected: identifier");

			return new BinaryExpression(
				leftExpression,
				tokens[lastOperatorIndex],
				rightExpression);
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
						tokens[0].Value + tokens[1].Value,
						tokens[1].DataType));
			}

			// Unary expressions
			if (tokens[0].Type == TokenType.Plus)
				return ParseExpression(tokens.Slice(1), endToken);
			if ((tokens[0].Type == TokenType.Minus) || (tokens[0].Type == TokenType.NOT))
				return new UnaryExpression(tokens[0], ParseExpression(tokens.Slice(1), endToken));

			if (tokens.Count == 0)
				throw new SyntaxErrorException(endToken, "Expected: expression");
			if (lastOperatorIndex == 0)
				throw new SyntaxErrorException(tokens[0], "Expected: expression");

			if (tailParenthesized)
			{
				if (openParenthesisIndex == 0)
					return new ParenthesizedExpression(tokens[0], ParseExpression(tokens.Slice(1, tokens.Count - 2), tokens.Last()));

				// Function calls have already been taken care of.

				throw new SyntaxErrorException(tokens[0], "Expected: identifier");
			}

			if (tokens.Count == 1)
			{
				if ((tokens[0].Type == TokenType.Number) || (tokens[0].Type == TokenType.String))
					return new LiteralExpression(tokens[0]);
				if (tokens[0].Type == TokenType.Identifier)
					return new IdentifierExpression(tokens[0]);
				if (tokens[0].IsParameterlessKeywordFunction)
					return new KeywordFunctionExpression(tokens[0]);
			}

			// Try interpreting negative numbers as the '-' operator followed by positive numbers.
			List<Token>? reinterpretedTokens = null;

			for (int i = tokens.Count - 1; i >= 0; i--)
			{
				if ((tokens[i].Type == TokenType.Number) && tokens[i].Value.StartsWith("-"))
				{
					if (reinterpretedTokens == null)
					{
						reinterpretedTokens = [.. tokens, tokens[tokens.Count - 1]];

						if (i + 2 < tokens.Count)
						{
							var span = CollectionsMarshal.AsSpan(reinterpretedTokens);

							span.Slice(i + 1).CopyTo(span.Slice(i + 2));
						}
					}

					reinterpretedTokens[i] = new Token(
						tokens[i].Line,
						tokens[i].Column,
						TokenType.Minus,
						"-");

					reinterpretedTokens[i + 1] = new Token(
						tokens[i].Line,
						tokens[i].Column + 1,
						TokenType.Number,
						tokens[i].Value.Substring(1));

					var parsed = TestParseExpressionReinterpretNegativeNumber(reinterpretedTokens, endToken);

					if (parsed != null)
						return parsed;
				}
			}

			throw new SyntaxErrorException(tokens[0], "Expected: expression");
		}
	}

	private Expression? TestParseSubjectExpression(ListRange<Token> tokens, int openParenthesisIndex)
	{
		Expression? subjectExpression = null;

		try
		{
			subjectExpression = ParseExpression(tokens.Slice(0, openParenthesisIndex), tokens[openParenthesisIndex]);
		}
		catch (SyntaxErrorException) { }

		return subjectExpression;
	}

	private Expression? TestParseExpressionReinterpretNegativeNumber(ListRange<Token> reinterpretedTokens, Token endToken)
	{
		Expression? parsed = null;

		try
		{
			parsed = ParseExpression(reinterpretedTokens, endToken);
		}
		catch (SyntaxErrorException) { }

		return parsed;
	}

	private bool IsOperator(Token token, out Operator op)
	{
		switch (token.Type)
		{
			case TokenType.Period: op = Operator.Field; return true;

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
