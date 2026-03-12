using System;
using System.IO;

using QBX.LexicalAnalysis;
using QBX.OperatingSystem;

namespace QBX.ExecutionEngine;

[Serializable]
public class RuntimeException : Exception
{
	public int ErrorNumber { get; }

	public int LineNumber { get; }

	public Token? Context { get; }
	public int ContextLength { get; }

	public static int LastLineNumber;

	public RuntimeException(string message, int errorNumber = -1)
		: this(default(Token), message, errorNumber)
	{
	}

	public RuntimeException(CodeModel.Statements.Statement? statement, string message, int errorNumber = -1)
		: this(
				statement?.FirstToken,
				statement?.SourceLength ?? 0,
				message,
				errorNumber)
	{
	}

	public RuntimeException(CodeModel.Expressions.Expression? expression, string message, int errorNumber = -1)
		: this(
				expression?.Token,
				expression?.Token?.Length ?? 0,
				message,
				errorNumber)
	{
	}

	RuntimeException(Token? context, string message, int errorNumber = -1)
		: this(context, context?.Length ?? 0, message, errorNumber)
	{
	}

	public RuntimeException(Token? context, int contextLength, string message, int errorNumber = -1)
		: this(context, contextLength, message, errorNumber, LastLineNumber)
	{
	}

	RuntimeException(Token? context, int contextLength, string message, int errorNumber, int lineNumber)
		: base(message)
	{
		Context = context;
		ContextLength = contextLength;

		ErrorNumber = errorNumber;

		LineNumber = lineNumber;
	}

	public RuntimeException AddContext(CodeModel.Statements.Statement? statement)
		=> AddContext(statement?.FirstToken, statement?.FirstToken?.Length ?? 0);
	public RuntimeException AddContext(CodeModel.Expressions.Expression? expression)
		=> AddContext(expression?.Token, expression?.Token?.Length ?? 0);

	RuntimeException AddContext(Token? context, int contextLength)
	{
		if (this.Context != null)
			return this;
		else
		{
			return new RuntimeException(
				context,
				contextLength,
				Message,
				ErrorNumber,
				LineNumber);
		}
	}

	public static string GetErrorMessage(int errorNumber)
	{
		string message = "Unprintable error";

		if ((errorNumber >= 0) && (errorNumber < s_messageByErrorNumber.Length))
			message = s_messageByErrorNumber[errorNumber] ?? message;

		return message;
	}

	public static RuntimeException ForErrorNumber(int errorNumber, CodeModel.Statements.Statement? statement)
		=> new RuntimeException(statement, GetErrorMessage(errorNumber), errorNumber);

	public static RuntimeException ForErrorNumber(int errorNumber, CodeModel.Expressions.Expression? expression)
		=> new RuntimeException(expression, GetErrorMessage(errorNumber), errorNumber);

	public static RuntimeException ForErrorNumber(int errorNumber, Token? context)
		=> new RuntimeException(context, GetErrorMessage(errorNumber), errorNumber);

	// Some of these we can't/don't generate because the associated analysis is
	// attached to the compile phase, not runtime. But the user could still
	// trigger them explicitly with ERROR statements.
	static string?[] s_messageByErrorNumber =
		[
			/* 0 */ null,
			/* 1 */ "NEXT without FOR",
			/* 2 */ "Syntax error",
			/* 3 */ "RETURN without GOSUB",
			/* 4 */ "Out of DATA",
			/* 5 */ "Illegal function call",
			/* 6 */ "Overflow",
			/* 7 */ "Out of memory",
			/* 8 */ "Label not defined",
			/* 9 */ "Subscript out of range",
			/* 10 */ "Duplicate definition",
			/* 11 */ "Division by zero",
			/* 12 */ "Illegal in direct mode",
			/* 13 */ "Type mismatch",
			/* 14 */ "Out of string space",
			/* 15 */ null, // was "String too long" in GWBASIC
			/* 16 */ "String formula too complex",
			/* 17 */ "Cannot continue",
			/* 18 */ "Function not defined",
			/* 19 */ "No RESUME",
			/* 20 */ "RESUME without error",
			/* 21 */ null,
			/* 22 */ null, // was "Missing operand" in GWBASIC
			/* 23 */ null, // was "Line buffer overflow" in GWBASIC
			/* 24 */ "Device timeout",
			/* 25 */ "Device fault",
			/* 26 */ "FOR without NEXT",
			/* 27 */ "Out of paper",
			/* 28 */ null,
			/* 29 */ "WHILE without WEND",
			/* 30 */ "WEND without WHILE",
			/* 31 */ null,
			/* 32 */ null,
			/* 33 */ "Duplicate label",
			/* 34 */ null,
			/* 35 */ "Subprogram not defined",
			/* 36 */ null,
			/* 37 */ "Argument-count mismatch",
			/* 38 */ "Array not defined",
			/* 39 */ null,
			/* 40 */ "Variable required",
			/* 41-49 */ null, null, null, null, null, null, null, null, null,
			/* 50 */ "FIELD overflow",
			/* 51 */ "Internal error",
			/* 52 */ "Bad file name or number",
			/* 53 */ "File not found",
			/* 54 */ "Bad file mode",
			/* 55 */ "File already open",
			/* 56 */ "FIELD statement active",
			/* 57 */ "Device I/O error",
			/* 58 */ "File already exists",
			/* 59 */ "Bad record length",
			/* 60 */ null,
			/* 61 */ "Disk full",
			/* 62 */ "Input past end of file",
			/* 63 */ "Bad record number",
			/* 64 */ "Bad file name",
			/* 65 */ null,
			/* 66 */ null, // was "Direct statement in file" in GWBASIC
			/* 67 */ "Too many files",
			/* 68 */ "Device unavailable",
			/* 69 */ "Communication-buffer overflow",
			/* 70 */ "Permission denied",
			/* 71 */ "Disk not ready",
			/* 72 */ "Disk-media error",
			/* 73 */ "Feature unavailable",
			/* 74 */ "Rename across disks",
			/* 75 */ "Path/File access error",
			/* 76 */ "Path not found",
			/* 77-79 */ null, null, null,
			/* 80 */ "Feature removed",
			/* 81 */ "Invalid name",
			/* 82 */ "Table not found",
			/* 83 */ "Index not found",
			/* 84 */ "Invalid column",
			/* 85 */ "No current record",
			/* 86 */ "Duplicate value for unique index",
			/* 87 */ "Invalid operation on null index",
			/* 88 */ "Database needs repair",
			/* 89 */ "Insufficient ISAM buffers",
			/* 90 */ "",
			/* 91 */ "", // unused range
			/* 92 */ "",
			/* 93 */ "",
			/* 94 */ "",
			/* 95 */ "",
			/* 96 */ "",
			/* 97 */ "",
			/* 98 */ "",
			/* 99 */ "",
			/* 100 */ "", // QBX custom errors below
			/* 101 */ "Argument count mismatch",
		];

	public static RuntimeException SyntaxError(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(2, expression);
	public static RuntimeException ReturnWithoutGoSub(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(3, statement);
	public static RuntimeException OutOfData(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(4, statement);
	public static RuntimeException IllegalFunctionCall(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(5, expression);
	public static RuntimeException IllegalFunctionCall()
		=> ForErrorNumber(5, default(Token));
	public static RuntimeException IllegalFunctionCall(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(5, statement);
	public static RuntimeException Overflow(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(6, expression);
	public static RuntimeException Overflow(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(6, statement);
	public static RuntimeException Overflow(Token? context)
		=> ForErrorNumber(6, context);
	public static RuntimeException OutOfMemory(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(7, statement);
	public static RuntimeException SubscriptOutOfRange(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(9, expression);
	public static RuntimeException SubscriptOutOfRange()
		=> ForErrorNumber(9, default(Token));
	public static RuntimeException DuplicateDefinition(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(10, statement);
	public static RuntimeException DivisionByZero(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(11, expression);
	public static RuntimeException TypeMismatch(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(13, statement);
	public static RuntimeException TypeMismatch(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(13, expression);
	public static RuntimeException TypeMismatch()
		=> ForErrorNumber(13, default(Token?));
	public static RuntimeException NoResume(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(19, statement);
	public static RuntimeException ResumeWithoutError(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(20, statement);
	public static RuntimeException VariableRequired(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(40, statement);
	public static RuntimeException FieldOverflow()
		=> ForErrorNumber(50, default(Token));
	public static RuntimeException BadFileNameOrNumber(Token? context)
		=> ForErrorNumber(52, context);
	public static RuntimeException BadFileNameOrNumber(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(52, statement);
	public static RuntimeException FileNotFound(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(53, statement);
	public static RuntimeException BadFileMode(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(54, statement);
	public static RuntimeException BadFileMode(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(54, expression);
	public static RuntimeException FileAlreadyOpen(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(55, statement);
	public static RuntimeException DeviceIOError(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(57, statement);
	public static RuntimeException FileAlreadyExists(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(58, statement);
	public static RuntimeException BadRecordLength(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(59, statement);
	public static RuntimeException DiskFull(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(61, statement);
	public static RuntimeException InputPastEndOfFile(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(62, statement);
	public static RuntimeException InputPastEndOfFile(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(62, expression);
	public static RuntimeException BadRecordNumber(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(63, statement);
	public static RuntimeException BadRecordNumber(Token? context)
		=> ForErrorNumber(63, context);
	public static RuntimeException BadFileName()
		=> ForErrorNumber(64, default(Token));
	public static RuntimeException TooManyFiles(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(67, statement);
	public static RuntimeException TooManyFiles(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(67, expression);
	public static RuntimeException DeviceUnavailable()
		=> ForErrorNumber(68, default(Token));
	public static RuntimeException PathFileAccessError(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(75, statement);
	public static RuntimeException PathNotFound()
		=> ForErrorNumber(76, default(Token));
	public static RuntimeException PathNotFound(CodeModel.Statements.Statement? statement)
		=> ForErrorNumber(76, statement);
	public static RuntimeException PathNotFound(CodeModel.Expressions.Expression? expression)
		=> ForErrorNumber(76, expression);
	public static RuntimeException ArgumentCountMismatch()
		=> ForErrorNumber(101, default(Token));

	public static RuntimeException ForDOSError(DOSError dosError, CodeModel.Expressions.Expression? expression)
		=> ForDOSError(dosError, statement: null).AddContext(expression);

	public static RuntimeException ForDOSError(DOSError dosError, CodeModel.Statements.Statement? statement)
	{
		switch (dosError)
		{
			case DOSError.InvalidFunction: return IllegalFunctionCall(statement);
			case DOSError.FileNotFound: return FileNotFound(statement);
			case DOSError.PathNotFound: return PathNotFound(statement);
			case DOSError.TooManyOpenFiles: return TooManyFiles(statement);
			case DOSError.InvalidAccess: return PathFileAccessError(statement);
			case DOSError.NotEnoughMemory: return OutOfMemory(statement);
			case DOSError.AccessDenied: return PathFileAccessError(statement);
			case DOSError.CurrentDirectory: return PathNotFound(statement);
			case DOSError.FileExists: return FileAlreadyExists(statement);
			case DOSError.CRC: return DeviceIOError(statement);
			case DOSError.SectorNotFound: return DeviceIOError(statement);
			case DOSError.WriteFault: return DeviceIOError(statement);
			case DOSError.ReadFault: return DeviceIOError(statement);
			case DOSError.AdapterHardwareError: return DeviceIOError(statement);
			case DOSError.HandleDiskFull: return DiskFull(statement);

			case DOSError.InvalidHandle:
			case DOSError.ArenaTrashed:
			case DOSError.InvalidBlock:
			case DOSError.BadEnvironment:
			case DOSError.BadFormat:
			case DOSError.InvalidData:
			case DOSError.InvalidDrive:
			case DOSError.NotSameDevice:
			case DOSError.NoMoreFiles:
			case DOSError.WriteProtect:
			case DOSError.BadUnit:
			case DOSError.NotReady:
			case DOSError.BadCommand:
			case DOSError.BadLength:
			case DOSError.Seek:
			case DOSError.NotDOSDisk:
			case DOSError.OutOfPaper:
			case DOSError.GeneralFailure:
			case DOSError.SharingViolation:
			case DOSError.LockViolation:
			case DOSError.WrongDisk:
			case DOSError.FCBUnavailable:
			case DOSError.SharingBufferExceeded:
			case DOSError.CodePageMismatched:
			case DOSError.HandleEOF:
			case DOSError.NotSupported:
			case DOSError.RemoteNotListed:
			case DOSError.DuplicateName:
			case DOSError.BadNetworkPath:
			case DOSError.NetworkBusy:
			case DOSError.DeviceDoesNotExist:
			case DOSError.TooManyCommands:
			case DOSError.BadNetworkResponse:
			case DOSError.UnexpectedNetworkError:
			case DOSError.BadRemoteAdapter:
			case DOSError.PrintQueueFull:
			case DOSError.NoSpoolSpace:
			case DOSError.PrintCancelled:
			case DOSError.NetworkNameDeleted:
			case DOSError.NetworkAccessDenied:
			case DOSError.BadDeviceType:
			case DOSError.BadNetworkName:
			case DOSError.TooManyNames:
			case DOSError.TooManySessions:
			case DOSError.SharingPaused:
			case DOSError.RequestNotAccepted:
			case DOSError.RedirectionPaused:
			case DOSError.DuplicateFCB:
			case DOSError.CannotMakeDirectory:
			case DOSError.FailureOnINT24:
			case DOSError.OutOfStructures:
			case DOSError.AlreadyAssigned:
			case DOSError.InvalidPassword:
			case DOSError.InvalidParameter:
			case DOSError.NetworkWriteFault:
			case DOSError.SystemComponentNotLoaded:
			default:
				return IllegalFunctionCall(statement);
		}
	}

	public static RuntimeException ForIOException(IOException exception, CodeModel.Statements.Statement? statement = null)
		=> ForDOSError(exception.ToDOSError(), statement);
}
