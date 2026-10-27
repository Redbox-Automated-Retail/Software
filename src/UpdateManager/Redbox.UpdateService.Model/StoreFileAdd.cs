namespace Redbox.UpdateService.Model;

public class StoreFileAdd
{
	public long Store { get; set; }

	public string StoreNumber { get; set; }

	public long KioskId { get; set; }

	public string SyncHash { get; set; }

	public byte[] Data { get; set; }

	public string DataHash { get; set; }
}
