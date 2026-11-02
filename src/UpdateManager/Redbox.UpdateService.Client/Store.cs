using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Store
{
	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Number { get; set; } = null!;

	public Identifier Group { get; set; } = null!;

	public uint MaxBandwidthWhileOutsideOfSchedule { get; set; }

	public uint MaxBandwidthWhileWithInSchedule { get; set; }

	public byte StartOfScheduleInHoursFromMidnight { get; set; }

	public byte EndOfScheduleInHoursFromMidnight { get; set; }

	public int AllowedBandwidthInMB { get; set; }

	public uint NoProgressTimeout { get; set; }

	public DateTime LastCheckIn { get; set; }

	public bool Enabled { get; set; }

	public List<Identifier> Repositories { get; set; } = null!;

	public List<Identifier> Tags { get; set; } = null!;

	public Identifier Node { get; set; } = null!;

	public Store()
	{
		NoProgressTimeout = 0u;
	}
}
