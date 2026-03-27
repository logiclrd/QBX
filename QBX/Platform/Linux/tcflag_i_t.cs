using System;

namespace QBX.Platform.Linux;

[Flags]
public enum tcflag_i_t
{
	IgnoreBreakOnInput = 1, // IGNBRK
	InterruptOnBreak = 2, // BRKINT
	IgnoreFramingAndParityErrors = 4, // IGNPAR
	MarkFramingAndParityErrors = 8, // PARMRK
	EnableInputParityChecking = 0x10, // INPCK
	StripHighBit = 0x20, // ISTRIP
	TranslateInputNewLineToCarriageReturn = 0x40, // INLCR
	IgnoreInputCarriageReturn = 0x80, // IGNCR
	TranslateInputCarriageReturnToNewLine = 0x100, // ICRNL
	InputLowerCase = 0x200, // IUCLC
	EnableOutputXONXOFF = 0x400, // IXON
	ResumeStoppedOutputOnAny = 0x800, // IXANY
	EnableInputXONXOFF = 0x1000, // IXOFF
	BellOnInputQueueFull = 0x2000, // IMAXBEL
	InputUTF8 = 0x4800, // IUTF8
}

