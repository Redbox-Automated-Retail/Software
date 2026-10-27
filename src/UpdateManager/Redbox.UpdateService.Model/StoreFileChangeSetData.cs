using System;

namespace Redbox.UpdateService.Model;

public class StoreFileChangeSetData
{
	public long StoreFile { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public long StoreFileData { get; set; }

	public int SyncId { get; set; }

	public string SyncHash { get; set; }

	public string DataHash { get; set; }

	public string Base64Data { get; set; }

	public DateTime ModifiedOn { get; set; }

	public StoreFileAction Action { get; set; }
}
