namespace QBX.ExecutionEngine.Execution;

public enum SurfacedVariableType : byte
{
	Unknown = 0,

	Integer = 2, // VT_I2
	Long = 20, // VT_I4
	Single = 4, // VT_R4
	Double = 8, // VT_R8
	Currency = 24, // VT_CY
	String = 3, // VT_SD
}
