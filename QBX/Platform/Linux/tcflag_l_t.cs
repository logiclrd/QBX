using System;

namespace QBX.Platform.Linux;

[Flags]
public enum tcflag_l_t
{
	SignalOnSignalCharacters = 1, // ISIG
	CanonicalMode = 2, // ICANON
	EscapedUpperCaseMode = 4, // XCASE
	EchoInputCharacters = 8, // ECHO
	EnableEraseCharacters = 0x10, // ECHOE
	EnableKillLineCharacter = 0x20, // ECHOK
	AlwaysEchoNewLine = 0x40, // ECHONL
	DoNotFlushQueuesOnSignals = 0x80, // NOFLSH
	TranslateStoppedOutputToSignal = 0x100, // TOSTOP
	DisplayControlCharactersWithCaret = 0x200, // ECHOCTL
	PrintCharactersOnErase = 0x400, // ECHOPRT
	KillLineByErasingAllCharacters = 0x800, // ECHOKE
	OutputFlushInProgress = 0x1000, // FLUSHO
	ReprintInputQueueOnRead = 0x4000, // PENDIN
	EnableInputPreProcessing = 0x8000, // IEXTEN
}

