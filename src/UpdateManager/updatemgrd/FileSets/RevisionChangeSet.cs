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

	public string? Message { get; set; }

	public int RetryCount { get; set; }

	// The properties below are filled by the object initializer or the JSON reader.
	public string Path { get; set; } = null!;

	public int CompressionType { get; set; }

	public string ContentHash { get; set; } = null!;

	public string FileHash { get; set; } = null!;

	public long PatchRevisionId { get; set; }

	public DateTime DownloadOn { get; set; }

	public DownloadPriority DownloadPriority { get; set; }

	public DateTime ActiveOn { get; set; }

	public string ActivateStartTime { get; set; } = null!;

	public string ActivateEndTime { get; set; } = null!;

	public string DownloadUrl { get; set; } = null!;

	public string StateText => Enum.GetName(typeof(ChangesetState), State)!; // State is a ChangesetState
}
