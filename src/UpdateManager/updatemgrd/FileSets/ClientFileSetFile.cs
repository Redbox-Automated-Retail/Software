namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetFile
{
	public long FileId { get; set; }

	// The properties below are filled by the object initializer or the JSON reader.
	public string Name { get; set; } = null!;

	public string FileDestination { get; set; } = null!;

	public long FileRevisionId { get; set; }

	public string ContentHash { get; set; } = null!;

	public string Path { get; set; } = null!;

	public int CompressionType { get; set; }

	public string FileHash { get; set; } = null!;

	public long BlobId { get; set; }

	public long ContentSize { get; set; }

	public long FileSize { get; set; }
}
