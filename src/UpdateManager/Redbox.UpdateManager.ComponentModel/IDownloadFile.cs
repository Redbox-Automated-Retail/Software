namespace Redbox.UpdateManager.ComponentModel;

public interface IDownloadFile
{
	IDownloadFileData DownloadFileData { get; }

	ErrorList ProcessDownloadFile();

	ErrorList SaveDownloadFile();

	ErrorList DeleteDownloadFile();
}
