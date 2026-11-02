namespace Redbox.UpdateService.Model;

public class StoreKioskClientMap
{
	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Number { get; set; } = null!;

	public long KioskClient_ID { get; set; }
}
