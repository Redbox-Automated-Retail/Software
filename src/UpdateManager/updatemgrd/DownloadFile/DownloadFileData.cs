using System;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.DownloadFile;

internal class DownloadFileData : IDownloadFileData
{
	// The properties below are filled by the object initializer or the JSON reader.
	public string ActivateScript { get; set; } = null!;

	public string DestinationPath { get; set; } = null!;

	public string EndTime { get; set; } = null!;

	public string FileKey { get; set; } = null!;

	public string FileName { get; set; } = null!;

	public long Id { get; set; }

	public string Name { get; set; } = null!;

	public string StartTime { get; set; } = null!;

	public string StatusKey { get; set; } = null!;

	public string Url { get; set; } = null!;

	public Guid FileGuid { get; set; }

	public DownloadFileDataState DownloadFileDataState { get; set; }

	public DownloadFileDataType DownloadFileDataType { get; set; }

	public Guid BitsGuid { get; set; }

	public string Message { get; set; } = null!; // IDownloadFileData declares it non-null; set on an error

	public int RetryCount { get; set; }
}
