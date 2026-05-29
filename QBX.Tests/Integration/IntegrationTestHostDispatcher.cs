using System.Threading.Channels;

namespace QBX.Tests.Integration;

class IntegrationTestHostDispatcher : IDispatcher
{
	public static IntegrationTestHostDispatcher Start(CancellationToken cancellationToken)
	{
		var dispatcher = new IntegrationTestHostDispatcher();

		dispatcher.StartDispatcherThread(cancellationToken);

		return dispatcher;
	}

	ChannelReader<Action> _queueReader;
	ChannelWriter<Action> _queueWriter;

	private IntegrationTestHostDispatcher()
	{
		var queue = Channel.CreateUnbounded<Action>();

		_queueReader = queue.Reader;
		_queueWriter = queue.Writer;
	}

	public void Dispatch(Action action)
	{
		_queueWriter.WriteAsync(action);
	}

	void StartDispatcherThread(CancellationToken cancellationToken)
	{
		Task.Run(() => DispatcherThread(cancellationToken));
	}

	void DispatcherThread(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			var readResult = _queueReader.ReadAsync(cancellationToken);

			var action = readResult.AsTask().Result;

			action();
		}
	}
}
