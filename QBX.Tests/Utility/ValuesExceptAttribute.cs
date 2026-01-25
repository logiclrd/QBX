using System.Collections;

using NUnit.Framework.Interfaces;

namespace QBX.Tests.Utility;

[AttributeUsage(AttributeTargets.Parameter)]
public class ValuesExceptAttribute : NUnitAttribute, IParameterDataSource
{
	private readonly object[] exceptValues;

	public ValuesExceptAttribute(object exceptValue)
	{
		exceptValues = new[] { exceptValue };
	}

	public ValuesExceptAttribute(params object[] exceptValues)
	{
		this.exceptValues = exceptValues;
	}

	public IEnumerable GetData(IParameterInfo parameter)
	{
		return new ValuesAttribute()
				.GetData(parameter)
				.Cast<object>()
				.Except(exceptValues);
	}
}
