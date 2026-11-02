namespace Redbox.UpdateService.Client;

public class StatusMessage
{
	public long OID;

	// The fields below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber = null!;

	public string Key = null!;

	public string SubKey = null!;

	public string Type = null!;

	public string Description = null!;

	public string Data = null!;

	public string KioskTimeStamp = null!;

	public string CreatedOn = null!;
}
