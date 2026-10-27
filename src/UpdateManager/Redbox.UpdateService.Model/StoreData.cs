using System;

namespace Redbox.UpdateService.Model;

public class StoreData
{
	public byte? EndOfScheduleInHoursFromMidnight;

	public byte? StartOfScheduleInHoursFromMidnight;

	public decimal? MaxBandwidthWhileOutsideOfSchedule;

	public decimal? MaxBandwidthWhileWithInSchedule;

	public decimal? NoProgressTimeout;

	public long ID { get; set; }

	public string Number { get; set; }

	public DateTime? LastCheckIn { get; set; }

	public bool? Enabled { get; set; }

	public int? AllowedBandwidthInMb { get; set; }
}
