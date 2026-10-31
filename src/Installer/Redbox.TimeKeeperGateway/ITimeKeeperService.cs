namespace Redbox.TimeKeeperGateway;

public interface ITimeKeeperService
{
	void ReportTime(long kioskId, string currentKioskTime, string currentKioskTimeZone, bool isDaylightSavingsTime);
}
