using System;
using System.Linq;
using System.Text;

using QBX.ExecutionEngine.Compiled.Statements;
using QBX.LexicalAnalysis;

namespace QBX.Parser;

public class TokenHandler(ListRange<Token> tokens, IdentifierRepository identifierRepository)
{
	ListRange<Token> _tokens = tokens;
	int _tokenIndex = 0;

	Token FindTokenToBlame()
	{
		if ((_tokenIndex >= 0) && (_tokenIndex < _tokens.Count))
			return _tokens[_tokenIndex];

		var range = _tokens.Unwrap();

		if (range.Offset + range.Count < range.List.Count)
			return range.List[range.Offset + range.Count];
		else if (range.Offset > 0)
			return range.List[range.Offset - 1];
		else if (range.List.Count > 0)
			return range.List.Last();
		else
			return EndToken;
	}

	public Token EndToken
	{
		get
		{
			if (_tokens.Count == 0)
				return new Token(Token.CreateDummyLine(), 0, TokenType.Empty, "");

			var lastToken = _tokens.Last();

			return new Token(
				lastToken.LineNumberBox,
				lastToken.Column + (lastToken.Value?.Length ?? 1),
				TokenType.Empty,
				" ");
		}
	}

	public int TokenIndex
	{
		get => _tokenIndex;
		set => _tokenIndex = value;
	}

	public Token NextToken => this[0];

	public Token this[int relativeIndex]
	{
		get
		{
			if (_tokenIndex + relativeIndex >= _tokens.Count)
				throw new SyntaxErrorException(FindTokenToBlame(), "Unexpected end of statement/expression");

			return _tokens[_tokenIndex + relativeIndex];
		}
	}

	public Token PreviousToken
	{
		get
		{
			if (_tokenIndex > 0)
				return _tokens[_tokenIndex - 1];
			else
				throw new Exception("No previous token");
		}
	}

	public ListRange<Token> RemainingTokens => _tokens.Slice(_tokenIndex);

	public void Reset()
	{
		_tokenIndex = 0;
	}

	public void Advance(int count = 1)
	{
		if (_tokenIndex + count > _tokens.Count)
			throw new SyntaxErrorException(FindTokenToBlame(), "Unexpected end of statement/expression");

		_tokenIndex += count;
	}

	public void AdvanceToEnd()
	{
		_tokenIndex = _tokens.Count;
	}

	public bool HasMoreTokens => (_tokenIndex < _tokens.Count);

	public void ExpectMoreTokens(string message = "Unexpected end of statement")
	{
		if (!HasMoreTokens)
			throw new SyntaxErrorException(FindTokenToBlame(), message);
	}

	public void ExpectEndOfTokens(string message = "Expected end of statement")
	{
		if (HasMoreTokens)
			throw new SyntaxErrorException(FindTokenToBlame(), message);
	}

	public bool NextTokenIs(TokenType type) => HasMoreTokens && (_tokens[_tokenIndex].Type == type);

	public Identifier ExpectIdentifier(bool allowTypeCharacter, bool allowDots)
		=> ExpectIdentifier(allowTypeCharacter, allowDots, out _);

	public Identifier ExpectIdentifier(bool allowTypeCharacter, bool allowDots, out Token identifierToken)
	{
		if (!HasMoreTokens)
			throw new SyntaxErrorException(FindTokenToBlame(), "Unexpected end of statement");

		var firstToken = _tokens[_tokenIndex];

		if (!firstToken.IsWordStart)
			throw new SyntaxErrorException(firstToken, "Expected identifier");

		int numTokens = 1;

		if (allowDots)
		{
			// Combine any combination of adjacent dots and word-like tokens (including numbers -- as long as they don't contain symbols),
			// optionally terminated with a type-declaration character.

			bool IsTypeDeclarationCharacterToken(Token token)
			{
				switch (token.Value)
				{
					case "%":
					case "&":
					case "!":
					case "#":
					case "$":
					case "@":
						return true;
				}

				return false;
			}

			while (_tokenIndex + numTokens < _tokens.Count)
			{
				var nextToken = _tokens[_tokenIndex + numTokens];

				if (!string.IsNullOrEmpty(nextToken.PrecedingWhitespace))
					break;
				if ((nextToken.Type != TokenType.Period) && !nextToken.IsWord && !IsTypeDeclarationCharacterToken(nextToken))
					break;

				numTokens++;

				if (char.IsSymbol(nextToken.Value.Last()))
					break;
			}
		}

		string identifier;

		if (numTokens == 1)
		{
			identifierToken = firstToken;
			identifier = identifierToken.Value;

			if (identifierToken.Type != TokenType.Identifier)
				throw new SyntaxErrorException(identifierToken, "Expected identifier");

			_tokenIndex++;
		}
		else
		{
			var builder = new StringBuilder();

			for (int i=0; i < numTokens; i++)
				builder.Append(_tokens[_tokenIndex++].Value);

			identifier = builder.ToString();

			identifierToken = new Token(firstToken.LineNumberBox, firstToken.Column, TokenType.Identifier, identifier);
			identifierToken.PrecedingWhitespace = firstToken.PrecedingWhitespace;
		}

		if (!allowTypeCharacter && char.IsSymbol(identifier.Last()))
			throw new SyntaxErrorException(identifierToken, "Identifier cannot end with %, &, !, #, $, or @");

		return identifierRepository.UpdateCanonicalIdentifier(identifier);
	}

	public Token Expect(TokenType expectedTokenType)
	{
		if (!HasMoreTokens)
			throw new SyntaxErrorException(FindTokenToBlame(), "Unexpected end of statement");

		var token = _tokens[_tokenIndex];

		if (token.Type != expectedTokenType)
			throw new SyntaxErrorException(token, "Expected: " + expectedTokenType.GetString());

		_tokenIndex++;

		return token;
	}

	public Token ExpectOneOf(params TokenType[] tokenTypes)
	{
		if (!HasMoreTokens)
			throw new SyntaxErrorException(FindTokenToBlame(), "Unexpected end of statement");

		var token = _tokens[_tokenIndex];

		if (!tokenTypes.Contains(token.Type))
			throw new SyntaxErrorException(token, "Expected: " + string.Join(", ", tokenTypes));

		_tokenIndex++;

		return token;
	}

	public ListRange<Token> ExpectParenthesizedTokens()
		=> ExpectParenthesizedTokens(out _);

	public ListRange<Token> ExpectParenthesizedTokens(out Token endToken)
	{
		if (!NextTokenIs(TokenType.OpenParenthesis))
			throw new SyntaxErrorException(FindTokenToBlame(), "Expected: (");

		int level = 1;

		_tokenIndex++;

		int rangeStart = _tokenIndex;

		while (HasMoreTokens && (level > 0))
		{
			switch (_tokens[_tokenIndex].Type)
			{
				case TokenType.OpenParenthesis: level++; break;
				case TokenType.CloseParenthesis: level--; break;
			}

			_tokenIndex++;
		}

		if (level > 0)
			throw new SyntaxErrorException(FindTokenToBlame(), "Expected: )");

		int rangeEnd = _tokenIndex - 1;

		endToken = _tokens[rangeEnd];

		return _tokens.Slice(rangeStart, rangeEnd - rangeStart);
	}

	public int FindNextUnparenthesizedOf(params TokenType[] tokenToFind)
		=> FindNextUnparenthesizedOf(startIndex: 0, tokenToFind);

	public int FindNextUnparenthesizedOf(int startIndex, params TokenType[] tokenToFind)
	{
		int index = 0;
		int level = 0;

		var findSet = tokenToFind.ToHashSet();

		while (_tokenIndex + index < _tokens.Count)
		{
			if ((level == 0) && (index >= startIndex) && findSet.Contains(_tokens[_tokenIndex + index].Type))
				return index;

			switch (_tokens[_tokenIndex + index].Type)
			{
				case TokenType.OpenParenthesis: level++; break;
				case TokenType.CloseParenthesis: level--; break;
			}

			index++;
		}

		return -1;
	}
}
