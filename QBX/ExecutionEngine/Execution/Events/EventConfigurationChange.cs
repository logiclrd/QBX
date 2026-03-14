using System.Threading;

namespace QBX.ExecutionEngine.Execution.Events;

public class EventConfigurationChange
{
	public Event Key { get; }
	public EventEnabledState? Enable { get; }
	public KeyEventKeyDefinition? CustomKeyDefinition { get; }
	public int ParameterValue { get; }

	ManualResetEvent _processed = new ManualResetEvent(initialState: false);

	public void WaitUntilProcessed() => _processed.WaitOne();
	public void MarkProcessed() => _processed.Set();

	EventConfigurationChange(Event key, EventEnabledState? enable, KeyEventKeyDefinition? customKeyDefinition, int parameterValue)
	{
		Key = key;
		Enable = enable;
		CustomKeyDefinition = CustomKeyDefinition;
		ParameterValue = parameterValue;
	}

	public static EventConfigurationChange EnableDisableKey(int source, EventEnabledState enable)
		=> new EventConfigurationChange(new Event(EventType.Key, source), enable, customKeyDefinition: null, parameterValue: -1);

	public static EventConfigurationChange ConfigureKey(int source, KeyEventKeyDefinition keyDefinition)
		=> new EventConfigurationChange(new Event(EventType.Key, source), enable: null, keyDefinition, parameterValue: -1);

	public static EventConfigurationChange EnableDisablePen(EventEnabledState enable)
		=> new EventConfigurationChange(new Event(EventType.Pen), enable, customKeyDefinition: null, parameterValue: -1);

	public static EventConfigurationChange EnableDisablePlay(EventEnabledState enable)
		=> new EventConfigurationChange(new Event(EventType.Play), enable, customKeyDefinition: null, parameterValue: -1);

	public static EventConfigurationChange ConfigurePlay(int playQueueTriggerLength)
		=> new EventConfigurationChange(new Event(EventType.Play), enable: null, customKeyDefinition: null, parameterValue: playQueueTriggerLength);

	public static EventConfigurationChange EnableDisableTimer(EventEnabledState enable)
		=> new EventConfigurationChange(new Event(EventType.Timer), enable, customKeyDefinition: null, parameterValue: -1);

	public static EventConfigurationChange ConfigureTimer(int interval)
		=> new EventConfigurationChange(new Event(EventType.Timer), enable: null, customKeyDefinition: null, parameterValue: interval);

	public static EventConfigurationChange EnableDisableUserEvents(EventEnabledState enable)
		=> new EventConfigurationChange(new Event(EventType.UserEvent), enable, customKeyDefinition: null, parameterValue: -1);
}

