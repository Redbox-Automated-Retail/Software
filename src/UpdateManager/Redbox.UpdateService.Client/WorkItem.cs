using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class WorkItem
{
	public long ID { get; set; }

	public long ScriptId { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public List<Store> Targets { get; set; } = null!;

	public DateTime EffectiveDate { get; set; }

	public WorkItemRecurrence Recurrence { get; set; }
}
