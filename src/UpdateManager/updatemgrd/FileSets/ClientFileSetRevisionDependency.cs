namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetRevisionDependency
{
	public long FileSetRevisionDependencyId { get; set; }

	public long DependsOnFileSetId { get; set; }

	public DependencyType DependencyType { get; set; }

	// The properties below are filled by the object initializer or the JSON reader.
	public string MinimumVersion { get; set; } = null!;

	public string MaximumVersion { get; set; } = null!;
}
