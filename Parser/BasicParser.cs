using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using System.Buffers.Binary;

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
					else if ((lastStatement.Type == StatementType.EndSub) || (lastStatement.Type == StatementType.EndFunction))
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

	Statement ParseStatement(IList<Token> tokens, bool colonAfter)
	{
		int tokenIndex = 1;

		bool NextTokenIs(TokenType type) => (tokenIndex < tokens.Count) && (tokens[tokenIndex].Type == type);

		string ExpectIdentifier(bool allowTypeCharacter)
		{
			var token = tokens[tokenIndex];

			if (token.Type != TokenType.Identifier)
				throw new SyntaxErrorException(token, "Expected identifier");

			string identifier = token.Value ?? "";

			if (identifier.Length == 0)
				throw new Exception("Internal error: Identifier token with no value");

			if (!allowTypeCharacter && char.IsSymbol(identifier.Last()))
				throw new SyntaxErrorException(token, "Cannot use a type character in this context");

			return identifier;
		}

		void ExpectEndOfStatement()
		{
			if (tokenIndex < tokens.Count)
				throw new SyntaxErrorException(tokens[tokenIndex], "Expected end of statement");
		}

		ListRange<Token> ExpectParenthesizedTokens()
		{
			if (!NextTokenIs(TokenType.OpenParenthesis))
				throw new SyntaxErrorException(tokens[tokenIndex], "Expected: (");

			int level = 1;

			tokenIndex++;

			int rangeStart = tokenIndex;

			while ((tokenIndex < tokens.Count) && (level > 0))
			{
				switch (tokens[tokenIndex].Type)
				{
					case TokenType.OpenParenthesis: level++; break;
					case TokenType.CloseParenthesis: level--; break;
				}

				tokenIndex++;
			}

			if (level > 0)
				throw new SyntaxErrorException(tokens.Last(), "Expected: )");

			int rangeEnd = tokenIndex - 1;

			return tokens.Slice(rangeStart, rangeEnd - rangeStart);
		}

		switch (tokens[0].Type)
		{
			case TokenType.Comment:
			{
				if (tokens.Count > 1)
					throw new Exception("Internal error: Additional tokens between Comment and Newline");

				string commentText = tokens[0].Value ?? "";

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
				string targetName = ExpectIdentifier(allowTypeCharacter: false);

				ExpressionList? arguments = null;

				if (NextTokenIs(TokenType.OpenParenthesis))
				{
					var argumentTokens = ExpectParenthesizedTokens();

					arguments = ParseExpressionList(argumentTokens);
				}

				ExpectEndOfStatement();

				return new CallStatement(CallStatementType.Explicit, targetName, arguments);
			}

			case TokenType.CASE:
			{
				var expressions = ParseExpressionList(tokens.Slice(1));

				return new CaseStatement(expressions);
			}

			case TokenType.CLS:
			{
				if (tokens.Count > 1)
				{
					var mode = ParseExpression(tokens.Slice(1));

					return new ClsStatement(mode);
				}
				else
					return new ClsStatement();
			}

			case TokenType.COLOR:
			{
				if (tokens.Count == 1)
					return new ColorStatement(); // this is a runtime error but should parse
				else
				{
					var arguments = ParseExpressionList(tokens.Slice(1));

					if (arguments.Expressions.Count > 3)
						throw new SyntaxErrorException(tokens[0], "Expected no more than 3 arguments");

					return new ColorStatement(arguments);
				}
			}

			case TokenType.CONST:
			{
				var declarationSyntax = ParseExpressionList(tokens.Slice(1));

				var declarations = new List<ConstDeclaration>();

				for (int i = 0; i < declarationSyntax.Expressions.Count; i++)
				{
					var syntax = declarationSyntax.Expressions[i];

					if ((syntax is not BinaryExpression binarySyntax)
					 || (binarySyntax.Operator != Operator.Equals)
					 || (binarySyntax.Left is not IdentifierExpression identifierSyntax))
						throw new SyntaxErrorException(binarySyntax.OperatorToken, "Expected: name = value");

					declarations.Add(new ConstDeclaration(identifierSyntax.Identifier, binarySyntax.Right));
				}

				return new ConstStatement(declarations);
			}

	/*
	case TokenType.DATA,
	case TokenType.DECLARE,
	case TokenType.DEF,
	case TokenType.DEFCUR,
	case TokenType.DEFDBL,
	case TokenType.DEFINT,
	case TokenType.DEFLNG,
	case TokenType.DEFSNG,
	case TokenType.DEFSTR,
	case TokenType.DIM,
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

	ExpressionList ParseExpressionList(ListRange<Token> tokens)
	{
		throw new Exception("TODO");
	}

	Expression ParseExpression(ListRange<Token> tokens)
	{
		throw new Exception("TODO");
	}
}
