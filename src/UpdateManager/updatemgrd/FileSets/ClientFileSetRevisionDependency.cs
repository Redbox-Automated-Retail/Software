namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetRevisionDependency
{
	public long FileSetRevisionDependencyId { get; set; }

	public long DependsOnFileSetId { get; set; }

	public DependencyType DependencyType { get; set; }

	public string MinimumVersion { get; set; }

	public string MaximumVersion { get; set; }
}
