namespace Redbox.UpdateService.Model;

public class InterServerTask
{
	public InterServerTaskType Type { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Args { get; set; } = null!;
}
