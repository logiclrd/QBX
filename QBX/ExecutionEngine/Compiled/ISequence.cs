namespace QBX.ExecutionEngine.Compiled;

public interface ISequence
{
	int Count { get; }
	IExecutable this[int index] { get; }

	void Append(IExecutable executable);
	void Prepend(IExecutable executable);
}
