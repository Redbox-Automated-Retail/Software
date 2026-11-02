using System;

namespace Redbox.UpdateManager.DownloadService;

internal class DownloadData
{
	// The properties below are filled by the object initializer or the JSON reader.
	public string Key { get; set; } = null!;

	public string Url { get; set; } = null!;

	public string Hash { get; set; } = null!;

	public string Path { get; set; } = null!;

	public DownloadState DownloadState { get; set; }

	public DownloadType DownloadType { get; set; }

	public DownloadPriority DownloadPriority { get; set; }

	public Guid FileGuid { get; set; }

	public Guid BitsGuid { get; set; }

	public string? Message { get; set; }

	public int RetryCount { get; set; }
}
