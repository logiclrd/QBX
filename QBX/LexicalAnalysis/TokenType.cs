namespace QBX.LexicalAnalysis;

public enum TokenType
{
	Empty,

	[KeywordFunction]
	[KeywordToken] ABS,
	[KeywordToken] ACCESS,
	[KeywordToken] AND,
	[KeywordToken] ANY,
	[KeywordToken] APPEND,
	[KeywordToken] AS,
	[KeywordFunction]
	[KeywordToken] ATN,
	[KeywordToken] BINARY,
	[KeywordToken] BYVAL,
	[KeywordToken] CALL,
	[KeywordToken] CASE,
	[KeywordFunction]
	[KeywordToken] CCUR,
	[KeywordFunction]
	[KeywordToken] CDBL,
	[KeywordFunction]
	[KeywordToken("CHR$")] CHR,
	[KeywordFunction]
	[KeywordToken] CINT,
	[KeywordFunction]
	[KeywordToken] CLNG,
	[KeywordToken] CLOSE,
	[KeywordToken] CLS,
	[KeywordToken] COLOR,
	[KeywordToken] COM,
	[KeywordToken] CONST,
	[KeywordFunction]
	[KeywordToken] COS,
	[KeywordFunction]
	[KeywordToken] CSNG,
	[KeywordToken] CURRENCY,
	[KeywordToken] DATA,
	[KeywordToken] DECLARE,
	[KeywordToken] DEF,
	[KeywordToken] DEFCUR,
	[KeywordToken] DEFDBL,
	[KeywordToken] DEFINT,
	[KeywordToken] DEFLNG,
	[KeywordToken] DEFSNG,
	[KeywordToken] DEFSTR,
	[KeywordToken] DIM,
	[KeywordToken] DO,
	[KeywordToken] DOUBLE,
	[KeywordToken] ELSE,
	[KeywordToken] ELSEIF,
	[KeywordToken] END,
	[KeywordToken] EQV,
	[KeywordFunction]
	[KeywordToken] EXP,
	[KeywordFunction]
	[KeywordToken] FIX,
	[KeywordToken] FOR,
	[KeywordToken] FUNCTION,
	[KeywordToken] GET,
	[KeywordToken] GOSUB,
	[KeywordToken] GOTO,
	[KeywordFunction]
	[KeywordToken("HEX$")] HEX,
	[KeywordToken] IF,
	[KeywordToken] IMP,
	[KeywordFunction(parameters: false)]
	[KeywordToken("INKEY$")] INKEY,
	[KeywordToken] INPUT,
	[KeywordToken("INPUT$")] INPUTFunction,
	[KeywordFunction]
	[KeywordToken] INT,
	[KeywordToken] INTEGER,
	[KeywordToken] IS,
	[KeywordToken] KEY,
	[KeywordFunction]
	[KeywordToken("LCASE$")] LCASE,
	[KeywordFunction]
	[KeywordToken] LEN,
	[KeywordFunction]
	[KeywordToken("LEFT$")] LEFT,
	[KeywordToken] LINE,
	[KeywordToken] LOCATE,
	[KeywordToken] LOCK,
	[KeywordFunction]
	[KeywordToken] LOG,
	[KeywordToken] LONG,
	[KeywordToken] LOOP,
	[KeywordToken] LPRINT,
	[KeywordFunction]
	[KeywordToken("MID$")] MID,
	[KeywordToken] MOD,
	[KeywordToken] NEXT,
	[KeywordToken] NOT,
	[KeywordFunction]
	[KeywordToken("OCT$")] OCT,
	[KeywordToken] OFF,
	[KeywordToken] ON,
	[KeywordToken] OPEN,
	[KeywordToken] OR,
	[KeywordToken] OUTPUT,
	[KeywordToken] PCOPY,
	[KeywordFunction]
	[KeywordToken] PEEK,
	[KeywordToken] PEN,
	[KeywordToken] PLAY,
	[KeywordToken] POKE,
	[KeywordToken] PRESET,
	[KeywordToken] PRINT,
	[KeywordToken] PSET,
	[KeywordToken] PUT,
	[KeywordToken] RANDOM,
	[KeywordToken] RANDOMIZE,
	[KeywordToken] READ,
	[KeywordToken] RESTORE,
	[KeywordToken] RETURN,
	[KeywordFunction]
	[KeywordToken("RIGHT$")] RIGHT,
	[KeywordFunction(parameters: true, noParameters: true)]
	[KeywordToken] RND,
	[KeywordToken] SCREEN,
	[KeywordToken] SEG,
	[KeywordToken] SELECT,
	[KeywordFunction]
	[KeywordToken] SGN,
	[KeywordToken] SHARED,
	[KeywordToken] SIGNAL,
	[KeywordFunction]
	[KeywordToken] SIN,
	[KeywordToken] SINGLE,
	[KeywordFunction]
	[KeywordToken("SPACE$")] SPACE,
	[KeywordFunction]
	[KeywordToken] SQR,
	[KeywordToken] STATIC,
	[KeywordToken] STEP,
	[KeywordToken] STOP,
	[KeywordFunction]
	[KeywordToken("STR$")] STR,
	[KeywordToken] STRIG,
	[KeywordToken] STRING,
	[KeywordToken] SUB,
	[KeywordFunction]
	[KeywordToken] TAN,
	[KeywordToken] THEN,
	[KeywordFunction(parameters: false)]
	[KeywordToken] TIMER,
	[KeywordToken] TO,
	[KeywordToken] TYPE,
	[KeywordFunction]
	[KeywordToken("UCASE$")] UCASE,
	[KeywordToken] UEVENT,
	[KeywordToken] UNTIL,
	[KeywordToken] USING,
	[KeywordFunction]
	[KeywordToken] VAL,
	[KeywordToken] VIEW,
	[KeywordToken] WEND,
	[KeywordToken] WHILE,
	[KeywordToken] WIDTH,
	[KeywordToken] WRITE,
	[KeywordToken] XOR,

	[TokenCharacter('#')] NumberSign,
	[TokenCharacter('(')] OpenParenthesis,
	[TokenCharacter(')')] CloseParenthesis,
	[TokenCharacter('*')] Asterisk,
	[TokenCharacter('+')] Plus,
	[TokenCharacter(',')] Comma,
	[TokenCharacter('-')] Minus,
	Hyphen = Minus,
	[TokenCharacter('.')] Period,
	[TokenCharacter('/')] Slash,
	[TokenCharacter(':')] Colon,
	[TokenCharacter(';')] Semicolon,
	[TokenCharacter('<')] LessThan,
	[TokenCharacter('=')] Equals,
	[TokenCharacter('>')] GreaterThan,
	[TokenCharacter('\\')] Backslash,
	[TokenCharacter('^')] Caret,
	[TokenCharacter('|')] Pipe,

	[TokenValue("<=")] LessThanOrEquals,
	[TokenValue(">=")] GreaterThanOrEquals,
	[TokenValue("<>")] NotEquals,

	[ValueToken] Comment,
	[ValueToken] String,
	[ValueToken] Number,
	[ValueToken] Identifier,
	[ValueToken] Whitespace,
	[ValueToken] NewLine,
}
