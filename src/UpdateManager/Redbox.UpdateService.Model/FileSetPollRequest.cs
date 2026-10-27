namespace Redbox.UpdateService.Model;

public class FileSetPollRequest
{
	public long FileSetId { get; set; }

	public long FileSetRevisionId { get; set; }

	public FileSetState FileSetState { get; set; }
}
