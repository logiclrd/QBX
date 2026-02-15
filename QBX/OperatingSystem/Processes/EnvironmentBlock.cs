using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;

namespace QBX.OperatingSystem.Processes;

public class EnvironmentBlock : Dictionary<string, string>
{
	public static EnvironmentBlock FromAmbientEnvironment()
	{
		var ret = new EnvironmentBlock();

		foreach (System.Collections.DictionaryEntry variable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
		{
			string variableName = variable.Key?.ToString() ?? string.Empty;
			string value = variable.Value?.ToString() ?? string.Empty;

			ret[variableName] = value;
		}

		return ret;
	}

	public StringValue Encode()
	{
		var buffer = new StringValue();

		EncodeTo(buffer);

		return buffer;
	}

	public void EncodeTo(StringValue buffer)
	{
		foreach (var variable in this)
			buffer.Append(variable.Key).Append('=').Append(variable.Value ?? "").Append(0);

		buffer.Append(0);
	}

	public void Decode(IMemory systemMemory, int address, DOS context)
	{
		Clear();
		DecodeTo(systemMemory, address, context, this);
	}

	public static void DecodeTo(IMemory systemMemory, int address, DOS context, IDictionary<string, string> environment)
	{
		while (true)
		{
			var keyValuePair = context.ReadStringZ(systemMemory, address);

			if (context.LastError != DOSError.None)
				break;

			if (keyValuePair.Length == 0)
				break;

			int separator = keyValuePair.IndexOf((byte)'=');

			string variableName;
			string value;

			if (separator < 0)
			{
				variableName = keyValuePair.ToString();
				value = "";
			}
			else
			{
				variableName = keyValuePair.ToString(0, separator);
				value = keyValuePair.ToString(separator + 1);
			}

			environment[variableName] = value;
		}
	}
}
