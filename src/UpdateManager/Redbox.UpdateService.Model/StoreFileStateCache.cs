using System;

namespace Redbox.UpdateService.Model;

public class StoreFileStateCache
{
	public long StoreFileStateId { get; set; }

	public long StoreFile { get; set; }

	public long Store { get; set; }

	public long StoreFileData { get; set; }

	public int SyncId { get; set; }

	public DateTime ReportedOn { get; set; }

	public string StoreNumber { get; set; }
}
