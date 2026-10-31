using System;

namespace Redbox.TimeKeeper;

public class InsertKioskTimeLogCommand : DatabaseCommand
{
	public InsertKioskTimeLogCommand(long kioskID, DateTime currentTime, string currentTimeZone, bool isDaylightSavings)
		: base("tmp_InsertKioskTimeLog")
	{
		AddParameter("@KioskID", kioskID);
		AddParameter("@ReportedTime", currentTime);
		AddParameter("@ReportedTimeZone", currentTimeZone);
		AddParameter("@IsDaylightSavings", isDaylightSavings);
	}
}
