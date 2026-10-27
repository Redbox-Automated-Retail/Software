using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using Redbox.Core;

namespace Redbox.REDS.Framework;

public class ManifestAspectType : AspectType
{
	protected override void OnParseAspect(IResourceBundle bundle, IAspect aspect, XmlNode aspectNode, ErrorList errors)
	{
		if (aspect.Name != "schema")
		{
			return;
		}
		XmlNodeList xmlNodeList = aspectNode.SelectNodes("resourceType");
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			string attributeValue = item.GetAttributeValue<string>("name");
			if (string.IsNullOrEmpty(attributeValue))
			{
				errors.Add(Error.NewError("R005", "The name attribute must be defined on the aspect element.", "Add a valid name attribute to the aspect element."));
				break;
			}
			IResourceType resourceType = null;
			string attributeValue2 = item.GetAttributeValue<string>("superType");
			if (!string.IsNullOrEmpty(attributeValue2))
			{
				if (!bundle.ResourceTypes.ContainsKey(attributeValue2))
				{
					errors.Add(Error.NewError("R003", $"The type '{attributeValue2}' is not defined.", "Specify a valid resource type in the resource XML and recompile."));
					break;
				}
				resourceType = bundle.ResourceTypes[attributeValue2];
			}
			string attributeValue3 = item.GetAttributeValue<string>("runtimeType");
			Type type = null;
			if (!string.IsNullOrEmpty(attributeValue3))
			{
				type = GetType(attributeValue3);
				if (type == null)
				{
					errors.Add(Error.NewWarning("R004", $"The runtime type '{attributeValue3}' is not defined in any loaded assembly.", "Specify a valid runtime type and ensure the appropriate assemblies are loaded."));
				}
			}
			IResourceType resourceType2 = ((resourceType == null) ? bundle.ResourceTypes["root"] : resourceType.NewType(attributeValue, type));
			XmlNodeList xmlNodeList2 = item.SelectNodes("propertyType");
			if (xmlNodeList2 != null)
			{
				foreach (XmlNode item2 in xmlNodeList2)
				{
					Type type2 = GetType(item2.GetAttributeValue<string>("type")) ?? typeof(string);
					IPropertyType propertyType = resourceType2.AddPropertyType(item2.GetAttributeValue<string>("name"), type2, item2.GetAttributeValue<string>("description"), item2.GetAttributeValue<bool>("isRequired"), item2.GetAttributeValue<bool>("isFilter"), item2.GetAttributeValue<int?>("score"), item2.GetAttributeValue<string>("label"), item2.GetAttributeValue<string>("category"));
					propertyType.SetDefaultValue(item2.GetAttributeValue<string>("defaultValue"));
				}
			}
			XmlNodeList xmlNodeList3 = item.SelectNodes("aspectType");
			if (xmlNodeList3 != null)
			{
				foreach (XmlNode item3 in xmlNodeList3)
				{
					IAspectType aspectType = resourceType2.AddAspectType(item3.GetAttributeValue<string>("name"), GetType(item3.GetAttributeValue<string>("type")));
					XmlNodeList xmlNodeList4 = item3.SelectNodes("property");
					if (xmlNodeList4 == null)
					{
						continue;
					}
					foreach (XmlNode item4 in xmlNodeList4)
					{
						aspectType[item4.GetAttributeValue<string>("name")] = item4.GetAttributeValue<string>("value");
					}
				}
			}
			if (resourceType2.Name != "root")
			{
				bundle.ResourceTypes[attributeValue] = resourceType2;
			}
		}
	}

	private static Type GetType(string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		if (typeName.StartsWith("System.Windows.Forms"))
		{
			Assembly assembly = typeof(Control).Assembly;
			return assembly.GetType(typeName);
		}
		if (typeName.StartsWith("System.Drawing"))
		{
			Assembly assembly2 = typeof(FontStyle).Assembly;
			return assembly2.GetType(typeName);
		}
		if (typeName.StartsWith("Redbox.REDS.Framework"))
		{
			Assembly assembly3 = typeof(ManifestAspectType).Assembly;
			return assembly3.GetType(typeName);
		}
		return typeName.ToType();
	}
}
