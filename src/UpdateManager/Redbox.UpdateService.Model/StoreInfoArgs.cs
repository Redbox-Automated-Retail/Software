namespace Redbox.UpdateService.Model;

public class StoreInfoArgs
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber { get; set; } = null!;

	public ClientStoreInfo ClientStoreInfo { get; set; } = null!;

	public int ClientStoreInfoSyncID { get; set; }
}
