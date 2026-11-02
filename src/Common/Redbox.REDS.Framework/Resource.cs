using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using Redbox.Core;

namespace Redbox.REDS.Framework;

public class Resource : IResource
{
	private IDictionary<string, Aspect>? m_aspects;

	private IDictionary<string, object?>? m_properties;

	private readonly List<string> m_dependencies = new List<string>();

	public string? Name => (Properties.ContainsKey("name") ? Properties["name"] : null) as string;

	public object? this[string name]
	{
		get
		{
			IPropertyType? propertyType = Type.GetPropertyType(name);
			if (propertyType == null)
			{
				return null;
			}
			if (!Properties.ContainsKey(name))
			{
				return propertyType.DefaultValue;
			}
			return Properties[name];
		}
		set
		{
			IPropertyType? propertyType = Type.GetPropertyType(name);
			if (propertyType == null)
			{
				throw new ArgumentException($"Property '{name}' is not a valid part of the type.");
			}
			Properties[name] = propertyType.GetTypedValue(value);
		}
	}

	public IResourceBundleStorageEntry? StorageEntry { get; internal set; }

	public ReadOnlyCollection<string> Dependencies => m_dependencies.AsReadOnly();

	public IResourceType Type { get; internal set; } = null!; // set by ResourceType.NewResourceInstance

	internal List<string> InnerDependencies => m_dependencies;

	internal IDictionary<string, Aspect> Aspects
	{
		get
		{
			if (m_aspects == null)
			{
				m_aspects = new Dictionary<string, Aspect>();
			}
			return m_aspects;
		}
	}

	internal IDictionary<string, object?> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new Dictionary<string, object?>();
			}
			return m_properties;
		}
	}

	public static ErrorList FromXml(ResourceBundle bundle, XmlDocument document, out IResource? resource)
	{
		resource = null;
		ErrorList errorList = new ErrorList();
		try
		{
			if (document == null || document.DocumentElement == null)
			{
				errorList.Add(Error.NewError("R001", "The specified XML document is invalid.", "Review the resource bundle to ensure valid XML is embedded."));
				return errorList;
			}
			string typeName = document.DocumentElement.GetAttributeValue<string>("type");
			if (string.IsNullOrEmpty(typeName))
			{
				errorList.Add(Error.NewError("R002", "The type attribute must be specified.", "Specify the type attribute in the resource XML and recompile."));
				return errorList;
			}
			if (!bundle.ResourceTypes.ContainsKey(typeName))
			{
				errorList.Add(Error.NewError("R003", $"The type '{typeName}' is not defined.", "Specify a valid resource type in the resource XML and recompile."));
				return errorList;
			}
			IResourceType resourceType = bundle.ResourceTypes[typeName];
			XmlNode? node = document.DocumentElement.SelectSingleNode("property[@name='name']");
			resource = resourceType.NewResourceInstance(node.GetAttributeValue<string>("value"));
			XmlNodeList? propertyNodes = document.DocumentElement.SelectNodes("property");
			if (propertyNodes != null)
			{
				foreach (XmlNode propertyNode in propertyNodes)
				{
					string propertyName = propertyNode.GetAttributeValue<string>("name");
					if (propertyNode.Attributes!["value"] != null)
					{
						resource[propertyName] = propertyNode.GetAttributeValue<object>("value");
						continue;
					}
					XmlNodeList? valueNodes = propertyNode.SelectNodes("value");
					if (valueNodes == null || valueNodes.Count == 0)
					{
						errorList.Add(Error.NewError("R006", $"The property '{typeName}' does not have a value attribute or child value nodes.", "Each property must have a valid value attribute or one or more valid value child nodes."));
						continue;
					}
					List<object> values = new List<object>();
					foreach (XmlNode valueNode in valueNodes)
					{
						values.Add(valueNode.GetNodeValue<object>());
					}
					resource[propertyName] = values;
				}
			}
			string dependsOn = document.DocumentElement.GetAttributeValue<string>("depends");
			if (!string.IsNullOrEmpty(dependsOn))
			{
				string[] parts = dependsOn.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < parts.Length; i++)
				{
					string dependency = parts[i].Trim();
					if (!(dependency == resource.Name))
					{
						resource.AddDependency(dependency);
					}
				}
			}
			XmlNodeList? aspectNodes = document.DocumentElement.SelectNodes("aspect");
			if (aspectNodes != null)
			{
				foreach (XmlNode aspectNode in aspectNodes)
				{
					string aspectName = aspectNode.GetAttributeValue<string>("name");
					if (string.IsNullOrEmpty(aspectName))
					{
						errorList.Add(Error.NewError("R004", "The name attribute on the aspect element must be specified.", "Specify the name attribute on the aspect element in the resource XML and recompile."));
						continue;
					}
					IAspectType? aspectType = resource.Type.GetAspectType(aspectName);
					if (aspectType == null)
					{
						errorList.Add(Error.NewError("R005", $"The aspect type '{aspectName}' is not defined.", "Specify a valid aspect type in the resource XML and recompile."));
						return errorList;
					}
					IAspect aspect = resource.NewAspect(aspectName, aspectType);
					foreach (XmlAttribute attribute in aspectNode.Attributes!)
					{
						if (!(attribute.Name == "name"))
						{
							aspect.Properties[attribute.Name] = attribute.Value;
						}
					}
					errorList.AddRange(aspect.Type.ParseAspect(bundle, aspect, aspectNode));
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("R999", "An unhandled exception was raised in Resource.FromXml.", e));
		}
		return errorList;
	}

	public override string ToString()
	{
		return Name ?? "(unnamed)";
	}

	public bool IsOverriden(ReadOnlyCollection<string> exclusions)
	{
		foreach (IPropertyType filterProperty in Type.GetFilterProperties())
		{
			if (!exclusions.Contains(filterProperty.Name) && this[filterProperty.Name] != null)
			{
				return true;
			}
		}
		return false;
	}

	public IAspect NewAspect(string name, IAspectType type)
	{
		Aspect aspect = new Aspect
		{
			Name = name,
			Type = type
		};
		Aspects[name] = aspect;
		return aspect;
	}

	public IAspect? GetAspect(string name)
	{
		if (!Aspects.ContainsKey(name))
		{
			return null;
		}
		return Aspects[name];
	}

	public string GetFullName()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Name);
		foreach (IPropertyType filterProperty in Type.GetFilterProperties())
		{
			string? text = this[filterProperty.Name] as string;
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.AppendFormat("_{0}", text);
			}
		}
		return stringBuilder.ToString();
	}

	public void AddDependency(string name)
	{
		if (!InnerDependencies.Contains(name))
		{
			InnerDependencies.Add(name);
		}
	}

	public void RemoveDependency(string name)
	{
		InnerDependencies.Remove(name);
	}

	public void ForEachPropertyNameDo(Action<string> action)
	{
		foreach (string key in Properties.Keys)
		{
			action(key);
		}
	}

	public void ForEachPropertyValueDo(Action<object?> action)
	{
		foreach (object? value in Properties.Values)
		{
			action(value);
		}
	}

	public ErrorList Save(IResourceBundle bundle)
	{
		ErrorList result = new ErrorList();
		StringBuilder stringBuilder = new StringBuilder();
		XmlTextWriter xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder))
		{
			Formatting = Formatting.Indented
		};
		xmlTextWriter.WriteStartDocument();
		xmlTextWriter.WriteStartElement("resource");
		xmlTextWriter.WriteAttributeString("type", Type.Name);
		foreach (KeyValuePair<string, object?> property in Properties)
		{
			if (property.Value == null)
			{
				continue;
			}
			xmlTextWriter.WriteStartElement("property");
			xmlTextWriter.WriteAttributeString("name", property.Key);
			if (property.Value is List<object>)
			{
				foreach (object item in (List<object>)property.Value)
				{
					xmlTextWriter.WriteElementString("value", item.ToString());
				}
			}
			else
			{
				xmlTextWriter.WriteAttributeString("value", property.Value.ToString());
			}
			xmlTextWriter.WriteEndElement();
		}
		foreach (KeyValuePair<string, Aspect> aspect in Aspects)
		{
			aspect.Value.Save(xmlTextWriter);
		}
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndDocument();
		bundle?.StoreResource(GetStorageFriendlyName(Name!), stringBuilder.ToString());
		return result;
	}

	public bool PropertyEquals(string name, object? value)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		if (this[name] is List<object?> list)
		{
			return list.Contains(value);
		}
		if (this[name] is string strA && value is string)
		{
			return string.Compare(strA, (string)value, ignoreCase: true) == 0;
		}
		if (this[name] != null)
		{
			return this[name]!.Equals(value);
		}
		return false;
	}

	public bool PropertyEquals2(string name, object? value)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		bool negate = name.Substring(0, 2) == "!=";
		if (this[name] is List<object?> list)
		{
			if (!negate)
			{
				return list.Contains(value);
			}
			return !list.Contains(value);
		}
		if (this[name] is string strA && value is string)
		{
			if (!negate)
			{
				return string.Compare(strA, (string)value, ignoreCase: true) == 0;
			}
			return string.Compare(strA, (string)value, ignoreCase: true) != 0;
		}
		if (!negate)
		{
			if (this[name] != null)
			{
				return this[name]!.Equals(value);
			}
			return false;
		}
		if (this[name] != null)
		{
			return !this[name]!.Equals(value);
		}
		return false;
	}

	protected internal Resource()
	{
	}

	private static string GetStorageFriendlyName(string name)
	{
		name = name.Replace("_", "-");
		name = name.Replace(" ", "-");
		return Path.ChangeExtension(name, ".resource");
	}
}
