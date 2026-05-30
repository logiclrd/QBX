using System;

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine.Compiled.Functions;

public static class PointFunctionFactory
{
	public static Function Construct(CodeModel.Expressions.KeywordFunctionExpression expression)
	{
		if (expression.Function != TokenType.POINT)
			throw new Exception("Internal error: PointFunctionFactory.Construct called on non-POINT keyword: " + expression.Function);

		switch (expression.Arguments?.Count ?? 0)
		{
			case 1: return new CoordinatePointFunction();
			case 2: return new PixelPointFunction();

			default:
				throw new Exception("Internal error: PointFunctionFactory.Construct called on a function with an unrecognized argument count");
		}
	}
}
