using Microsoft.Win32.SafeHandles;
using System;

namespace QBX.LexicalAnalysis;

public class KeywordFunctionAttribute : Attribute
{
	public readonly int MinimumParameterCount;
	public readonly int MaximumParameterCount;
	public readonly int FileNumberParameter;
	public readonly bool IsAssignable;

	public KeywordFunctionAttribute(int parameterCount = 1, int minimumParameterCount = -1, int maximumParameterCount = -1, int fileNumberParameter = -1, bool isAssignable = false)
	{
		int Default(int optionalCount) => optionalCount == -1 ? parameterCount : optionalCount;

		MinimumParameterCount = Math.Min(Default(minimumParameterCount), Default(maximumParameterCount));
		MaximumParameterCount = Math.Max(Default(minimumParameterCount), Default(maximumParameterCount));
		FileNumberParameter = fileNumberParameter;
		IsAssignable = isAssignable;
	}

	public bool TakesParameters => MaximumParameterCount > 0;
	public bool TakesNoParameters => MinimumParameterCount == 0;
}
