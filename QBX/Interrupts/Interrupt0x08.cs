using QBX.Hardware;

namespace QBX.Interrupts;

public class Interrupt0x08(Machine machine) : InterruptHandler
{
	public override Registers Execute(Registers input)
	{
		machine.Timer.Timer0.BumpIntervals();

		return input;
	}
}
