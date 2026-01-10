namespace QBX.ExecutionEngine.Compiled;

public interface ISequence
{
	int Count { get; }
	Executable this[int index] { get; }

	void Append(Executable executable);
	void Prepend(Executable executable);
}
