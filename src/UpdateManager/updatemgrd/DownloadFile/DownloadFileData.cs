using System;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.DownloadFile;

internal class DownloadFileData : IDownloadFileData
{
	public string ActivateScript { get; set; }

	public string DestinationPath { get; set; }

	public string EndTime { get; set; }

	public string FileKey { get; set; }

	public string FileName { get; set; }

	public long Id { get; set; }

	public string Name { get; set; }

	public string StartTime { get; set; }

	public string StatusKey { get; set; }

	public string Url { get; set; }

	public Guid FileGuid { get; set; }

	public DownloadFileDataState DownloadFileDataState { get; set; }

	public DownloadFileDataType DownloadFileDataType { get; set; }

	public Guid BitsGuid { get; set; }

	public string Message { get; set; }

	public int RetryCount { get; set; }
}
