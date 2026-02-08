using System;
using System.Threading;

namespace QBX.OperatingSystem.Breaks;

public class CancellationScope : IDisposable
{
	DOS _owner;
	CancellationTokenSource _cancellationTokenSource;
	bool _breakReceived;

	public CancellationToken Token => _cancellationTokenSource.Token;
	public bool BreakReceived => _breakReceived;

	public CancellationScope(DOS owner)
	{
		_cancellationTokenSource = new CancellationTokenSource();

		_owner = owner;
		_owner.Break += owner_Break;
	}

	public void Dispose()
	{
		_owner.Break -= owner_Break;
	}

	private void owner_Break()
	{
		_breakReceived = true;

		try
		{
			_cancellationTokenSource.Cancel();
		}
		catch { }
	}
}
