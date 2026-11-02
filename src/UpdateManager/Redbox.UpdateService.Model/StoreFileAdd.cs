namespace Redbox.UpdateService.Model;

public class StoreFileAdd
{
	public long Store { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber { get; set; } = null!;

	public long KioskId { get; set; }

	public string SyncHash { get; set; } = null!;

	public byte[] Data { get; set; } = null!;

	public string DataHash { get; set; } = null!;
}
