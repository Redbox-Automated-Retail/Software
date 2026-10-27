using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Configuration;

public sealed class ConfigurationService : IConfigurationService
{
	internal const string IndexerGroup = "indexer";

	internal const string TextIndexerRegex = "\\[\"(?<indexer>.*?)\"\\]";

	internal const string NumericIndexerRegex = "\\[(?<indexer>[0-9]*?)\\]";

	private readonly Dictionary<string, IAttributeXmlConfiguration> Configurations = new Dictionary<string, IAttributeXmlConfiguration>();

	private readonly string ConfigurationFile;

	private readonly char[] PropertyPathSeparators = new char[1] { '.' };

	private readonly BindingFlags GetObjectFromPathBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty;

	public static ConfigurationService Make(string configFile)
	{
		return new ConfigurationService(configFile);
	}

	public string FormatAsXml(string friendly)
	{
		friendly = friendly.ToLower();
		if (Configurations.ContainsKey(friendly))
		{
			IAttributeXmlConfiguration attributeXmlConfiguration = Configurations[friendly];
			return PropertyHelper.FormatObjectAsXml(attributeXmlConfiguration, attributeXmlConfiguration.RootName);
		}
		return null;
	}

	public void UpdateFromXml(string key, string xmlData, ErrorList errors)
	{
		LogHelper.Instance.Log("[Configuration Service] Configuration change.");
		string key2 = key.ToLower();
		if (!Configurations.ContainsKey(key2))
		{
			LogHelper.Instance.Log("[Configuration Service UpdateFromXml] The configuration {0} doesn't exist.", key);
			errors.Add(Error.NewError("S001", $"The requested configuration name '{key}' is not known.", "Specify one of the valid configuration names."));
			return;
		}
		IAttributeXmlConfiguration attributeXmlConfiguration = Configurations[key2];
		BroadcastChangeStart(attributeXmlConfiguration);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xmlData);
		PropertyHelper.UpdateObjectFromXml(attributeXmlConfiguration, xmlDocument.DocumentElement);
		Save(errors, attributeXmlConfiguration);
		BroadcastChangeEnd(attributeXmlConfiguration);
	}

	public void Load(ErrorList errors)
	{
		if (!File.Exists(ConfigurationFile))
		{
			errors.Add(Error.NewError("P001", $"The configuration file {ConfigurationFile} does not exist.", "Specify a valid configuration file path."));
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(ConfigurationFile);
		if (xmlDocument.DocumentElement == null)
		{
			return;
		}
		foreach (string key in Configurations.Keys)
		{
			Configurations[key].LoadProperties(xmlDocument, errors);
		}
		LogHelper.Instance.Log("[Configuration Service] Broadcast configuration load.");
		foreach (string key2 in Configurations.Keys)
		{
			Configurations[key2].NotifyConfigurationLoaded();
		}
	}

	public void Save(ErrorList errors)
	{
		if (!File.Exists(ConfigurationFile))
		{
			errors.Add(Error.NewError("P001", $"The configuration file {ConfigurationFile} does not exist.", "Specify a valid configuration file path."));
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(ConfigurationFile);
		if (xmlDocument.DocumentElement == null)
		{
			return;
		}
		foreach (string key in Configurations.Keys)
		{
			Configurations[key].StoreProperties(xmlDocument, errors);
		}
		xmlDocument.Save(ConfigurationFile);
	}

	public object GetPropertyByName(string key, string name)
	{
		if (name == null)
		{
			return null;
		}
		IAttributeXmlConfiguration attributeXmlConfiguration = FromKey(key);
		if (attributeXmlConfiguration != null)
		{
			return GetValueForPath(name, attributeXmlConfiguration);
		}
		return null;
	}

	public void SetPropertyByName(string key, string name, object[] value)
	{
		LogHelper.Instance.Log("[Configuration Service] Property update.");
		if (name != null && value != null)
		{
			IAttributeXmlConfiguration attributeXmlConfiguration = FromKey(key);
			if (attributeXmlConfiguration != null)
			{
				BroadcastChangeStart(attributeXmlConfiguration);
				SetValueForPath(name, attributeXmlConfiguration, (1 == value.Length) ? value[0] : value);
				Save(new ErrorList(), attributeXmlConfiguration);
				BroadcastChangeEnd(attributeXmlConfiguration);
			}
		}
	}

	public void RegisterConfiguration(string key, IAttributeXmlConfiguration me)
	{
		string key2 = key.ToLower();
		if (!Configurations.ContainsKey(key2))
		{
			Configurations[key2] = me;
		}
	}

	public void LoadAndImport(ErrorList errors)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(ConfigurationFile);
		if (xmlDocument.DocumentElement == null)
		{
			return;
		}
		Load(errors);
		foreach (string key in Configurations.Keys)
		{
			IAttributeXmlConfiguration attributeXmlConfiguration = Configurations[key];
			attributeXmlConfiguration.Import(errors);
			attributeXmlConfiguration.StoreProperties(xmlDocument, errors);
		}
		xmlDocument.Save(ConfigurationFile);
	}

	public void LoadAndUpgrade(ErrorList errors)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(ConfigurationFile);
		if (xmlDocument.DocumentElement == null)
		{
			return;
		}
		foreach (string key in Configurations.Keys)
		{
			Configurations[key].Upgrade(xmlDocument, errors);
		}
		xmlDocument.Save(ConfigurationFile);
	}

	public IAttributeXmlConfiguration FindConfiguration(string name)
	{
		return FromKey(name);
	}

	public IAttributeXmlConfiguration FindConfiguration(Configurations configuration)
	{
		return FindConfiguration(configuration.ToString());
	}

	private void Save(ErrorList errors, IAttributeXmlConfiguration configuration)
	{
		if (!File.Exists(ConfigurationFile))
		{
			errors.Add(Error.NewError("P001", $"The configuration file {ConfigurationFile} does not exist.", "Specify a valid configuration file path."));
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(ConfigurationFile);
		if (xmlDocument.DocumentElement != null)
		{
			configuration.StoreProperties(xmlDocument, errors);
			xmlDocument.Save(ConfigurationFile);
		}
	}

	private void BroadcastChangeEnd(IAttributeXmlConfiguration config)
	{
		LogHelper.Instance.Log("[ConfigurationService] Broadcast config change end.");
		config.NotifyConfigurationChangeEnd();
	}

	private void BroadcastChangeStart(IAttributeXmlConfiguration config)
	{
		LogHelper.Instance.Log("[ConfigurationService] Broadcast config change start.");
		config.NotifyConfigurationChangeStart();
	}

	private object GetValueForPath(string path, object rootObject)
	{
		return GetObjectFromPath(path, rootObject, null);
	}

	private IAttributeXmlConfiguration FromKey(string key)
	{
		key = key.ToLower();
		if (Configurations.ContainsKey(key))
		{
			return Configurations[key];
		}
		return null;
	}

	private object ExtractIndexInternal(string part)
	{
		if (part == null)
		{
			return null;
		}
		Match match = Regex.Match(part, "\\[\"(?<indexer>.*?)\"\\]", RegexOptions.Singleline);
		Match match2 = Regex.Match(part, "\\[(?<indexer>[0-9]*?)\\]", RegexOptions.Singleline);
		object result = null;
		int result2;
		if (match.Success)
		{
			result = match.Groups["indexer"].Captures[0].Value;
		}
		else if (match2.Success && int.TryParse(match2.Groups["indexer"].Captures[0].Value, out result2))
		{
			result = result2;
		}
		return result;
	}

	private object GetObjectFromPath(string path, object rootObject, int? depthAdjust)
	{
		if (path == null)
		{
			return null;
		}
		string[] array = path.Split(PropertyPathSeparators);
		if (array.Length == 0 || rootObject == null)
		{
			return null;
		}
		int num = array.Length;
		if (depthAdjust.HasValue)
		{
			num -= depthAdjust.Value;
		}
		object obj = rootObject;
		for (int i = 0; i < num; i++)
		{
			object[] args = null;
			string text = array[i];
			object obj2 = ExtractIndexInternal(text);
			if (obj2 != null)
			{
				text = text.Substring(0, text.IndexOf("["));
				args = new object[1] { obj2 };
			}
			try
			{
				obj = obj.GetType().InvokeMember(text, GetObjectFromPathBindingFlags, null, obj, args);
				if (obj == null)
				{
					break;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}
		return obj;
	}

	private void SetValueForPath(string path, object rootObject, object value)
	{
		object objectFromPath = GetObjectFromPath(path, rootObject, 1);
		if (objectFromPath != null)
		{
			string[] array = path.Split(PropertyPathSeparators);
			PropertyInfo property = objectFromPath.GetType().GetProperty(array[array.Length - 1], BindingFlags.Instance | BindingFlags.Public);
			property?.SetValue(objectFromPath, ConversionHelper.ChangeType(value, property.PropertyType), null);
		}
	}

	private ConfigurationService(string file)
	{
		if (!string.IsNullOrEmpty(file))
		{
			ConfigurationFile = file;
		}
	}
}
