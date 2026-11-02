using System.Xml;
using NAnt.Core;

namespace Redbox.Nant.Tasks;

public class ConfigFile
{
	public XmlDocument Xml;

	public string _filename;

	public Task? _parentTask;

	public ConfigFile(string filename)
	{
		_filename = filename;
		Xml = new XmlDocument();
	}

	public ConfigFile(string filename, Task parentTask)
		: this(filename)
	{
		_parentTask = parentTask;
	}

	public virtual void Load()
	{
		Xml.Load(_filename);
	}

	public virtual void AddElement(string key, string keyValue, bool preserve)
	{
		XmlNode xmlNode = Xml.SelectSingleNode($"/configuration/appSettings/add[@key='{key}']");
		if (xmlNode != null)
		{
			if (!preserve)
			{
				Log($"Updating key {key} with value {keyValue}");
				xmlNode.Attributes["key"].Value = key;
				xmlNode.Attributes["value"].Value = keyValue;
			}
			else
			{
				Log($"Preserving key {key}");
			}
		}
		else
		{
			Log($"Adding key {key} with value {keyValue}");
			XmlNode xmlNode2 = Xml.SelectSingleNode("/configuration/appSettings");
			XmlElement xmlElement = Xml.CreateElement("add");
			xmlElement.SetAttribute("key", key);
			xmlElement.SetAttribute("value", keyValue);
			xmlNode2.AppendChild(xmlElement);
		}
	}

	public virtual void RemoveElement(string key)
	{
		XmlNode xmlNode = Xml.SelectSingleNode("/configuration/appSettings");
		XmlNode xmlNode2 = Xml.SelectSingleNode($"/configuration/appSettings/add[@key='{key}']");
		if (xmlNode2 != null)
		{
			Log($"Removing key {key}");
			xmlNode.RemoveChild(xmlNode2);
		}
	}

	public virtual void Save()
	{
		Xml.Save(_filename);
	}

	private void Log(string message)
	{
		if (_parentTask != null)
		{
			_parentTask.Log(Level.Info, message);
		}
	}
}
