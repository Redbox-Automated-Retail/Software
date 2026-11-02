using System;

namespace Redbox.UpdateService.Model;

public class StoreFileInfo
{
	public long StoreFile { get; set; }

	public long StoreFileData { get; set; }

	public long Store { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber { get; set; } = null!;

	public long KioskId { get; set; }

	public int SyncId { get; set; }

	public string SyncHash { get; set; } = null!;

	public string DataHash { get; set; } = null!;

	public DateTime ModifiedOn { get; set; }
}
