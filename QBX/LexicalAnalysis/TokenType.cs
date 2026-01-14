namespace QBX.LexicalAnalysis;

public enum TokenType
{
	Empty,

	[KeywordFunction]
	[KeywordToken] ABS,
	[KeywordToken] ACCESS,
	[KeywordToken] AND,
	[KeywordToken] ALIAS,
	[KeywordToken] ALL,
	[KeywordToken] ANY,
	[KeywordToken] APPEND,
	[KeywordToken] AS,
	[KeywordFunction]
	[KeywordToken] ASC,
	[KeywordFunction]
	[KeywordToken] ATN,
	[KeywordToken] BASE,
	[KeywordToken] BEGINTRANS,
	[KeywordToken] BINARY,
	[KeywordFunction(fileNumberParameter: 0)]
	[KeywordToken] BOF,
	[KeywordToken] BYVAL,
	[KeywordToken] CALL,
	[KeywordToken] CASE,
	[KeywordFunction]
	[KeywordToken] CCUR,
	[KeywordFunction]
	[KeywordToken] CDBL,
	[KeywordToken] CDECL,
	[KeywordFunction]
	[KeywordToken("CHR$")] CHR,
	[KeywordFunction]
	[KeywordToken] CINT,
	[KeywordToken] CIRCLE,
	[KeywordFunction]
	[KeywordToken] CLNG,
	[KeywordToken] CLOSE,
	[KeywordToken] CLS,
	[KeywordToken] COLOR,
	[KeywordToken] COM,
	[KeywordFunction]
	[KeywordToken("COMMAND$")] COMMAND,
	[KeywordToken] COMMITTRANS,
	[KeywordToken] CONST,
	[KeywordFunction]
	[KeywordToken] COS,
	[KeywordFunction]
	[KeywordToken] CSNG,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken] CSRLIN,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken("CURDIR$")] CURDIR,
	[KeywordToken] CURRENCY,
	[KeywordFunction]
	[KeywordToken] CVC,
	[KeywordFunction]
	[KeywordToken] CVD,
	[KeywordFunction]
	[KeywordToken] CVDMBF,
	[KeywordFunction]
	[KeywordToken] CVI,
	[KeywordFunction]
	[KeywordToken] CVL,
	[KeywordFunction]
	[KeywordToken] CVS,
	[KeywordFunction]
	[KeywordToken] CVSMBF,
	[KeywordToken] DATA,
	[KeywordFunction(parameterCount: 0, isAssignable: true)]
	[KeywordToken("DATE$")] DATE,
	[KeywordToken] DECLARE,
	[KeywordToken] DEF,
	[KeywordToken] DEFCUR,
	[KeywordToken] DEFDBL,
	[KeywordToken] DEFINT,
	[KeywordToken] DEFLNG,
	[KeywordToken] DEFSNG,
	[KeywordToken] DEFSTR,
	[KeywordToken] DIM,
	[KeywordFunction(minimumParameterCount: 0)]
	[KeywordToken("DIR$")] DIR,
	[KeywordToken] DO,
	[KeywordToken] DOUBLE,
	[KeywordToken] ELSE,
	[KeywordToken] ELSEIF,
	[KeywordToken] END,
	[KeywordToken] ENDIF,
	[KeywordFunction]
	[KeywordToken("ENVIRON$")] ENVIRON,
	[KeywordFunction(fileNumberParameter: 0)]
	[KeywordToken] EOF,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken] ERDEV,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken("ERDEV$")] ERDEV_s,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken] ERL,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken] ERR,
	[KeywordToken] ERROR,
	[KeywordFunction]
	[KeywordToken] EQV,
	[KeywordToken] EXIT,
	[KeywordFunction]
	[KeywordToken] EXP,
	[KeywordToken] FIELD,
	[KeywordFunction(maximumParameterCount: 2)]
	[KeywordToken] FILEATTR,
	[KeywordFunction]
	[KeywordToken] FIX,
	[KeywordFunction]
	[KeywordToken] FRE,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken] FREEFILE,
	[KeywordToken] FOR,
	[KeywordToken] FUNCTION,
	[KeywordToken] GET,
	[KeywordFunction]
	[KeywordToken("GETINDEX$")] GETINDEX,
	[KeywordToken] GOSUB,
	[KeywordToken] GOTO,
	[KeywordFunction]
	[KeywordToken("HEX$")] HEX,
	[KeywordToken] IF,
	[KeywordToken] IMP,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken("INKEY$")] INKEY,
	[KeywordToken] INPUT,
	[KeywordFunction(maximumParameterCount: 2, fileNumberParameter: 1)]
	[KeywordToken("INPUT$")] INPUT_s, // TODO: parsing must allow for a # sign before file numbers
	[KeywordFunction(minimumParameterCount: 2, maximumParameterCount: 3)]
	[KeywordToken] INSTR,
	[KeywordFunction]
	[KeywordToken] INT,
	[KeywordToken] INTEGER,
	[KeywordToken] IOCTL,
	[KeywordFunction(fileNumberParameter: 0)]
	[KeywordToken("IOCTL$")] IOCTL_s, // TODO: parsing must allow for a # sign before file numbers
	[KeywordToken] IS,
	[KeywordToken] KEY,
	[KeywordFunction(maximumParameterCount: 2)]
	[KeywordToken] LBOUND,
	[KeywordFunction]
	[KeywordToken("LCASE$")] LCASE,
	[KeywordFunction(parameterCount: 2)]
	[KeywordToken("LEFT$")] LEFT,
	[KeywordFunction]
	[KeywordToken] LEN,
	[KeywordToken] LET,
	[KeywordToken] LINE,
	[KeywordFunction]
	[KeywordToken] LOC,
	[KeywordToken] LOCAL,
	[KeywordToken] LOCATE,
	[KeywordToken] LOCK,
	[KeywordFunction]
	[KeywordToken] LOF,
	[KeywordFunction]
	[KeywordToken] LOG,
	[KeywordToken] LONG,
	[KeywordToken] LOOP,
	[KeywordFunction]
	[KeywordToken] LPOS,
	[KeywordToken] LPRINT,
	[KeywordFunction]
	[KeywordToken("LTRIM$")] LTRIM,
	[KeywordFunction(minimumParameterCount: 2, maximumParameterCount: 3, isAssignable: true)]
	[KeywordToken("MID$")] MID,
	[KeywordFunction]
	[KeywordToken("MKC$")] MKC,
	[KeywordFunction]
	[KeywordToken("MKD$")] MKD,
	[KeywordFunction]
	[KeywordToken("MKDMBF$")] MKDMBF,
	[KeywordFunction]
	[KeywordToken("MKI$")] MKI,
	[KeywordFunction]
	[KeywordToken("MKL$")] MKL,
	[KeywordFunction]
	[KeywordToken("MKS$")] MKS,
	[KeywordFunction]
	[KeywordToken("MKSMBF$")] MKSMBF,
	[KeywordToken] MOD,
	[KeywordToken] NAME,
	[KeywordToken] NEXT,
	[KeywordToken] NOT,
	[KeywordFunction]
	[KeywordToken("OCT$")] OCT,
	[KeywordToken] OFF,
	[KeywordToken] ON,
	[KeywordToken] OPEN,
	[KeywordToken] OPTION,
	[KeywordToken] OR,
	[KeywordToken] OUTPUT,
	[KeywordToken] PAINT,
	[KeywordToken] PALETTE,
	[KeywordToken] PCOPY,
	[KeywordFunction]
	[KeywordToken] PEEK,
	[KeywordFunction] // can also be a statement
	[KeywordToken] PEN,
	[KeywordFunction] // can also be a statement
	[KeywordToken] PLAY,
	[KeywordFunction(parameterCount: 2)]
	[KeywordToken] PMAP,
	[KeywordFunction(parameterCount: 2)]
	[KeywordToken] POINT,
	[KeywordToken] POKE,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken] POS,
	[KeywordToken] PRESERVE,
	[KeywordToken] PRESET,
	[KeywordToken] PRINT,
	[KeywordToken] PSET,
	[KeywordToken] PUT,
	[KeywordToken] RANDOM,
	[KeywordToken] RANDOMIZE,
	[KeywordToken] READ,
	[KeywordToken] REDIM,
	[KeywordToken] RESET,
	[KeywordToken] RESTORE,
	[KeywordToken] RESUME,
	[KeywordToken] RETURN,
	[KeywordFunction(parameterCount: 2)]
	[KeywordToken("RIGHT$")] RIGHT,
	[KeywordFunction(minimumParameterCount: 0, maximumParameterCount: 1)]
	[KeywordToken] RND,
	[KeywordToken] ROLLBACK,
	[KeywordFunction]
	[KeywordToken("RTRIM$")] RTRIM,
	[KeywordFunction]
	[KeywordToken] SADD,
	[KeywordFunction(parameterCount: 0)]
	[KeywordToken] SAVEPOINT,
	[KeywordFunction(minimumParameterCount: 2, maximumParameterCount: 3)] // can also be a statement
	[KeywordToken] SCREEN,
	[KeywordFunction]
	[KeywordToken] SEEK,
	[KeywordToken] SEG,
	[KeywordToken] SELECT,
	[KeywordToken] SETINDEX,
	[KeywordFunction]
	[KeywordToken] SGN,
	[KeywordToken] SHARED,
	[KeywordFunction] // can also be a statement
	[KeywordToken] SHELL,
	[KeywordToken] SIGNAL,
	[KeywordFunction]
	[KeywordToken] SIN,
	[KeywordToken] SINGLE,
	[KeywordToken] SOUND,
	[KeywordFunction]
	[KeywordToken("SPACE$")] SPACE,
	[KeywordToken] SPC,
	[KeywordFunction]
	[KeywordToken] SQR,
	[KeywordFunction]
	[KeywordToken] SSEG,
	[KeywordFunction]
	[KeywordToken] SSEGADD,
	[KeywordFunction(parameterCount: 0)] // can also be a statement
	[KeywordToken] STACK,
	[KeywordToken] STATIC,
	[KeywordToken] STEP,
	[KeywordFunction]
	[KeywordToken] STICK,
	[KeywordToken] STOP,
	[KeywordFunction]
	[KeywordToken("STR$")] STR,
	[KeywordToken] STRIG,
	[KeywordToken] STRING,
	[KeywordFunction(parameterCount: 2)]
	[KeywordToken("STRING$")] STRING_s,
	[KeywordToken] SUB,
	[KeywordToken] TAB,
	[KeywordFunction]
	[KeywordToken] TAN,
	[KeywordFunction(parameterCount: 2)]
	[KeywordToken] TEXTCOMP,
	[KeywordToken] THEN,
	[KeywordFunction(parameterCount: 0, isAssignable: true)]
	[KeywordToken("TIME$")] TIME,
	[KeywordFunction(parameterCount: 0)] // can also be a statement
	[KeywordToken] TIMER,
	[KeywordToken] TO,
	[KeywordToken] TYPE,
	[KeywordFunction(maximumParameterCount: 2)]
	[KeywordToken] UBOUND,
	[KeywordFunction]
	[KeywordToken("UCASE$")] UCASE,
	[KeywordToken] UEVENT,
	[KeywordToken] UNLOCK,
	[KeywordToken] UNTIL,
	[KeywordToken] UPDATE,
	[KeywordToken] USING,
	[KeywordFunction]
	[KeywordToken] VAL,
	[KeywordFunction]
	[KeywordToken] VARPTR,
	[KeywordFunction]
	[KeywordToken("VARPTR$")] VARPTR_s,
	[KeywordFunction]
	[KeywordToken] VARSEG,
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
