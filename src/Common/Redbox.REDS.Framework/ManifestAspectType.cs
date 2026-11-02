using System;
using System.Drawing;
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
		XmlNodeList? resourceTypeNodes = aspectNode.SelectNodes("resourceType");
		if (resourceTypeNodes == null)
		{
			return;
		}
		foreach (XmlNode typeNode in resourceTypeNodes)
		{
			string typeName = typeNode.GetAttributeValue<string>("name");
			if (string.IsNullOrEmpty(typeName))
			{
				errors.Add(Error.NewError("R005", "The name attribute must be defined on the aspect element.", "Add a valid name attribute to the aspect element."));
				break;
			}
			IResourceType? superType = null;
			string superTypeName = typeNode.GetAttributeValue<string>("superType");
			if (!string.IsNullOrEmpty(superTypeName))
			{
				if (!bundle.ResourceTypes.ContainsKey(superTypeName))
				{
					errors.Add(Error.NewError("R003", $"The type '{superTypeName}' is not defined.", "Specify a valid resource type in the resource XML and recompile."));
					break;
				}
				superType = bundle.ResourceTypes[superTypeName];
			}
			string runtimeTypeName = typeNode.GetAttributeValue<string>("runtimeType");
			Type? runtimeType = null;
			if (!string.IsNullOrEmpty(runtimeTypeName))
			{
				runtimeType = GetType(runtimeTypeName);
				if (runtimeType == null)
				{
					errors.Add(Error.NewWarning("R004", $"The runtime type '{runtimeTypeName}' is not defined in any loaded assembly.", "Specify a valid runtime type and ensure the appropriate assemblies are loaded."));
				}
			}
			IResourceType newType = ((superType == null) ? bundle.ResourceTypes["root"] : superType.NewType(typeName, runtimeType));
			XmlNodeList? propertyTypeNodes = typeNode.SelectNodes("propertyType");
			if (propertyTypeNodes != null)
			{
				foreach (XmlNode propertyTypeNode in propertyTypeNodes)
				{
					Type valueType = GetType(propertyTypeNode.GetAttributeValue<string>("type")) ?? typeof(string);
					newType.AddPropertyType(propertyTypeNode.GetAttributeValue<string>("name"), valueType, propertyTypeNode.GetAttributeValue<string>("description"), propertyTypeNode.GetAttributeValue<bool>("isRequired"), propertyTypeNode.GetAttributeValue<bool>("isFilter"), propertyTypeNode.GetAttributeValue<int?>("score"), propertyTypeNode.GetAttributeValue<string>("label"), propertyTypeNode.GetAttributeValue<string>("category")).SetDefaultValue(propertyTypeNode.GetAttributeValue<string>("defaultValue"));
				}
			}
			XmlNodeList? aspectTypeNodes = typeNode.SelectNodes("aspectType");
			if (aspectTypeNodes != null)
			{
				foreach (XmlNode aspectTypeNode in aspectTypeNodes)
				{
					IAspectType aspectType = newType.AddAspectType(aspectTypeNode.GetAttributeValue<string>("name"), GetType(aspectTypeNode.GetAttributeValue<string>("type")));
					XmlNodeList? propertyNodes = aspectTypeNode.SelectNodes("property");
					if (propertyNodes == null)
					{
						continue;
					}
					foreach (XmlNode propertyNode in propertyNodes)
					{
						aspectType[propertyNode.GetAttributeValue<string>("name")] = propertyNode.GetAttributeValue<string>("value");
					}
				}
			}
			if (newType.Name != "root")
			{
				bundle.ResourceTypes[typeName] = newType;
			}
		}
	}

	private static Type? GetType(string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		if (typeName.StartsWith("System.Windows.Forms"))
		{
			return typeof(Control).Assembly.GetType(typeName);
		}
		if (typeName.StartsWith("System.Drawing"))
		{
			return typeof(FontStyle).Assembly.GetType(typeName);
		}
		if (typeName.StartsWith("Redbox.REDS.Framework"))
		{
			return typeof(ManifestAspectType).Assembly.GetType(typeName);
		}
		return typeName.ToType();
	}
}
