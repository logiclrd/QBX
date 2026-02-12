using System;

namespace QBX.OperatingSystem;

public class SystemClock
{
	long _bias;

	public DateTime Now => new DateTime(DateTime.Now.Ticks + _bias, DateTimeKind.Local);

	public void SetCurrentTime(DateTime time)
	{
		_bias = (time - DateTime.Now).Ticks;
	}
}
