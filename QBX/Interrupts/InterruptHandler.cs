namespace QBX.Interrupts;

public abstract class InterruptHandler
{
	public abstract Registers Execute(Registers input);
}
