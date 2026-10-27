using System.Xml;

namespace Redbox.REDS.Framework;

public class XmlAspectType : AspectType
{
	protected override void OnParseAspect(IResourceBundle bundle, IAspect aspect, XmlNode aspectNode, ErrorList errors)
	{
		aspect.SetContent(aspectNode.Clone());
	}
}
