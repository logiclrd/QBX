using System.Runtime.CompilerServices;

namespace QBX.Hardware.AdLib;

[InlineArray(length: (int)ControlRegister.RegisterCount)]
struct ControlRegisterData
{
	byte _element0;

	public void SetRegister(ControlRegister register, byte value)
	{
		if ((register < ControlRegister.First)
		 || (register > ControlRegister.Last))
			return;

		this[(int)register] = value;
	}

	public byte GetRegister(ControlRegister register)
	{
		if ((register < ControlRegister.First)
		 || (register > ControlRegister.Last))
			return 0;

		if (register == ControlRegister.ControlID)
		{
			// Special case
			return 0x71; // bits 0-3 model id 1 ("1000"), bits 4-6 extra board options (all 1 -- "not present")
		}
		else
			return this[(int)register];
	}
}
