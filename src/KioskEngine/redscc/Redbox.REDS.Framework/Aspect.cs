using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.Core;

namespace Redbox.REDS.Framework;

public class Aspect : IAspect
{
	private IDictionary<string, string> m_properties;

	public string Name { get; internal set; }

	public IAspectType Type { get; internal set; }

	public AspectFlags Flags { get; internal set; }

	public IDictionary<string, string> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new Dictionary<string, string>();
			}
			return m_properties;
		}
	}

	internal object Content { get; set; }

	public object GetContent()
	{
		if (Content is IAspectContentLoader aspectContentLoader)
		{
			Content = aspectContentLoader.GetContent();
		}
		return Content;
	}

	public void SetContent(object value)
	{
		Content = value;
	}

	public ErrorList Save(XmlTextWriter writer)
	{
		ErrorList result = new ErrorList();
		writer.WriteStartElement("aspect");
		writer.WriteAttributeString("name", Name);
		foreach (KeyValuePair<string, string> property in Properties)
		{
			if (property.Value != null)
			{
				writer.WriteAttributeString(property.Key, property.Value);
			}
		}
		if (!Properties.ContainsKey("file"))
		{
			object content = GetContent();
			if (content is byte[])
			{
				AspectBinaryEncodingType? aspectBinaryEncodingType = AspectBinaryEncodingType.Base64;
				if (Properties.ContainsKey("encoding"))
				{
					aspectBinaryEncodingType = Enum<AspectBinaryEncodingType?>.ParseIgnoringCase(Properties["encoding"], AspectBinaryEncodingType.Base64);
				}
				else
				{
					writer.WriteAttributeString("encoding", aspectBinaryEncodingType.ToString());
				}
				switch (aspectBinaryEncodingType)
				{
				case AspectBinaryEncodingType.Base64:
					writer.WriteString(Convert.ToBase64String((byte[])content));
					break;
				case AspectBinaryEncodingType.ASCII85:
					writer.WriteCData(ASCII85.Encode((byte[])content));
					break;
				}
			}
			else if (content is XmlNode)
			{
				writer.WriteString(((XmlNode)content).OuterXml);
			}
			else if (content is string)
			{
				writer.WriteString((string)content);
			}
		}
		writer.WriteEndElement();
		return result;
	}

	protected internal Aspect()
	{
	}
}
