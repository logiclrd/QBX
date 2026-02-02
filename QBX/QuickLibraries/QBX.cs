using QBX.ExecutionEngine;
using QBX.Hardware;
using QBX.Interrupts;

namespace QBX.QuickLibraries;

[QuickLibraryName("QBX.QLB")]
public class QBX(Machine machine) : QuickLibrary
{
	[Export]
	public void Interrupt(short intnum, Registers inreg, out Registers outreg)
	{
		var interrupt = machine.InterruptHandlers[intnum];

		if (interrupt != null)
			outreg = interrupt.Execute(inreg);
		else
			outreg = inreg;
	}

	[Export]
	public void InterruptX(short intnum, RegistersEx inreg, out RegistersEx outreg)
	{
		var interrupt = machine.InterruptHandlers[intnum];

		if (interrupt != null)
			outreg = interrupt.Execute(inreg).AsRegistersEx();
		else
			outreg = inreg;
	}

	[Export]
	public void Absolute(short address)
	{
		throw RuntimeException.IllegalFunctionCall();
	}

	[Export]
	public void Int86Old(short intnum, ushort[] inarray, out ushort[] outarray)
	{
		var registers = new Registers();

		registers.AX = inarray[0];
		registers.BX = inarray[1];
		registers.CX = inarray[2];
		registers.DX = inarray[3];
		registers.BP = inarray[4];
		registers.SI = inarray[5];
		registers.DI = inarray[6];
		registers.FLAGS = (Flags)inarray[7];

		Interrupt(intnum, registers, out registers);

		outarray = new ushort[8];

		outarray[0] = registers.AX;
		outarray[1] = registers.BX;
		outarray[2] = registers.CX;
		outarray[3] = registers.DX;
		outarray[4] = registers.BP;
		outarray[5] = registers.SI;
		outarray[6] = registers.DI;
		outarray[7] = unchecked((ushort)registers.FLAGS);
	}

	[Export]
	public void Int86XOld(short intnum, ushort[] inarray, out ushort[] outarray)
	{
		var registers = new RegistersEx();

		registers.AX = inarray[0];
		registers.BX = inarray[1];
		registers.CX = inarray[2];
		registers.DX = inarray[3];
		registers.BP = inarray[4];
		registers.SI = inarray[5];
		registers.DI = inarray[6];
		registers.FLAGS = (Flags)inarray[7];
		registers.ES = inarray[8];
		registers.DS = inarray[9];

		InterruptX(intnum, registers, out registers);

		outarray = new ushort[10];

		outarray[0] = registers.AX;
		outarray[1] = registers.BX;
		outarray[2] = registers.CX;
		outarray[3] = registers.DX;
		outarray[4] = registers.BP;
		outarray[5] = registers.SI;
		outarray[6] = registers.DI;
		outarray[7] = unchecked((ushort)registers.FLAGS);

		if (registers is RegistersEx ex)
		{
			outarray[8] = ex.ES;
			outarray[9] = ex.DS;
		}
		else
		{
			outarray[8] = inarray[8];
			outarray[9] = inarray[9];
		}
	}
}
