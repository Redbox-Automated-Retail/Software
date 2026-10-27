using System.Xml;

namespace Redbox.REDS.Framework;

public interface IAspectType
{
	string Name { get; }

	string this[string name] { get; set; }

	ErrorList ParseAspect(IResourceBundle bundle, IAspect aspect, XmlNode aspectNode);
}
