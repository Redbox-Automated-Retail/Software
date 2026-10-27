namespace Redbox.UpdateService.Client;

public class ChangeSetItem
{
	public bool IsFirst { get; set; }

	public string Revision { get; set; }

	public string VersionHash { get; set; }

	public string ContentHash { get; set; }

	public string TargetName { get; set; }

	public string TargetPath { get; set; }

	public bool Composite { get; set; }
}
