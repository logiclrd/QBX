using QBX.OperatingSystem.Memory;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class VarPtrFunction : VariableAddressFunction
{
	protected override ushort GetAddressPart(SegmentedAddress address)
		=> address.Offset;
}
