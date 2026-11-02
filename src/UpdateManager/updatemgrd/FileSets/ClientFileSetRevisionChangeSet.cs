using System;

namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetRevisionChangeSet
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

	public DateTime DownloadOn { get; set; }

	public DownloadPriority DownloadPriority { get; set; }

	public DateTime ActiveOn { get; set; }

	public string ActivateStartTime { get; set; } = null!;

	public string ActivateEndTime { get; set; } = null!;

	public string DownloadUrl { get; set; } = null!;

	public FileSetAction Action { get; set; }
}
