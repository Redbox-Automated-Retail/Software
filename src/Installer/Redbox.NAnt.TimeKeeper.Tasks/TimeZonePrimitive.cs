using System;

namespace Redbox.NAnt.TimeKeeper.Tasks;

public class TimeZonePrimitive
{
	public virtual TimeZone CurrentTimeZone => TimeZone.CurrentTimeZone;

	public virtual bool IsDaylightSavingTime()
	{
		return TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now);
	}
}
