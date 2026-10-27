using System;

namespace Redbox.UpdateManager.FileSets;

internal class RevisionChangeSet
{
	public long FileSetId { get; set; }

	public long RevisionId { get; set; }

	public DateTime? Received { get; set; }

	public DateTime? Downloaded { get; set; }

	public DateTime? Staged { get; set; }

	public DateTime? Activated { get; set; }

	public ChangesetState State { get; set; }

	public string Message { get; set; }

	public int RetryCount { get; set; }

	public string Path { get; set; }

	public int CompressionType { get; set; }

	public string ContentHash { get; set; }

	public string FileHash { get; set; }

	public long PatchRevisionId { get; set; }

	public DateTime DownloadOn { get; set; }

	public DownloadPriority DownloadPriority { get; set; }

	public DateTime ActiveOn { get; set; }

	public string ActivateStartTime { get; set; }

	public string ActivateEndTime { get; set; }

	public string DownloadUrl { get; set; }

	public string StateText => Enum.GetName(typeof(ChangesetState), State);
}
