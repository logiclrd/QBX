namespace QBX.Tests.Utility;

public static class RandomExtensions
{
	public static T Next<T>(this Random rnd, IList<T> domain)
		=> domain[rnd.Next(domain.Count)];
}
