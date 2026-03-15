using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem.Memory;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class SSegAddFunction : StringAddressFunction
{
	public override DataType Type => DataType.Long;

	protected override Variable CreateResult(SegmentedAddress address)
		=> new LongVariable(address.ToFarPointer());
}
