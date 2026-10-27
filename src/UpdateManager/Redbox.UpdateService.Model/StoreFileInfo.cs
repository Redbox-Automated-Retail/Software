using System;

namespace Redbox.UpdateService.Model;

public class StoreFileInfo
{
	public long StoreFile { get; set; }

	public long StoreFileData { get; set; }

	public long Store { get; set; }

	public string StoreNumber { get; set; }

	public long KioskId { get; set; }

	public int SyncId { get; set; }

	public string SyncHash { get; set; }

	public string DataHash { get; set; }

	public DateTime ModifiedOn { get; set; }
}
