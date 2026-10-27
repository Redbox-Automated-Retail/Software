using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetState
{
	public long FileSetId { get; set; }

	public long RevisionId { get; set; }

	public FileSetState FileSetState { get; set; }
}
