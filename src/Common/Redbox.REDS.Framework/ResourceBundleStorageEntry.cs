namespace Redbox.REDS.Framework;

internal class ResourceBundleStorageEntry : IResourceBundleStorageEntry
{
	public long Size { get; set; }

	public long Index { get; set; }

	public string Name { get; set; } = null!; // set by the storage's object initializer

	public string? Path { get; set; } // only for file-system storage
}
