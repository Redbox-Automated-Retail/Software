using System.Xml;

namespace Redbox.HAL.Component.Model;

public interface IXmlConfigurationElement
{
	void WriteElement(XmlWriter writer);
}
