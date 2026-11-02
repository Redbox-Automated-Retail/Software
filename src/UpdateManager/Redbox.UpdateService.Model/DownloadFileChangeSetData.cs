namespace Redbox.UpdateService.Model;

public class DownloadFileChangeSetData
{
	public long Id { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Name { get; set; } = null!;

	public DownloadFileType DownloadFileType { get; set; }

	public string StatusKey { get; set; } = null!;

	public string FileName { get; set; } = null!;

	public string DestinationPath { get; set; } = null!;

	public string FileKey { get; set; } = null!;

	public string ActivateScript { get; set; } = null!;

	public string StartTime { get; set; } = null!;

	public string EndTime { get; set; } = null!;

	public string Url { get; set; } = null!;
}
