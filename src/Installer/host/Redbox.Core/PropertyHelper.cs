using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Redbox.Core;

public static class PropertyHelper
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
		XmlNodeList xmlNodeList = parentNode.SelectNodes("property");
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			if (item.Attributes["read-only"] != null && string.Compare(item.Attributes["read-only"].Value, bool.TrueString, ignoreCase: true) == 0)
			{
				continue;
			}
			PropertyInfo propertyInfo = null;
			if (item.Attributes["name"] != null)
			{
				propertyInfo = instance.GetType().GetProperty(item.Attributes["name"].Value, BindingFlags.Instance | BindingFlags.Public);
			}
			if (propertyInfo == null)
			{
				continue;
			}
			XmlNodeList xmlNodeList2 = item.SelectNodes("property");
			if (xmlNodeList2 != null && xmlNodeList2.Count > 0)
			{
				UpdateObjectFromXml(propertyInfo.GetValue(instance, new object[0]), item);
				continue;
			}
			CustomEditorAttribute customEditorAttribute = (CustomEditorAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CustomEditorAttribute));
			if (item.ChildNodes.Count > 0 && customEditorAttribute != null && customEditorAttribute.SetMethodName != null)
			{
				instance.GetType().GetMethod(customEditorAttribute.SetMethodName, BindingFlags.Instance | BindingFlags.Public)?.Invoke(instance, new object[1] { item });
			}
			else if (item.Attributes["value"] != null)
			{
				string value = item.Attributes["value"].Value;
				propertyInfo.SetValue(instance, ConversionHelper.ChangeType(value, propertyInfo.PropertyType), null);
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
			CustomEditorAttribute customEditorAttribute = (CustomEditorAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CustomEditorAttribute));
			if (customEditorAttribute != null)
			{
				xmlWriter.WriteAttributeString("custom-editor", customEditorAttribute.Text);
				if (customEditorAttribute.GetMethodName != null)
				{
					instance.GetType().GetMethod(customEditorAttribute.GetMethodName, BindingFlags.Instance | BindingFlags.Public)?.Invoke(instance, new object[1] { xmlWriter });
				}
			}
			else
			{
				object obj = null;
				try
				{
					obj = propertyInfo.GetValue(instance, new object[0]);
					if (obj != null)
					{
						TypeConverter converter = TypeDescriptor.GetConverter(obj);
						string value = ((converter == null || !converter.CanConvertTo(typeof(string))) ? obj.ToString() : converter.ConvertToString(obj));
						xmlWriter.WriteAttributeString("value", value);
						xmlWriter.WriteAttributeString("default-value", value);
					}
				}
				catch (Exception)
				{
				}
				if (!Attribute.IsDefined(propertyInfo, typeof(ExcludeTypeAttribute)))
				{
					xmlWriter.WriteAttributeString("type", propertyInfo.PropertyType.AssemblyQualifiedName);
				}
				if (Attribute.IsDefined(propertyInfo, typeof(RecurseAttribute)) && obj != null)
				{
					WalkObjectTree(obj, xmlWriter);
				}
				else
				{
					ValidValueListProviderAttribute validValueListProviderAttribute = (ValidValueListProviderAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ValidValueListProviderAttribute));
					if (validValueListProviderAttribute != null)
					{
						MethodInfo method = instance.GetType().GetMethod(validValueListProviderAttribute.MethodName, BindingFlags.Instance | BindingFlags.Public);
						if (method != null && method.Invoke(instance, null) is string[] array)
						{
							xmlWriter.WriteAttributeString("valid-value-count", array.Length.ToString());
							string[] array2 = array;
							foreach (string value2 in array2)
							{
								xmlWriter.WriteStartElement("valid");
								xmlWriter.WriteAttributeString("value", value2);
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
