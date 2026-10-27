using System;

namespace Redbox.UpdateManager.ComponentModel;

public interface IDownloadFileData
{
	string ActivateScript { get; set; }

	string DestinationPath { get; set; }

	string EndTime { get; set; }

	string FileKey { get; set; }

	string FileName { get; set; }

	long Id { get; set; }

	string Name { get; set; }

	string StartTime { get; set; }

	string StatusKey { get; set; }

	string Url { get; set; }

	DownloadFileDataState DownloadFileDataState { get; set; }

	DownloadFileDataType DownloadFileDataType { get; set; }

	Guid FileGuid { get; set; }

	Guid BitsGuid { get; set; }

	string Message { get; set; }

	int RetryCount { get; set; }
}
