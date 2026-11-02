namespace Redbox.UpdateManager.FileSets;

internal class FileSetFileInfo
{
	public long FileId { get; set; }

	public long FileRevisionId { get; set; }

	public bool Exists { get; set; }

	public long PatchSize { get; set; }

	public long FileSize { get; set; }

	// The properties below are filled by the object initializer or the JSON reader.
	public string FileHash { get; set; } = null!;

	public string PatchHash { get; set; } = null!;

	public string Key { get; set; } = null!;

	public string PatchKey { get; set; } = null!;

	public string PatchUrl { get; set; } = null!;

	public string Url { get; set; } = null!;
}
