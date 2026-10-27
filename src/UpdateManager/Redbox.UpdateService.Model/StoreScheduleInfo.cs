namespace Redbox.UpdateService.Model;

public class StoreScheduleInfo
{
	public byte EndOfScheduleInHoursFromMidnight;

	public byte StartOfScheduleInHoursFromMidnight;

	public int MaxBandwidthWhileOutsideOfSchedule;

	public int MaxBandwidthWhileWithInSchedule;

	public uint NoProgressTimeout;
}
