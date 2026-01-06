using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace QBX.CodeModel.Expressions;

public static class OperatorExtensions
{
	static Dictionary<Operator, int> s_precedenceByOperator =
		typeof(Operator).GetFields(BindingFlags.Static | BindingFlags.Public)
		.Select(field =>
			(
				Operator: (Operator)field.GetValue(null)!,
				Precedence: field.GetCustomAttribute<PrecedenceAttribute>()!.Precedence
			))
		.ToDictionary(key => key.Operator, value => value.Precedence);

	public static int GetPrecedence(this Operator op) => s_precedenceByOperator[op];
}
