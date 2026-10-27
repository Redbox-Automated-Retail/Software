using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class StoreGroup
{
	public long ID { get; set; }

	public string Name { get; set; }

	public uint MaxBandwidthWhileOutsideOfSchedule { get; set; }

	public uint MaxBandwidthWhileWithInSchedule { get; set; }

	public byte StartOfScheduleInHoursFromMidnight { get; set; }

	public byte EndOfScheduleInHoursFromMidnight { get; set; }

	public int AllowedBandwidthInMB { get; set; }

	public List<Repository> Repositories { get; set; }

	public int StoreCount { get; set; }
}
