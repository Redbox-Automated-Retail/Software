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

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Number { get; set; } = null!;

	public DateTime? LastCheckIn { get; set; }

	public bool? Enabled { get; set; }

	public int? AllowedBandwidthInMb { get; set; }
}
