using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

using QBX.Hardware;

using Timer = System.Threading.Timer;

namespace QBX.ExecutionEngine.Execution.Events;

public class EventHub
{
	public readonly EventConfiguration Configuration = new EventConfiguration();
	public readonly Queue<Event> Events = new Queue<Event>();

	public readonly HashSet<Event> HeldEvents = new HashSet<Event>();

	IDispatcher _dispatcher;

	Lock _eventsLock = new Lock();

	public volatile bool HaveEvents;

	bool _suspendAllEvents;

	class SuspendAllEventsScope(EventHub owner) : IDisposable
	{
		public void Dispose() => owner._suspendAllEvents = false;
	}

	public IDisposable SuspendAllEvents()
	{
		_suspendAllEvents = true;
		return new SuspendAllEventsScope(this);
	}

	Timer _timerEventGenerator;

	public EventHub(IDispatcher dispatcher)
	{
		_dispatcher = dispatcher;

		_timerEventGenerator = new Timer(TimerEventGeneratorCallback);

		Configuration.ReleaseEvents +=
			evt =>
			{
				if ((evt.Type == EventType.Key) && (evt.Source == 0))
					ReleaseAllKeyEvents();
				else
					ReleaseEvent(evt);
			};
	}

	void TimerEventGeneratorCallback(object? state)
	{
		var evt = new Event(EventType.Timer);

		if (Configuration.IsEnabledFor(evt) != EventEnabledState.On)
			_timerEventGenerator.Change(Timeout.Infinite, Timeout.Infinite);

		PostEvent(evt);
	}

	Semaphore _configurationInFlight = new(initialCount: 0, maximumCount: int.MaxValue);

	public void DispatchConfigurationChange(EventConfigurationChange change)
	{
		_dispatcher.Dispatch(
			() =>
			{
				ApplyConfigurationChange(change);
				_configurationInFlight.Release();
			});

		_configurationInFlight.WaitOne();
	}

	public void ApplyConfigurationChange(EventConfigurationChange change)
	{
		Configuration.ApplyChange(change);

		if ((Configuration.TimerEnabled == EventEnabledState.Off)
		 || (Configuration.TimerInterval < 1))
			_timerEventGenerator.Change(Timeout.Infinite, Timeout.Infinite);
		else
		{
			var interval = TimeSpan.FromSeconds(Configuration.TimerInterval);

			_timerEventGenerator.Change(interval, interval);
		}
	}

	public bool PostKeyEvent(KeyEvent keyEvent)
	{
		var evt = Configuration.GenerateKeyEvent(keyEvent);

		if ((evt == default)
		 || (Configuration.IsEnabledFor(evt) == EventEnabledState.Off))
			return false;

		PostEvent(evt);
		return true;
	}

	public void PostEvent(EventType type)
		=> PostEvent(new Event(type));

	public void PostEvent(Event evt)
	{
		if (_suspendAllEvents)
		{
			lock (_eventsLock)
				HeldEvents.Add(evt);
		}
		else
		{
			switch (Configuration.IsEnabledFor(evt))
			{
				case EventEnabledState.Stop:
					lock (_eventsLock)
						HeldEvents.Add(evt);
					break;
				case EventEnabledState.On:
					lock (_eventsLock)
					{
						HaveEvents = true;
						Events.Enqueue(evt);
					}
					break;
			}
		}
	}

	public void ReleaseEvent(Event evt)
	{
		lock (_eventsLock)
		{
			if (HeldEvents.Remove(evt))
				PostEvent(evt);
		}
	}

	public void ReleaseAllKeyEvents()
	{
		lock (_eventsLock)
		{
			foreach (int keyNumber in KeyEventKeyNumber.AllKeyNumbers)
			{
				var evt = new Event(EventType.Key, keyNumber);

				if (HeldEvents.Remove(evt))
					PostEvent(evt);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetEvent([NotNullWhen(true)] out Event evt)
	{
		lock (_eventsLock)
		{
			if (Events.Count == 1)
				HaveEvents = false;

			return Events.TryDequeue(out evt);
		}
	}
}

