using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.Core;

namespace Redbox.REDS.Framework;

public class AspectType : IAspectType
{
	private readonly IDictionary<string, string> m_properties = new Dictionary<string, string>();

	public string Name { get; internal set; }

	public string this[string name]
	{
		get
		{
			if (!m_properties.ContainsKey(name))
			{
				return null;
			}
			return m_properties[name];
		}
		set
		{
			m_properties[name] = value;
		}
	}

	public ErrorList ParseAspect(IResourceBundle bundle, IAspect aspect, XmlNode aspectNode)
	{
		ErrorList errorList = new ErrorList();
		OnParseAspect(bundle, aspect, aspectNode, errorList);
		return errorList;
	}

	protected virtual void OnParseAspect(IResourceBundle bundle, IAspect aspect, XmlNode aspectNode, ErrorList errors)
	{
		if (aspect.Properties.ContainsKey("file"))
		{
			aspect.SetContent(new BinaryContentLoader
			{
				Bundle = bundle,
				Value = bundle.GetEntry(aspect.Properties["file"])
			});
		}
		else
		{
			if (!aspect.Properties.ContainsKey("encoding"))
			{
				return;
			}
			AspectBinaryEncodingType? aspectBinaryEncodingType = Enum<AspectBinaryEncodingType?>.ParseIgnoringCase(aspect.Properties["encoding"], null);
			if (aspectBinaryEncodingType.HasValue && aspectBinaryEncodingType.HasValue)
			{
				switch (aspectBinaryEncodingType.GetValueOrDefault())
				{
				case AspectBinaryEncodingType.Base64:
					aspect.SetContent(Convert.FromBase64String(aspectNode.InnerText));
					break;
				case AspectBinaryEncodingType.ASCII85:
					aspect.SetContent(ASCII85.Decode(aspectNode.InnerText));
					break;
				}
			}
		}
	}
}
