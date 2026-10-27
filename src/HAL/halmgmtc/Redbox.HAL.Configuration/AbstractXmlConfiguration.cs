using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Configuration;

public abstract class AbstractXmlConfiguration
{
	public delegate void ConfigurationChangeHandler();

	private readonly IRuntimeService RuntimeService;

	private readonly string RootName;

	public event ConfigurationChangeHandler ConfigurationChangeStart;

	public event ConfigurationChangeHandler ConfigurationChangeEnd;

	public void Load(string path, ErrorList errors)
	{
		if (!File.Exists(path))
		{
			errors.Add(Error.NewError("P001", $"The configuration file {path} does not exist.", "Specify a valid configuration file path."));
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(path);
		if (xmlDocument.DocumentElement != null)
		{
			LoadProperties(xmlDocument, errors);
		}
	}

	public void Save(string path, ErrorList errors)
	{
		if (!File.Exists(path))
		{
			errors.Add(Error.NewError("P001", $"The configuration file {path} does not exist.", "Specify a valid configuration file path."));
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(path);
		if (xmlDocument.DocumentElement != null)
		{
			StoreProperties(xmlDocument, errors);
			xmlDocument.Save(path);
		}
	}

	public object GetPropertyByName(string name)
	{
		if (name != null)
		{
			return StringExtensions.GetValueForPath(name, this);
		}
		return null;
	}

	public void SetPropertyByName(string name, object[] value)
	{
		if (name != null && value != null)
		{
			if (ConfigurationChangeStart != null)
			{
				ConfigurationChangeStart();
			}
			if (value.Length == 1)
			{
				StringExtensions.SetValueForPath(name, this, value[0]);
			}
			else
			{
				StringExtensions.SetValueForPath(name, this, value);
			}
			Save(RuntimeService.ExpandConstantMacros("$(RunningPath)\\HAL.xml"), new ErrorList());
			if (ConfigurationChangeEnd != null)
			{
				LogHelper.Instance.Log("Broadcast configuration change.");
				ConfigurationChangeEnd();
			}
		}
	}

	public string FormatAsXml()
	{
		return PropertyHelper.FormatObjectAsXml(this, RootName);
	}

	public void UpdateFromXml(string xml)
	{
		if (ConfigurationChangeStart != null)
		{
			ConfigurationChangeStart();
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		PropertyHelper.UpdateObjectFromXml(this, xmlDocument.DocumentElement);
		Save(RuntimeService.ExpandConstantMacros("$(RunningPath)\\HAL.xml"), new ErrorList());
		if (ConfigurationChangeEnd != null)
		{
			LogHelper.Instance.Log("Broadcast configuration change.");
			ConfigurationChangeEnd();
		}
	}

	protected abstract void StoreProperties(XmlDocument document, ErrorList errors);

	protected abstract void LoadProperties(XmlDocument document, ErrorList errors);

	protected AbstractXmlConfiguration(string xmlRoot)
	{
		RootName = xmlRoot;
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
	}

	protected T GetNodeValue<T>(XmlNode parent, string singleNode, T defaultValue)
	{
		XmlNode xmlNode = parent.SelectSingleNode(singleNode);
		try
		{
			if (xmlNode != null && !string.IsNullOrEmpty(xmlNode.InnerText))
			{
				return (T)ConversionHelper.ChangeType(RuntimeService.ExpandConstantMacros(xmlNode.InnerText), typeof(T));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
	}

	protected void SetSingleNodeValue(XmlNode node, string path, object value)
	{
		GetTargetNode(node, path).InnerText = value.ToString();
	}

	private XmlNode GetTargetNode(XmlNode node, string path)
	{
		string[] array = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		XmlNode xmlNode = node;
		string[] array2 = array;
		foreach (string text in array2)
		{
			XmlNode xmlNode2 = xmlNode.SelectSingleNode(text);
			if (xmlNode2 == null)
			{
				xmlNode2 = xmlNode.OwnerDocument.CreateElement(text);
				xmlNode.AppendChild(xmlNode2);
			}
			xmlNode = xmlNode2;
		}
		return xmlNode;
	}
}
