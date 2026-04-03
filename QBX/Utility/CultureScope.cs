using System;
using System.Globalization;
using System.Threading;

namespace QBX.Utility;

public class CultureScope : IDisposable
{
	CultureInfo _previousCulture;
	bool _isDisposed;

	public CultureScope()
	{
		var currentThread = Thread.CurrentThread;

		_previousCulture = currentThread.CurrentCulture;
	}

	public CultureScope(CultureInfo scopeCulture)
	{
		var currentThread = Thread.CurrentThread;

		_previousCulture = currentThread.CurrentCulture;

		currentThread.CurrentCulture = scopeCulture;
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			Thread.CurrentThread.CurrentCulture = _previousCulture;
			_isDisposed = true;
		}
	}
}
