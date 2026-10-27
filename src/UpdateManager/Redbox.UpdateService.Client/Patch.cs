namespace Redbox.UpdateService.Client;

public class Patch
{
	public long ID { get; set; }

	public int Size { get; set; }

	public string VersionHash { get; set; }

	public string ContentHash { get; set; }

	public long VersionLength { get; set; }

	public string FileHash { get; set; }

	public string Revision { get; set; }

	public Identifier File { get; set; }
}
