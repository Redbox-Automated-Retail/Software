using System;

namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetRevisionChangeSet
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

	public DateTime DownloadOn { get; set; }

	public DownloadPriority DownloadPriority { get; set; }

	public DateTime ActiveOn { get; set; }

	public string ActivateStartTime { get; set; }

	public string ActivateEndTime { get; set; }

	public string DownloadUrl { get; set; }

	public FileSetAction Action { get; set; }
}
