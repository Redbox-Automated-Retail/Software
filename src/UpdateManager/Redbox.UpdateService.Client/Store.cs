using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Store
{
	public long ID { get; set; }

	public string Number { get; set; }

	public Identifier Group { get; set; }

	public uint MaxBandwidthWhileOutsideOfSchedule { get; set; }

	public uint MaxBandwidthWhileWithInSchedule { get; set; }

	public byte StartOfScheduleInHoursFromMidnight { get; set; }

	public byte EndOfScheduleInHoursFromMidnight { get; set; }

	public int AllowedBandwidthInMB { get; set; }

	public uint NoProgressTimeout { get; set; }

	public DateTime LastCheckIn { get; set; }

	public bool Enabled { get; set; }

	public List<Identifier> Repositories { get; set; }

	public List<Identifier> Tags { get; set; }

	public Identifier Node { get; set; }

	public Store()
	{
		NoProgressTimeout = 0u;
	}
}
