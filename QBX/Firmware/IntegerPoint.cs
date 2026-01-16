using QBX.CodeModel.Expressions;
using QBX.ExecutionEngine.Compiled.Functions;

namespace QBX.Firmware;

public struct IntegerPoint
{
	public int X, Y;

	public IntegerPoint(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static implicit operator IntegerPoint((int, int) tuple)
		=> new IntegerPoint(tuple.Item1, tuple.Item2);
}
