using System.Xml;

namespace Redbox.HAL.Component.Model;

public interface IAttributeXmlConfiguration : IConfigurationObserver
{
	string RootName { get; }

	void LoadProperties(XmlDocument document, ErrorList errors);

	void StoreProperties(XmlDocument document, ErrorList errors);

	void Import(ErrorList errors);

	void Upgrade(XmlDocument document, ErrorList errors);

	void AddObserver(IConfigurationObserver observer);

	void RemoveObserver(IConfigurationObserver observer);
}
