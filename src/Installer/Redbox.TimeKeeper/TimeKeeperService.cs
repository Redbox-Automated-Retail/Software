using System;
using System.Configuration;
using Redbox.TimeKeeperGateway;

namespace Redbox.TimeKeeper;

public class TimeKeeperService : MarshalByRefObject, ITimeKeeperService
{
	private IDatabase _db;

	public TimeKeeperService()
		: this(new Database(ConfigurationSettings.AppSettings.Get("ConnectionString")))
	{
	}

	public TimeKeeperService(IDatabase db)
	{
		_db = db;
	}

	public void ReportTime(long kioskId, string currentKioskTime, string currentKioskTimeZone, bool isDaylightSavingsTime)
	{
		try
		{
			InsertKioskTimeLogCommand insertKioskTimeLogCommand = new InsertKioskTimeLogCommand(kioskId, DateTime.Parse(currentKioskTime), currentKioskTimeZone, isDaylightSavingsTime);
			using (insertKioskTimeLogCommand)
			{
				_db.Modify(insertKioskTimeLogCommand);
			}
		}
		catch (Exception ex)
		{
			new RedboxLogger().Write("Error inserting kiosk time log", ex);
		}
	}
}
