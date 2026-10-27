namespace UpdateClientService.API.Services.FileSets;

public class FileSetRevisionChangeSet
{
	public long FileSetId { get; set; }

	public long RevisionId { get; set; }

	public string Path { get; set; }

	public int CompressionType { get; set; }

	public string ContentHash { get; set; }

	public string FileHash { get; set; }

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
