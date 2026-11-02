namespace Redbox.UpdateService.Client;

public class ChangeSetItem
{
	public bool IsFirst { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Revision { get; set; } = null!;

	public string VersionHash { get; set; } = null!;

	public string ContentHash { get; set; } = null!;

	public string TargetName { get; set; } = null!;

	public string TargetPath { get; set; } = null!;

	public bool Composite { get; set; }
}
