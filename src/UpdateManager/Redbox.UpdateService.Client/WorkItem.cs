using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class WorkItem
{
	public long ID { get; set; }

	public long ScriptId { get; set; }

	public List<Store> Targets { get; set; }

	public DateTime EffectiveDate { get; set; }

	public WorkItemRecurrence Recurrence { get; set; }
}
