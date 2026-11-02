namespace Redbox.UpdateService.Client;

public class Identifier
{
	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Reference { get; set; } = null!;
}
