namespace Redbox.UpdateService.Client;

public class Patch
{
	public long ID { get; set; }

	public int Size { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string VersionHash { get; set; } = null!;

	public string ContentHash { get; set; } = null!;

	public long VersionLength { get; set; }

	public string FileHash { get; set; } = null!;

	public string Revision { get; set; } = null!;

	public Identifier File { get; set; } = null!;
}
