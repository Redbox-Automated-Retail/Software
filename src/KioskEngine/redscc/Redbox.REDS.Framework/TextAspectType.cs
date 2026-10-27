using System.Xml;

namespace Redbox.REDS.Framework;

public class TextAspectType : AspectType
{
	protected override void OnParseAspect(IResourceBundle bundle, IAspect aspect, XmlNode aspectNode, ErrorList errors)
	{
		if (aspect.Properties.ContainsKey("file"))
		{
			aspect.SetContent(new TextContentLoader
			{
				Bundle = bundle,
				Value = bundle.GetEntry(aspect.Properties["file"])
			});
		}
		else
		{
			aspect.SetContent(aspectNode.InnerText);
		}
	}
}
