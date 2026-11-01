using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EPropertyHelper
{
	public static string FormatObjectAsXml(object instance, string rootName)
	{
		using StringWriter stringWriter = new StringWriter();
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
		xmlTextWriter.WriteStartDocument();
		xmlTextWriter.WriteStartElement(rootName);
		WalkObjectTree(instance, xmlTextWriter);
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndDocument();
		xmlTextWriter.Flush();
		return stringWriter.ToString();
	}

	public static void UpdateObjectFromXml(object instance, XmlNode parentNode)
	{
		if (instance == null || parentNode == null)
		{
			return;
		}
		XmlNodeList propertyNodes = parentNode.SelectNodes("property");
		if (propertyNodes == null)
		{
			return;
		}
		foreach (XmlNode propertyNode in propertyNodes)
		{
			if (propertyNode.Attributes["read-only"] != null && string.Compare(propertyNode.Attributes["read-only"].Value, bool.TrueString, ignoreCase: true) == 0)
			{
				continue;
			}
			PropertyInfo propertyInfo = null;
			if (propertyNode.Attributes["name"] != null)
			{
				propertyInfo = instance.GetType().GetProperty(propertyNode.Attributes["name"].Value, BindingFlags.Instance | BindingFlags.Public);
			}
			if (propertyInfo == null)
			{
				continue;
			}
			XmlNodeList childPropertyNodes = propertyNode.SelectNodes("property");
			if (childPropertyNodes != null && childPropertyNodes.Count > 0)
			{
				UpdateObjectFromXml(propertyInfo.GetValue(instance, new object[0]), propertyNode);
				continue;
			}
			Redbox_002EMacros1309_002ECustomEditorAttribute customEditor = (Redbox_002EMacros1309_002ECustomEditorAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(Redbox_002EMacros1309_002ECustomEditorAttribute));
			if (propertyNode.ChildNodes.Count > 0 && customEditor != null && customEditor.SetMethodName != null)
			{
				instance.GetType().GetMethod(customEditor.SetMethodName, BindingFlags.Instance | BindingFlags.Public)?.Invoke(instance, new object[1] { propertyNode });
			}
			else if (propertyNode.Attributes["value"] != null)
			{
				string value = propertyNode.Attributes["value"].Value;
				propertyInfo.SetValue(instance, Redbox_002EMacros1309_002EConversionHelper.ChangeType(value, propertyInfo.PropertyType), null);
			}
		}
	}

	private static void WalkObjectTree(object instance, XmlWriter xmlWriter)
	{
		if (instance == null)
		{
			return;
		}
		PropertyInfo[] properties = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			BrowsableAttribute browsableAttribute = (BrowsableAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(BrowsableAttribute));
			if (browsableAttribute != null && !browsableAttribute.Browsable)
			{
				continue;
			}
			xmlWriter.WriteStartElement("property");
			xmlWriter.WriteAttributeString("name", propertyInfo.Name);
			DisplayNameAttribute displayNameAttribute = (DisplayNameAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DisplayNameAttribute));
			if (displayNameAttribute != null)
			{
				xmlWriter.WriteAttributeString("display-name", displayNameAttribute.DisplayName);
			}
			DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute));
			if (descriptionAttribute != null)
			{
				xmlWriter.WriteAttributeString("description", descriptionAttribute.Description);
			}
			CategoryAttribute categoryAttribute = (CategoryAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CategoryAttribute));
			if (categoryAttribute != null)
			{
				xmlWriter.WriteAttributeString("category", categoryAttribute.Category);
			}
			ReadOnlyAttribute readOnlyAttribute = (ReadOnlyAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ReadOnlyAttribute));
			if (readOnlyAttribute != null)
			{
				xmlWriter.WriteAttributeString("read-only", XmlConvert.ToString(readOnlyAttribute.IsReadOnly));
			}
			Redbox_002EMacros1309_002ECustomEditorAttribute customEditor = (Redbox_002EMacros1309_002ECustomEditorAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(Redbox_002EMacros1309_002ECustomEditorAttribute));
			if (customEditor != null)
			{
				xmlWriter.WriteAttributeString("custom-editor", customEditor.Text);
				if (customEditor.GetMethodName != null)
				{
					instance.GetType().GetMethod(customEditor.GetMethodName, BindingFlags.Instance | BindingFlags.Public)?.Invoke(instance, new object[1] { xmlWriter });
				}
			}
			else
			{
				object propertyValue = null;
				try
				{
					propertyValue = propertyInfo.GetValue(instance, new object[0]);
					if (propertyValue != null)
					{
						TypeConverter converter = TypeDescriptor.GetConverter(propertyValue);
						string value = ((converter == null || !converter.CanConvertTo(typeof(string))) ? propertyValue.ToString() : converter.ConvertToString(propertyValue));
						xmlWriter.WriteAttributeString("value", value);
						xmlWriter.WriteAttributeString("default-value", value);
					}
				}
				catch (Exception)
				{
				}
				if (!Attribute.IsDefined(propertyInfo, typeof(Redbox_002EMacros1309_002EExcludeTypeAttribute)))
				{
					xmlWriter.WriteAttributeString("type", propertyInfo.PropertyType.AssemblyQualifiedName);
				}
				if (Attribute.IsDefined(propertyInfo, typeof(Redbox_002EMacros1309_002ERecurseAttribute)) && propertyValue != null)
				{
					WalkObjectTree(propertyValue, xmlWriter);
				}
				else
				{
					Redbox_002EMacros1309_002EValidValueListProviderAttribute validValueListProvider = (Redbox_002EMacros1309_002EValidValueListProviderAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(Redbox_002EMacros1309_002EValidValueListProviderAttribute));
					if (validValueListProvider != null)
					{
						MethodInfo method = instance.GetType().GetMethod(validValueListProvider.MethodName, BindingFlags.Instance | BindingFlags.Public);
						if (method != null && method.Invoke(instance, null) is string[] validValues)
						{
							xmlWriter.WriteAttributeString("valid-value-count", validValues.Length.ToString());
							foreach (string validValue in validValues)
							{
								xmlWriter.WriteStartElement("valid");
								xmlWriter.WriteAttributeString("value", validValue);
								xmlWriter.WriteEndElement();
							}
						}
					}
				}
			}
			xmlWriter.WriteEndElement();
		}
	}
}
