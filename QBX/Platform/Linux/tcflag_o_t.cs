using System;

namespace QBX.Platform.Linux;

[Flags]
public enum tcflag_o_t
{
	EnableOutputPostProcessing = 1, // OPOST
	OutputUpperCase = 2, // OLCUC
	TranslateOutputNewLineToCRNL = 4, // ONLCR
	TranslateOutputCarriageReturnToNewLine = 8, // OCRNL
	OutputNoCarriageReturnAtLineStart = 0x10, // ONOCR
	OutputNewLineImpliesCarriageReturn = 0x20, // ONLRET
	DelayWithFillCharacters = 0x40, // OFILL
	FillCharacterIsDelete = 0x80, // OFDEL

	NewLineDelayMask = 1 << 8, // NLDLY
	NewLineDelay_0 = 0 << 8, // NL0
	NewLineDelay_1 = 1 << 8, // NL1

	CarriageReturnDelayMask = 3 << 9, // CRDLY
	CarriageReturnDelay_0 = 0 << 9, // CR0
	CarriageReturnDelay_1 = 1 << 9, // CR1
	CarriageReturnDelay_2 = 2 << 9, // CR2
	CarriageReturnDelay_3 = 3 << 9, // CR3

	HoriontalTabDelayMask = 3 << 11, // TABDLY
	HorizontalTabDelay_0 = 0 << 11, // TAB0
	HorizontalTabDelay_1 = 1 << 11, // TAB1
	HorizontalTabDelay_2 = 2 << 11, // TAB2
	HorizontalTabDelay_3 = 3 << 11, // TAB3

	BackspaceDelayMask = 1 << 13, // BSDLY
	BackspaceDelay0 = 0 << 13, // BS0
	BackspaceDelay1 = 1 << 13, // BS1

	VerticalTabDelayMask = 1 << 14, // VTDLY
	VertictalTabDelay_0 = 0 << 14, // VT0
	VertictalTabDelay_1 = 1 << 14, // VT1

	FormFeedDelayMask = 1 << 15, // FFDLY
	FormFeedDelayMask_0 = 0 << 15, // FF0
	FormFeedDelayMask_1 = 1 << 15, // FF1
}

