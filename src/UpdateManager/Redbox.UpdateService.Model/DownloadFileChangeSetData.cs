namespace Redbox.UpdateService.Model;

public class DownloadFileChangeSetData
{
	public long Id { get; set; }

	public string Name { get; set; }

	public DownloadFileType DownloadFileType { get; set; }

	public string StatusKey { get; set; }

	public string FileName { get; set; }

	public string DestinationPath { get; set; }

	public string FileKey { get; set; }

	public string ActivateScript { get; set; }

	public string StartTime { get; set; }

	public string EndTime { get; set; }

	public string Url { get; set; }
}
