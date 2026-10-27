namespace Redbox.HAL.Component.Model;

public interface IConfigurationService
{
	void LoadAndImport(ErrorList errors);

	void LoadAndUpgrade(ErrorList errors);

	void Load(ErrorList errors);

	void Save(ErrorList errors);

	string FormatAsXml(string key);

	void UpdateFromXml(string key, string data, ErrorList errors);

	void RegisterConfiguration(string name, IAttributeXmlConfiguration config);

	object GetPropertyByName(string key, string name);

	void SetPropertyByName(string key, string name, object[] value);

	IAttributeXmlConfiguration FindConfiguration(string name);

	IAttributeXmlConfiguration FindConfiguration(Configurations configuration);
}
