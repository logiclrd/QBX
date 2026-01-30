namespace QBX.Interrupts;

public enum Flags : ushort
{
	None = 0,

	Carry           = 0b00000000_00000001,
	Parity          = 0b00000000_00000100,
	AuxCarry        = 0b00000000_00010000,
	Zero            = 0b00000000_01000000,
	Sign            = 0b00000000_10000000,
	Trap            = 0b00000001_00000000,
	InterruptEnable = 0b00000010_00000000,
	Direction       = 0b00000100_00000000,
	Overflow        = 0b00001000_00000000,

	IOPrivilegeLevel3 = 0b00110000_00000000,
	IOPrivilegeLevel2 = 0b00100000_00000000,
	IOPrivilegeLevel1 = 0b00010000_00000000,
	IOPrivilegeLevel0 = 0b00000000_00000000,

	IOPrivilegeLevelMask = 0b00110000_00000000,

	NestedTask      = 0b01000000_00000000,
	NECMode         = 0b10000000_00000000,
}
