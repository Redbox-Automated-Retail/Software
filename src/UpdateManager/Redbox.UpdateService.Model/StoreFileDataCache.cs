namespace Redbox.UpdateService.Model;

public class StoreFileDataCache
{
	public long StoreFile { get; set; }

	public long StoreFileData { get; set; }

	public long Store { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber { get; set; } = null!;

	public int SyncId { get; set; }
}
