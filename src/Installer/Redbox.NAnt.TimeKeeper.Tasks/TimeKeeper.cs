using System;
using NAnt.Core;
using Redbox.NAnt.Core;
using Redbox.TimeKeeperGateway;

namespace Redbox.NAnt.TimeKeeper.Tasks;

public class TimeKeeper
{
	private ITimeKeeperService _service;

	private long _kioskId;

	private TimeZonePrimitive _timeZone;

	private ITaskLogger _logger;

	public TimeKeeper(long kioskId, int timeOut, string url, ITaskLogger logger)
		: this(kioskId, new TimeKeeperService(url, timeOut), new TimeZonePrimitive(), logger)
	{
	}

	public TimeKeeper(long kioskId, ITimeKeeperService service, TimeZonePrimitive timeZone, ITaskLogger logger)
	{
		_service = service;
		_kioskId = kioskId;
		_timeZone = timeZone;
		_logger = logger;
	}

	public void Report()
	{
		try
		{
			string text = DateTimePrimitive.GetNow().ToString();
			_logger.Log(Level.Info, $"Reporting kiosk time - KioskId: {_kioskId} Time: {text} TimeZone: {_timeZone.CurrentTimeZone.StandardName} IsDayLightSavingTime: {_timeZone.IsDaylightSavingTime()}");
			_service.ReportTime(_kioskId, text, _timeZone.CurrentTimeZone.StandardName, _timeZone.IsDaylightSavingTime());
		}
		catch (Exception ex)
		{
			_logger.Log(Level.Error, ex.ToString());
		}
	}
}
