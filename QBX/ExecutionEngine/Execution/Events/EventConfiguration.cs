using System;
using System.Collections.Generic;

using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution.Events;

public class EventConfiguration
{
	public EventEnabledState[] KeyEnabled = new EventEnabledState[32];
	public KeyEventKeyDefinition[] CustomKeys = new KeyEventKeyDefinition[11];

	public const int FirstCustomKey = 15;
	public const int LastCustomKey = 25;

	public EventEnabledState PenEnabled;

	public EventEnabledState PlayEnabled;
	public int PlayQueueTriggerLength;

	public EventEnabledState TimerEnabled;
	public int TimerInterval;

	public EventEnabledState UserEventEnabled;

	public event Action<Event>? ReleaseEvents;

	Dictionary<KeyEventKeyDefinition, int> _customKeysByKey = new();

	public int GetCustomKey(KeyEventKeyDefinition key)
	{
		_customKeysByKey.TryGetValue(key, out var customKey);

		return customKey;
	}

	public void ApplyChange(EventConfigurationChange change)
	{
		var key = change.Key;

		switch (key.Type)
		{
			case EventType.Key:
				if ((key.Source < 0) || (key.Source >= KeyEnabled.Length))
					break;

				if (change.Enable != null)
				{
					if (key.Source == 0)
						KeyEnabled.AsSpan().Fill(change.Enable.Value);
					else
						KeyEnabled[key.Source] = change.Enable.Value;
				}

				if ((change.CustomKeyDefinition is KeyEventKeyDefinition newMapping)
				 && (newMapping != default)
				 && (key.Source >= FirstCustomKey)
				 && (key.Source <= LastCustomKey))
				{
					// Saturate shift bits
					if ((newMapping.Modifiers & KeyEventKeyModifiers.Shift) != 0)
						newMapping.Modifiers |= KeyEventKeyModifiers.Shift;

					if (CustomKeys[key.Source] is KeyEventKeyDefinition existingMapping)
						_customKeysByKey.Remove(existingMapping);
					if (_customKeysByKey.TryGetValue(newMapping, out var existingKey))
						CustomKeys[existingKey] = default;

					CustomKeys[key.Source] = newMapping;
					_customKeysByKey[newMapping] = key.Source;
				}

				break;
			case EventType.Pen:
				if (change.Enable != null)
					PenEnabled = change.Enable.Value;

				break;
			case EventType.Play:
				if (change.Enable != null)
					PlayEnabled = change.Enable.Value;

				if (change.ParameterValue >= 0)
					PlayQueueTriggerLength = change.ParameterValue;

				break;
			case EventType.Timer:
				if (change.Enable != null)
					TimerEnabled = change.Enable.Value;

				if (change.ParameterValue >= 0)
					TimerInterval = change.ParameterValue;

				break;
			case EventType.UserEvent:
				if (change.Enable != null)
					UserEventEnabled = change.Enable.Value;

				break;
		}

		if (change.Enable.HasValue
		 && (change.Enable.Value == EventEnabledState.On))
			ReleaseEvents?.Invoke(key);
	}

	public Event GenerateKeyEvent(KeyEvent rawInput)
	{
		int keyNumber = KeyEventKeyNumber.ForKeyEvent(rawInput, this);

		if (keyNumber > 0)
			return new Event(EventType.Key, keyNumber);
		else
			return default;
	}

	public EventEnabledState IsEnabledFor(Event evt)
	{
		switch (evt.Type)
		{
			case EventType.Key:
				if (KeyEventKeyNumber.IsDefined(evt.Source))
					return KeyEnabled[evt.Source];
				else
					return EventEnabledState.Off;

			case EventType.Pen: return PenEnabled;
			case EventType.Play: return PlayEnabled;
			case EventType.Timer: return TimerEnabled;
			case EventType.UserEvent: return UserEventEnabled;
		}

		return EventEnabledState.Off;
	}
}

