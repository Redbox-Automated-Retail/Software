using System;
using Redbox.TimeKeeperGateway;

namespace Redbox.NAnt.TimeKeeper.Tasks;

public class TimeKeeperService : Remoting, ITimeKeeperService
{
	private string _url;

	private int _timeOut;

	public TimeKeeperService(string url, int timeOut)
	{
		_url = url;
		_timeOut = timeOut;
	}

	public void ReportTime(long kioskId, string currentKioskTime, string currentKioskTimeZone, bool isDaylightSavingsTime)
	{
		ActivateTimeKeeperExecution().ReportTime(kioskId, currentKioskTime, currentKioskTimeZone, isDaylightSavingsTime);
	}

	private ITimeKeeperService ActivateTimeKeeperExecution()
	{
		base.Initialize(_timeOut);
		return (ITimeKeeperService)Activator.GetObject(typeof(ITimeKeeperService), _url);
	}
}
