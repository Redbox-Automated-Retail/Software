namespace Redbox.UpdateManager.ComponentModel;

public enum DownloadFileDataState
{
	None,
	Error,
	PendingDownload,
	Downloading,
	PostDownload,
	PendingInstall,
	Installing,
	PostInstall,
	Complete
}
