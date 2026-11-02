namespace Redbox.UpdateManager.FileSets;

internal class ClientPatchFileSetFile
{
	public long FileId { get; set; }

	public long PatchFileRevisionId { get; set; }

	public long FileRevisionId { get; set; }

	// The properties below are filled by the object initializer or the JSON reader.
	public string ContentHash { get; set; } = null!;

	public string Path { get; set; } = null!;

	public int CompressionType { get; set; }

	public string FileHash { get; set; } = null!;

	public long PatchBlobId { get; set; }

	public long ContentSize { get; set; }

	public long FileSize { get; set; }
}
