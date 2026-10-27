using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Configuration;

public abstract class AttributeXmlConfiguration : IAttributeXmlConfiguration, IConfigurationObserver
{
	private readonly Type ThisType;

	private readonly List<IConfigurationObserver> Observers = new List<IConfigurationObserver>();

	private const BindingFlags PropertyFlags = BindingFlags.Instance | BindingFlags.Public;

	[Browsable(false)]
	public string RootName { get; private set; }

	protected abstract string FileNodeName { get; }

	public void NotifyConfigurationLoaded()
	{
		foreach (IConfigurationObserver observer in Observers)
		{
			observer.NotifyConfigurationLoaded();
		}
	}

	public void NotifyConfigurationChangeStart()
	{
		foreach (IConfigurationObserver observer in Observers)
		{
			observer.NotifyConfigurationChangeStart();
		}
	}

	public void NotifyConfigurationChangeEnd()
	{
		foreach (IConfigurationObserver observer in Observers)
		{
			observer.NotifyConfigurationChangeEnd();
		}
	}

	public void AddObserver(IConfigurationObserver observer)
	{
		Observers.Add(observer);
	}

	public void RemoveObserver(IConfigurationObserver observer)
	{
		Observers.Remove(observer);
	}

	public void LoadProperties(XmlDocument document, ErrorList errors)
	{
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			foreach (XmlNode childNode in document.DocumentElement.SelectSingleNode(FileNodeName).ChildNodes)
			{
				string name = childNode.Name;
				try
				{
					PropertyInfo property = ThisType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
					if (property == null)
					{
						if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
						{
							LogHelper.Instance.Log("[AttributeXmlConfiguration] Unable to find property '{0}'", name);
						}
					}
					else if (Attribute.GetCustomAttribute(property, typeof(XmlConfigurationAttribute)) is XmlConfigurationAttribute)
					{
						object value = ConversionHelper.ChangeType(childNode.InnerText, property.PropertyType);
						property.SetValue(this, value, null);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("[AbstractXmlConfiguration] LoadProperties caught an exception.", e);
				}
			}
			LogHelper.Instance.Log("[AbstractXmlConfiguration] Time to load defaults on type {0}: {1}ms", ThisType.Name, executionTimer.ElapsedMilliseconds);
			executionTimer.Stop();
		}
		LoadPropertiesInner(document, errors);
	}

	public void StoreProperties(XmlDocument document, ErrorList errors)
	{
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			PropertyInfo[] properties = ThisType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (Attribute.GetCustomAttribute(propertyInfo, typeof(XmlConfigurationAttribute)) is XmlConfigurationAttribute)
				{
					try
					{
						string value = propertyInfo.GetValue(this, null).ToString();
						SetSingleNodeValue(document.DocumentElement, $"{FileNodeName}/{propertyInfo.Name}", value);
					}
					catch (Exception e)
					{
						LogHelper.Instance.Log($"[AbstractXmlConfiguration] StoreProperties caught an exception processing property {propertyInfo.Name}", e);
					}
				}
			}
			executionTimer.Stop();
			LogHelper.Instance.Log("[AbstractXmlConfiguration] Time to save defaults on type {0}: {1}ms", ThisType.Name, executionTimer.ElapsedMilliseconds);
		}
		StorePropertiesInner(document, errors);
	}

	public void Import(ErrorList errors)
	{
		ImportInner(errors);
	}

	public void Upgrade(XmlDocument document, ErrorList errors)
	{
		UpgradeInner(document, errors);
	}

	protected AttributeXmlConfiguration(string xmlRoot, Type type)
	{
		RootName = xmlRoot;
		ThisType = type;
		using ExecutionTimer executionTimer = new ExecutionTimer();
		PropertyInfo[] properties = ThisType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (Attribute.GetCustomAttribute(propertyInfo, typeof(XmlConfigurationAttribute)) is XmlConfigurationAttribute xmlConfigurationAttribute)
			{
				try
				{
					object value = ConversionHelper.ChangeType(xmlConfigurationAttribute.DefaultValue, propertyInfo.PropertyType);
					propertyInfo.SetValue(this, value, null);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log($"[AttributeXmlConfiguration] Ctor caught an exception processing property {propertyInfo.Name}.", e);
				}
			}
		}
		executionTimer.Stop();
		LogHelper.Instance.Log("[AttributeXmlConfiguration] Time to set defaults on type {0}: {1}ms", type.Name, executionTimer.ElapsedMilliseconds);
	}

	protected abstract void ImportInner(ErrorList errors);

	protected abstract void UpgradeInner(XmlDocument document, ErrorList errors);

	protected abstract void LoadPropertiesInner(XmlDocument document, ErrorList errors);

	protected abstract void StorePropertiesInner(XmlDocument document, ErrorList errors);

	private void SetSingleNodeValue(XmlNode node, string path, object value)
	{
		GetTargetNode(node, path).InnerText = value.ToString();
	}

	private T GetNodeValue<T>(XmlNode parent, string singleNode, T defaultValue)
	{
		XmlNode xmlNode = parent.SelectSingleNode(singleNode);
		try
		{
			IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
			if (xmlNode != null && !string.IsNullOrEmpty(xmlNode.InnerText))
			{
				return (T)ConversionHelper.ChangeType(service.ExpandConstantMacros(xmlNode.InnerText), typeof(T));
			}
		}
		catch (ArgumentException)
		{
		}
		return defaultValue;
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
