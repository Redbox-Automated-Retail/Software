namespace Redbox.UpdateService.Client;

public class Script
{
	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Name { get; set; } = null!;

	public string Text { get; set; } = null!;
}
