using System.Text;

namespace Redbox.REDS.Framework;

public class TextContentLoader : IAspectContentLoader
{
	internal object Value { get; set; }

	internal IResourceBundle Bundle { get; set; }

	public object GetContent()
	{
		if (Value is IResourceBundleStorageEntry)
		{
			return Encoding.UTF8.GetString(Bundle.Storage.GetEntryContent((IResourceBundleStorageEntry)Value));
		}
		return Value;
	}
}
