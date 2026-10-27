using System;
using Newtonsoft.Json;
using UpdateClientService.API.Services.DownloadService;

namespace UpdateClientService.API.Services.FileSets;

public class RevisionChangeSet : RevisionChangeSetKey
{
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

	public DateTime DownloadOn { get; set; }

	public DownloadPriority DownloadPriority { get; set; }

	public DateTime ActiveOn { get; set; }

	public string ActivateStartTime { get; set; }

	public string ActivateEndTime { get; set; }

	public string DownloadUrl { get; set; }

	public string StateText => Enum.GetName(typeof(ChangesetState), State);

	[JsonIgnore]
	public bool IsStaged
	{
		get
		{
			if (State != ChangesetState.Error)
			{
				return State >= ChangesetState.Staged;
			}
			return false;
		}
	}

	public static RevisionChangeSet Create(ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
	{
		return new RevisionChangeSet
		{
			FileSetId = clientFileSetRevisionChangeSet.FileSetId,
			RevisionId = clientFileSetRevisionChangeSet.RevisionId,
			Received = DateTime.Now,
			Downloaded = null,
			Staged = null,
			Activated = null,
			State = ChangesetState.Received,
			Message = string.Empty,
			RetryCount = 0,
			Path = clientFileSetRevisionChangeSet.Path,
			CompressionType = clientFileSetRevisionChangeSet.CompressionType,
			ContentHash = clientFileSetRevisionChangeSet.ContentHash,
			FileHash = clientFileSetRevisionChangeSet.FileHash,
			PatchRevisionId = clientFileSetRevisionChangeSet.PatchRevisionId,
			DownloadOn = clientFileSetRevisionChangeSet.DownloadOn.ToLocalTime(),
			DownloadPriority = clientFileSetRevisionChangeSet.DownloadPriority,
			ActiveOn = clientFileSetRevisionChangeSet.ActiveOn.ToLocalTime(),
			ActivateStartTime = clientFileSetRevisionChangeSet.ActivateStartTime,
			ActivateEndTime = clientFileSetRevisionChangeSet.ActivateEndTime,
			DownloadUrl = clientFileSetRevisionChangeSet.DownloadUrl
		};
	}
}
