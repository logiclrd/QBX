using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace QBX.Utility;

public class LineSplittingTextWriter(TextWriter wrapped, int lineLength) : TextWriter, IDisposable
{
	StringBuilder _wordBuffer = new StringBuilder();
	List<char> _matchBuffer = new List<char>(capacity: 5);
	bool _keepLineTogether, _inString;
	int _column;

	public override Encoding Encoding => throw new NotImplementedException();

	public override void Write(char ch)
	{
		if (_inString)
		{
			if ((ch != '"') && (ch != '\r') && (ch != '\n'))
				_wordBuffer.Append(ch);
			else
			{
				if (ch == '"')
					_wordBuffer.Append(ch);

				Flush();

				if (ch != '"')
				{
					_wordBuffer.Append(ch);
					Flush();
				}

				_inString = false;
			}
		}
		else
		{
			if (ch == '"')
			{
				_wordBuffer.Append(ch);
				_inString = true;
			}
			else if ((ch != ' ') && (ch != '\r') && (ch != '\n'))
				_wordBuffer.Append(ch);
			else
			{
				Flush();
				_wordBuffer.Append(ch);
				Flush();
			}
		}
	}

	private bool IsMatch(string str)
	{
		if (_matchBuffer.Count == str.Length)
		{
			var matchSpan = CollectionsMarshal.AsSpan(_matchBuffer);

			return str.Equals(matchSpan, StringComparison.OrdinalIgnoreCase);
		}

		return false;
	}

	public override void Flush()
	{
		if (!_keepLineTogether)
		{
			int cr = _wordBuffer.IndexOf('\r');
			int lf = _wordBuffer.IndexOf('\n');

			if (cr < 0)
				cr = _wordBuffer.Length;
			if (lf < 0)
				lf = _wordBuffer.Length;

			int charsBeforeEOL = Math.Min(cr, lf);

			if (_column + charsBeforeEOL >= lineLength)
			{
				wrapped.WriteLine(" _");
				_column = 0;
			}
		}

		// Scan for keywords that disable line splitting (REM and DATA, and also ' comments).
		bool inString = false;

		for (int i=0; i < _wordBuffer.Length; i++)
		{
			char ch = _wordBuffer[i];

			if ((ch == '\r') || (ch == '\n'))
			{
				_column = 0;
				_keepLineTogether = false;
				_matchBuffer.Clear();
				inString = false;
			}
			else if (!_keepLineTogether)
			{
				_column++;

				if (inString)
				{
					if (ch == '"')
					{
						_matchBuffer.Clear();
						inString = false;
					}
				}
				else
				{
					switch (ch)
					{
						case '"':
							inString = true;
							break;
						case '\'':
							_keepLineTogether = true;
							break;

						case ' ':
						case '\t':
						case '(':
						case ')':
						case '%':
						case '&':
						case '!':
						case '#':
						case '@':
						case '$':
						case '^':
						case '*':
						case '+':
						case '-':
						case '/':
						case '\\':
						case '<':
						case '=':
						case '>':
						case '.':
						case ',':
						case ':':
							if (IsMatch("REM") || IsMatch("DATA"))
								_keepLineTogether = true;

							_matchBuffer.Clear();
							break;

						default:
							_matchBuffer.Add(ch);
							break;
					}
				}
			}
		}

		wrapped.Write(_wordBuffer);
		_wordBuffer.Length = 0;
	}

	protected override void Dispose(bool disposing)
	{
		Flush();
		base.Dispose(disposing);
	}
}
