using System;
using System.Collections.Generic;
using System.Text;

namespace QBX.Utility;

public static class IEnumerableExtensions
{
	public static int Product(this IEnumerable<int> source)
	{
		int product = 1;

		foreach (var item in source)
			product *= item;

		return product;
	}
}
