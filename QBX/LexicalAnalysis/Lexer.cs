using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using QBX.CodeModel;
using QBX.Utility;

namespace QBX.LexicalAnalysis;

public class Lexer(TextReader input, CompilationElement? element = null, int startingLineNumber = 0) : IEnumerable<Token>
{
	TextReader _input = input;
	bool _consumed = false;
	Token _endToken = new Token(new MutableBox<int>(-1), -1, TokenType.Empty, "");

	public Token EndToken => _endToken;

	public CompilationElement? CurrentElement { get; set; } = element;

	public Lexer(string text, CompilationElement? element = null)
		: this(new StringReader(text), element)
	{
	}

	enum Mode
	{
		Any,
		Whitespace,
		Comment,
		String,
		RawStringToEndOfLine,
		Number,
		NumberAfterDecimal,
		NumberAfterExponent,
		NumberWithBase, // &H or &O
		HexNumber,
		OctalNumber,
		MaybeNumber, // seen a '.', don't know if it's ".Member" or ".234#"
		MaybeNegativeNumber, // seen a '-', don't know if it's "-123"/"-.123" or "-expression"
		MaybeOrEquals, // seen a '<' or '>', don't know if it'll be a "<=" or ">="
		MaybeCrLf, // seen a '\r', don't know if it'll be a "\r\n"
		Word,
	}

	public IEnumerator<Token> GetEnumerator()
	{
		var enumerator = Tokenize();

		while (enumerator.MoveNext())
		{
			var token = enumerator.Current;

			token.OwnerElement = CurrentElement;

			yield return token;
		}
	}

	IEnumerator<Token> Tokenize()
	{
		if (_consumed)
			throw new InvalidOperationException("This lexer has already been consumed");

		_consumed = true;

		var buffer = new StringBuilder();
		var mode = Mode.Any;

		MutableBox<int> line = new MutableBox<int>(startingLineNumber);
		int column = 0;

		int tokenStartColumn = column;

		while (true)
		{
			int readResult = _input.Read();

			bool atEOF = (readResult < 0);

			char ch = unchecked((char)readResult);

			bool reparse;

			do
			{
				reparse = false;

				switch (mode)
				{
					case Mode.Any:
					{
						if (atEOF)
							break;

						if (ch == '\r')
							mode = Mode.MaybeCrLf;
						else if (ch == '\n')
						{
							yield return new Token(line, tokenStartColumn, TokenType.NewLine, "\n");
							line = new MutableBox<int>(line.Value + 1);
							column = 0;
							tokenStartColumn = column;
							break;
						}
						else if ((ch == '<') || (ch == '>'))
							mode = Mode.MaybeOrEquals;
						else if (ch == '\'')
							mode = Mode.Comment;
						else if (ch == '"')
							mode = Mode.String;
						else if (char.IsDigit(ch))
							mode = Mode.Number;
						else if (ch == '&')
							mode = Mode.NumberWithBase;
						else if (ch == '.')
							mode = Mode.MaybeNumber;
						else if (ch == '-')
							mode = Mode.MaybeNegativeNumber;
						else if (char.IsAsciiLetter(ch))
							mode = Mode.Word;
						else if (char.IsWhiteSpace(ch))
							mode = Mode.Whitespace;
						else if (Token.TryForCharacter(line, column, ch, out var token))
						{
							yield return token;
							tokenStartColumn = column + 1;
							break;
						}
						else
						{
							yield return new Token(line, column, TokenType.StrayCharacter, ch.ToString());
							tokenStartColumn = column + 1;
							break;
						}

						// TODO: some sort of "ignore errors" mode to allow a .BAS file with
						//       an invalid token stream to be loaded

						buffer.Append(ch);

						break;
					}
					case Mode.Whitespace:
					{
						if (char.IsWhiteSpace(ch) && (ch != '\r') && (ch != '\n'))
							buffer.Append(ch);
						else
						{
							yield return new Token(line, tokenStartColumn, TokenType.Whitespace, buffer.ToString());
							buffer.Clear();
							mode = Mode.Any;
							reparse = true;
							tokenStartColumn = column;
						}

						break;
					}
					case Mode.MaybeCrLf:
					{
						if (ch == '\n')
						{
							yield return new Token(line, tokenStartColumn, TokenType.NewLine, "\r\n");

							line = new MutableBox<int>(line.Value + 1);
							column = 0;
							tokenStartColumn = 0;

							buffer.Clear();
							mode = Mode.Any;
						}
						else
						{
							yield return new Token(line, column, TokenType.NewLine, "\r");

							line = new MutableBox<int>(line.Value + 1);
							column = 0;
							tokenStartColumn = 1;

							if (ch != '\r')
							{
								buffer.Clear();
								mode = Mode.Any;
								reparse = true;
							}
						}

						break;
					}
					case Mode.RawStringToEndOfLine:
					{
						if ((ch == '\r') || (ch == '\n') || atEOF)
						{
							yield return new Token(line, tokenStartColumn, TokenType.RawString, buffer.ToString());
							buffer.Clear();
							mode = Mode.Any;
							reparse = true;
							tokenStartColumn = column;
						}
						else
							buffer.Append(ch);

						break;
					}
					case Mode.Comment:
					{
						if ((ch == '\r') || (ch == '\n') || atEOF)
						{
							yield return new Token(line, tokenStartColumn, TokenType.Comment, buffer.ToString());
							buffer.Clear();
							mode = Mode.Any;
							reparse = true;
							tokenStartColumn = column;
						}
						else
							buffer.Append(ch);

						break;
					}
					case Mode.String:
					{
						if ((ch == '\r') || (ch == '\n') || atEOF)
						{
							yield return new Token(line, tokenStartColumn, TokenType.String, buffer.ToString());
							buffer.Clear();
							mode = Mode.Any;
							reparse = true;
							tokenStartColumn = column;
						}
						else
						{
							buffer.Append(ch);

							if (ch == '"')
							{
								yield return new Token(line, tokenStartColumn, TokenType.String, buffer.ToString());
								buffer.Clear();
								mode = Mode.Any;
								tokenStartColumn = column + 1;
							}
						}

						break;
					}
					case Mode.MaybeNumber: // seen a '.', don't know if it's ".Member" or ".234#"
					{
						if (char.IsDigit(ch))
						{
							buffer.Append(ch);
							mode = Mode.NumberAfterDecimal;
						}
						else
						{
							yield return Token.ForCharacter(line, tokenStartColumn, '.');
							buffer.Clear();
							mode = Mode.Any;
							reparse = true;
							tokenStartColumn = column;
						}

						break;
					}
					case Mode.MaybeNegativeNumber:
					{
						// seen a '-', don't know whether it's a negative number or just a '-' before an unrelated expression

						if (char.IsDigit(ch))
						{
							buffer.Append(ch);
							mode = Mode.Number;
						}
						else if (ch == '.')
						{
							buffer.Append(ch);
							mode = Mode.NumberAfterDecimal;
						}
						else if (ch == '&')
						{
							buffer.Append(ch);
							mode = Mode.NumberWithBase;
						}
						else
						{
							yield return Token.ForCharacter(line, tokenStartColumn, '-');
							buffer.Clear();
							mode = Mode.Any;
							reparse = true;
							tokenStartColumn = column;
						}

						break;
					}
					case Mode.Number:
					{
						if (char.IsDigit(ch))
							buffer.Append(ch);
						else
						{
							if ((ch == 'e') || (ch == 'E')
							 || (ch == 'd') || (ch == 'D'))
							{
								buffer.Append(ch);
								mode = Mode.NumberAfterExponent;
							}
							else if (ch == '.')
							{
								buffer.Append(ch);
								mode = Mode.NumberAfterDecimal;
							}
							else
								goto case Mode.NumberAfterDecimal;
						}

						break;
					}
					case Mode.NumberAfterDecimal:
					{
						if (char.IsDigit(ch))
							buffer.Append(ch);
						else
						{
							if ((ch == 'e') || (ch == 'E')
							 || (ch == 'd') || (ch == 'D'))
							{
								buffer.Append(ch);
								mode = Mode.NumberAfterExponent;
							}
							else
							{
								var dataType = DataType.Unspecified;

								switch (ch)
								{
									case '%':
									case '&':
									case '!':
									case '#':
									case '@':
										buffer.Append(ch);

										switch (ch)
										{
											case '%': dataType = DataType.INTEGER; break;
											case '&': dataType = DataType.LONG; break;
											case '!': dataType = DataType.SINGLE; break;
											case '#': dataType = DataType.DOUBLE; break;
											case '@': dataType = DataType.CURRENCY; break;
										}

										break;
									default:
										reparse = true;
										break;
								}

								yield return new Token(line, tokenStartColumn, TokenType.Number, buffer.ToString(), dataType);
								buffer.Clear();
								mode = Mode.Any;
								tokenStartColumn = column;
							}
						}

						break;
					}
					case Mode.NumberAfterExponent:
					{
						if (char.IsDigit(ch))
							buffer.Append(ch);
						else
						{
							var dataType = DataType.Unspecified;

							switch (ch)
							{
								case '%':
								case '&':
								case '!':
								case '#':
								case '@':
									buffer.Append(ch);

									switch (ch)
									{
										case '%': dataType = DataType.INTEGER; break;
										case '&': dataType = DataType.LONG; break;
										case '!': dataType = DataType.SINGLE; break;
										case '#': dataType = DataType.DOUBLE; break;
										case '@': dataType = DataType.CURRENCY; break;
									}

									break;
								default:
									reparse = true;
									break;
							}

							yield return new Token(line, tokenStartColumn, TokenType.Number, buffer.ToString(), dataType);
							buffer.Clear();
							mode = Mode.Any;
							tokenStartColumn = column;
						}

						break;
					}
					case Mode.NumberWithBase:
					{
						switch (ch)
						{
							case 'H': case 'h': mode = Mode.HexNumber; break;
							case 'O': case 'o': mode = Mode.OctalNumber; break;
							default:
								yield return new Token(line, tokenStartColumn, TokenType.StrayCharacter, buffer.ToString());
								buffer.Clear();

								mode = Mode.Any;
								tokenStartColumn = column;
								reparse = true;
								continue;
						}

						buffer.Append(ch);

						break;
					}
					case Mode.HexNumber:
					{
						if (char.IsAsciiHexDigit(ch))
							buffer.Append(ch);
						else
						{
							var dataType = DataType.Unspecified;

							switch (ch)
							{
								case '%':
								case '&':
									buffer.Append(ch);

									switch (ch)
									{
										case '%': dataType = DataType.INTEGER; break;
										case '&': dataType = DataType.LONG; break;
									}

									break;
								default:
									reparse = true;
									break;
							}

							string hexString = buffer.ToString(2, buffer.Length - 2);

							yield return new Token(line, tokenStartColumn, TokenType.Number, buffer.ToString(), dataType);
							buffer.Clear();
							mode = Mode.Any;
							tokenStartColumn = column;
						}

						break;
					}
					case Mode.OctalNumber:
					{
						// '8' and '9' will be detected by NumberParser and handled by
						// Compiler.TranslateExpression later on, generating an Overflow
						// error. We collect them here as though they were valid.
						if ((ch >= '0') && (ch <= '9'))
							buffer.Append(ch);
						else
						{
							var dataType = DataType.Unspecified;

							switch (ch)
							{
								case '%':
								case '&':
									buffer.Append(ch);

									switch (ch)
									{
										case '%': dataType = DataType.INTEGER; break;
										case '&': dataType = DataType.LONG; break;
									}

									break;
								default:
									reparse = true;
									break;
							}

							yield return new Token(line, tokenStartColumn, TokenType.Number, buffer.ToString(), dataType);
							buffer.Clear();
							mode = Mode.Any;
							tokenStartColumn = column;
						}

						break;
					}
					case Mode.MaybeOrEquals: // seen a '<' or '>', don't know if it'll be a "<=" or ">="
					{
						if (ch == '=')
						{
							switch (buffer[0])
							{
								case '<': yield return Token.GetStatic(line, tokenStartColumn, "<=", TokenType.LessThanOrEquals); break;
								case '>': yield return Token.GetStatic(line, tokenStartColumn, ">=", TokenType.GreaterThanOrEquals); break;
							}
						}
						else if (ch == '>')
							yield return Token.GetStatic(line, tokenStartColumn, "<>", TokenType.NotEquals);
						else
						{
							yield return Token.ForCharacter(line, tokenStartColumn, buffer[0]);
							reparse = true;
						}

						buffer.Clear();
						mode = Mode.Any;
						tokenStartColumn = column;

						if (!reparse)
							tokenStartColumn++;

						break;
					}
					case Mode.Word:
					{
						if (char.IsAsciiLetterOrDigit(ch))
							buffer.Append(ch);
						else if (buffer.Equals("DATA", StringComparison.OrdinalIgnoreCase) && (ch != '$'))
						{
							yield return new Token(line, tokenStartColumn, TokenType.DATA, "DATA");

							tokenStartColumn = column;

							buffer.Clear();

							if (ch == '.')
							{
								yield return new Token(line, tokenStartColumn, TokenType.Period, ".");
								tokenStartColumn++;
								mode = Mode.Any;
							}
							else
							{
								if (!atEOF)
									buffer.Append(ch);

								mode = Mode.RawStringToEndOfLine;
							}
						}
						else
						{
							var dataType = DataType.Unspecified;

							Token? keyword = null;

							string word = buffer.ToString();
							string qualifiedWord = word;

							switch (ch)
							{
								case '%':
								case '&':
								case '!':
								case '#':
								case '@':
									qualifiedWord = word + ch;

									if (!Token.TryForKeyword(line, tokenStartColumn, qualifiedWord, out keyword)
									 && Token.TryForKeyword(line, tokenStartColumn, word, out var keywordFollowedBySymbol))
									{
										keyword = keywordFollowedBySymbol;
										reparse = true;
									}
									else
									{
										switch (ch)
										{
											case '%': dataType = DataType.INTEGER; break;
											case '&': dataType = DataType.LONG; break;
											case '!': dataType = DataType.SINGLE; break;
											case '#': dataType = DataType.DOUBLE; break;
											case '@': dataType = DataType.CURRENCY; break;
										}
									}

									break;
								case '$':
									qualifiedWord = word + ch;
									Token.TryForKeyword(line, tokenStartColumn, qualifiedWord, out keyword);
									dataType = DataType.STRING;
									break;
								default:
									Token.TryForKeyword(line, tokenStartColumn, word, out keyword);
									reparse = true;
									break;
							}

							if (word.Equals("REM", StringComparison.OrdinalIgnoreCase))
							{
								mode = Mode.Comment;
								break;
							}

							if (keyword != null)
								yield return keyword;
							else
								yield return Token.ForIdentifier(line, tokenStartColumn, qualifiedWord, dataType);

							buffer.Clear();
							mode = Mode.Any;
							tokenStartColumn = column;

							if (!reparse)
								tokenStartColumn++;
						}

						break;
					}
				}
			} while (reparse);

			if (atEOF)
			{
				_endToken.SetLocation(line, column);
				break;
			}

			if ((ch != '\r') && (ch != '\n'))
				column++;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
