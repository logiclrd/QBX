using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using System.Buffers.Binary;
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
			else
			{
				if (token.Type == TokenType.Colon)
				{
					line.Statements.Add(ParseStatement(buffer, colonAfter: true));
					haveContent = true;
					buffer.Clear();
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
		}


	/*
	case TokenType.DO,
	case TokenType.ELSE,
	case TokenType.END,
	case TokenType.FOR,
	case TokenType.FUNCTION,
	case TokenType.GOSUB,
	case TokenType.GOTO,
	case TokenType.IF,
	case TokenType.INPUT,
	case TokenType.LOCATE,
	case TokenType.LOOP,
	case TokenType.NEXT,
	case TokenType.PEEK,
	case TokenType.PLAY,
	case TokenType.POKE,
	case TokenType.PRINT,
	case TokenType.RANDOMIZE,
	case TokenType.READ,
	case TokenType.RESTORE,
	case TokenType.RETURN,
	case TokenType.SCREEN,
	case TokenType.SELECT,
	case TokenType.SHARED,
	case TokenType.STATIC,
	case TokenType.SUB,
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
