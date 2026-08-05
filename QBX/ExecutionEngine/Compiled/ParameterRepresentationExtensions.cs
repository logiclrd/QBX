using System;

namespace QBX.ExecutionEngine.Compiled;

public static class ParameterRepresentationExtensions
{
	public static ParameterRepresentation ToExecutionEngineType(this CodeModel.ParameterRepresentation codeModelRepresentation)
	{
		return
			codeModelRepresentation switch
			{
				CodeModel.ParameterRepresentation.Standard => ParameterRepresentation.Pointer,
				CodeModel.ParameterRepresentation.SEG => ParameterRepresentation.FarPointer,
				CodeModel.ParameterRepresentation.BYVAL => ParameterRepresentation.Value,
				_ => throw new Exception("Internal error")
			};
	}
}
