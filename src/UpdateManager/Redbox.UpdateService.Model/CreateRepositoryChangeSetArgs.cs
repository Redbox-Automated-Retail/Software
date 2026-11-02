namespace Redbox.UpdateService.Model;

public class CreateRepositoryChangeSetArgs
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string SourceRevision { get; set; } = null!;

	public string TargetRevision { get; set; } = null!;

	public long RepositoryOid { get; set; }

	public string CurrentLabel { get; set; } = null!;
}
