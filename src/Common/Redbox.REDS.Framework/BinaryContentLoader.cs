namespace Redbox.REDS.Framework;

public class BinaryContentLoader : IAspectContentLoader
{
	internal object Value { get; set; }

	internal IResourceBundle Bundle { get; set; }

	public object GetContent()
	{
		if (Value is IResourceBundleStorageEntry)
		{
			return Bundle.Storage.GetEntryContent((IResourceBundleStorageEntry)Value);
		}
		return Value;
	}
}
