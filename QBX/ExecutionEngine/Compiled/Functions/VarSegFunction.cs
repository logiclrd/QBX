using QBX.OperatingSystem.Memory;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class VarSegFunction : VariableAddressFunction
{
	protected override ushort GetAddressPart(SegmentedAddress address)
		=> address.Segment;
}
