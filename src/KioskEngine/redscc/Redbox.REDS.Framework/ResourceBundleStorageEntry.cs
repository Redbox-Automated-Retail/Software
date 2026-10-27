namespace Redbox.REDS.Framework;

internal class ResourceBundleStorageEntry : IResourceBundleStorageEntry
{
	public long Size { get; set; }

	public long Index { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }
}
