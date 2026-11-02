namespace Redbox.UpdateManager.FileSets;

internal class FileSetRevisionChangeSet
{
	public long FileSetId { get; set; }

	public long RevisionId { get; set; }

	// The properties below are filled by the object initializer or the JSON reader.
	public string Path { get; set; } = null!;

	public int CompressionType { get; set; }

	public string ContentHash { get; set; } = null!;

	public string FileHash { get; set; } = null!;

	public long ContentSize { get; set; }

	public long FileSize { get; set; }

	public long PatchRevisionId { get; set; }

	public ClientFileSetRevisionChangeSet ToClient()
	{
		return new ClientFileSetRevisionChangeSet
		{
			FileSetId = FileSetId,
			RevisionId = RevisionId,
			Path = Path,
			CompressionType = CompressionType,
			FileHash = FileHash,
			ContentHash = ContentHash,
			PatchRevisionId = PatchRevisionId,
			ContentSize = ContentSize,
			FileSize = FileSize
		};
	}

	public FileSetRevisionChangeSet Clone()
	{
		return new FileSetRevisionChangeSet
		{
			FileSetId = FileSetId,
			RevisionId = RevisionId,
			Path = Path,
			CompressionType = CompressionType,
			ContentHash = ContentHash,
			FileHash = FileHash,
			PatchRevisionId = PatchRevisionId,
			ContentSize = ContentSize,
			FileSize = FileSize
		};
	}
}
