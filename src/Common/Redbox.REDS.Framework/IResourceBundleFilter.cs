namespace Redbox.REDS.Framework;

public interface IResourceBundleFilter
{
	string this[string name] { get; set; }
}
