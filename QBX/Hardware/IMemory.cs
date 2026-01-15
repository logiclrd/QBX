namespace QBX.Hardware;

public interface IMemory
{
	public int Length { get; }
	byte this[int index] { get; set; }
}
