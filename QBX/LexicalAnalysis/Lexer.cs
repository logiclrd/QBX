using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using QBX.CodeModel;

namespace QBX.LexicalAnalysis;

public class Lexer(TextReader input) : IEnumerable<Token>
{
	TextReader _input = input;
	bool _consumed = false;

	public Lexer(string text)
		: this(new StringReader(text))
	{
	}

	enum Mode
	{
		Any,
		Whitespace,
		Comment,
		String,
		Number,
		NumberAfterDecimal,
		NumberWithBase, // &H or &O
		HexNumber,
		OctalNumber,
		MaybeNumber, // seen a '.', don't know if it's ".Member" or ".234#"
		MaybeOrEquals, // seen a '<' or '>', don't know if it'll be a "<=" or ">="
		MaybeCrLf, // seen a '\r', don't know if it'll be a "\r\n"
		Word,
	}

	public IEnumerator<Token> GetEnumerator()
	{
		if (_consumed)
			throw new InvalidOperationException("This lexer has already been consumed");

		_consumed = true;

		var buffer = new StringBuilder();
		var mode = Mode.Any;

		int line = 1;
		int column = 1;

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
							line++;
							column = 1;
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
							throw new Exception("Couldn't parse file: Unexpected character '" + ch + "'");

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

							line++;
							column = 1;
							tokenStartColumn = 1;

							buffer.Clear();
							mode = Mode.Any;
						}
						else
						{
							yield return new Token(line, column, TokenType.NewLine, "\r");

							line++;
							column = 1;
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
					case Mode.Number:
					{
						if (char.IsDigit(ch))
							buffer.Append(ch);
						else
						{
							if (ch == '.')
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
								throw new Exception("Expected: &H or &O");
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
						if ((ch >= '0') && (ch < '8'))
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
								case '<': yield return Token.GetStatic(line, tokenStartColumn, TokenType.LessThanOrEquals); break;
								case '>': yield return Token.GetStatic(line, tokenStartColumn, TokenType.GreaterThanOrEquals); break;
							}
						}
						else if (ch == '>')
							yield return Token.GetStatic(line, tokenStartColumn, TokenType.NotEquals);
						else
						{
							yield return Token.ForCharacter(line, tokenStartColumn, buffer[0]);
							buffer.Clear();
							reparse = true;
						}

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
								case '$':
									buffer.Append(ch);

									switch (ch)
									{
										case '%': dataType = DataType.INTEGER; break;
										case '&': dataType = DataType.LONG; break;
										case '!': dataType = DataType.SINGLE; break;
										case '#': dataType = DataType.DOUBLE; break;
										case '@': dataType = DataType.CURRENCY; break;
										case '$': dataType = DataType.STRING; break;
									}

									break;
								default:
									reparse = true;
									break;
							}

							string word = buffer.ToString();

							if (word.Equals("REM", StringComparison.OrdinalIgnoreCase))
							{
								mode = Mode.Comment;
								break;
							}

							if (Token.TryForKeyword(line, tokenStartColumn, word, out var keyword))
								yield return keyword;
							else
								yield return new Token(line, tokenStartColumn, TokenType.Identifier, word, dataType);

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
				break;

			if ((ch != '\r') && (ch != '\n'))
				column++;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
