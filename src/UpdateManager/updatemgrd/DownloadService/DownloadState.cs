namespace Redbox.UpdateManager.DownloadService;

internal enum DownloadState
{
	None,
	Error,
	Downloading,
	PostDownload,
	Complete
}
