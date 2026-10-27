using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.DownloadService;

internal interface IDownloader
{
	DownloadData DownloadData { get; }

	ErrorList ProcessDownload();

	ErrorList SaveDownload();

	ErrorList DeleteDownload();

	ErrorList Complete();
}
