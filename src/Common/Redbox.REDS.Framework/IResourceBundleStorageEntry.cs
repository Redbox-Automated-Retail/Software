namespace Redbox.REDS.Framework;

public interface IResourceBundleStorageEntry
{
	long Size { get; }

	long Index { get; }

	string Name { get; }

	string Path { get; }
}
