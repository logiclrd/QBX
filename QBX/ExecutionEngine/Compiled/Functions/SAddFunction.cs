using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem.Memory;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class SAddFunction : StringAddressFunction
{
	protected override Variable CreateResult(SegmentedAddress address)
		=> new IntegerVariable(address.Offset);
}
