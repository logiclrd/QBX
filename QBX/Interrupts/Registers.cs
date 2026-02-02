using System;
using System.Diagnostics;

namespace QBX.Interrupts;

public class Registers
{
	public ushort AX;
	public ushort BX;
	public ushort CX;
	public ushort DX;
	public ushort BP;
	public ushort SI;
	public ushort DI;
	public Flags FLAGS;

	public RegistersEx AsRegistersEx()
	{
		if (this is not RegistersEx ex)
		{
			ex = new RegistersEx();

			ex.AX = this.AX;
			ex.BX = this.BX;
			ex.CX = this.CX;
			ex.DX = this.DX;
			ex.BP = this.BP;
			ex.SI = this.SI;
			ex.DI = this.DI;
			ex.FLAGS = this.FLAGS;
		}

		return ex;
	}
}
