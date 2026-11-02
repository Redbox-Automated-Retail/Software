using System;

namespace Redbox.UpdateService.Model;

public class StoreFileChangeSetData
{
	public long StoreFile { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Name { get; set; } = null!;

	public string Path { get; set; } = null!;

	public long StoreFileData { get; set; }

	public int SyncId { get; set; }

	public string SyncHash { get; set; } = null!;

	public string DataHash { get; set; } = null!;

	public string Base64Data { get; set; } = null!;

	public DateTime ModifiedOn { get; set; }

	public StoreFileAction Action { get; set; }
}
