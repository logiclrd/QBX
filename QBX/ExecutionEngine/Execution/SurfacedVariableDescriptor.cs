using System.Runtime.InteropServices;

namespace QBX.ExecutionEngine.Execution;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
public struct SurfacedVariableDescriptor
{
	public SurfacedVariableType Type; // informational
	public ushort Key;
}
