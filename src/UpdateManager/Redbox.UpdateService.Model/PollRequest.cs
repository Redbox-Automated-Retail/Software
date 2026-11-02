namespace Redbox.UpdateService.Model;

public class PollRequest
{
	public PollRequestType PollRequestType { get; set; }

	public int SyncId { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Data { get; set; } = null!;
}
