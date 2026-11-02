namespace Redbox.UpdateService.Model;

public class PollReply
{
	public PollReplyType PollReplyType { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string CacheKey { get; set; } = null!;

	public int SyncId { get; set; }

	public string Data { get; set; } = null!;
}
