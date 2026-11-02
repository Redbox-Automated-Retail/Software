namespace Redbox.UpdateManager.FileSets;

internal class FileData
{
	public long FileId { get; set; }

	public long FileRevisionId { get; set; }

	public long Size { get; set; }

	// The properties below are filled by the object initializer or the JSON reader.
	public string Hash { get; set; } = null!;

	public string StagePath { get; set; } = null!;

	public string FileDestination { get; set; } = null!;
}
