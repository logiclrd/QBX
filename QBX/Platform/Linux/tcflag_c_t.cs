using System;

namespace QBX.Platform.Linux;

[Flags]
public enum tcflag_c_t
{
	BaudSpeedMask = 0b1111 | BaudSpeedMaskExtra, // CBAUD
	BaudSpeedMaskExtra = 1 << 12, // CBAUDEX
	CharacterSizeMask = 3 << 4, // CSIZE
	CharacterSize_5 = 0 << 4, // CS5
	CharacterSize_6 = 1 << 4, // CS6
	CharacterSize_7 = 2 << 4, // CS7
	CharacterSize_8 = 3 << 4, // CS8
	UseTwoStopBits = 0x40, // CSTOPB
	EnableReceiver = 0x80, // CREAD
	EnableParity = 0x100, // PARENB
	UseOddParity = 0x200, // PARODD
	HangUpAfterClose = 0x400, // HUPCL
	IgnoreModeControl = 0x800, // CLOCAL

	InputBaudSpeedShift = 16, // IBSHIFT
	InputBaudSpeedMask = BaudSpeedMask << InputBaudSpeedShift, // CIBAUD

	UseMarkSpaceParity = 0x40000000, // CMSPAR
	EnableRTSCTS = unchecked((int)0x80000000), // CRTSCTS
}

