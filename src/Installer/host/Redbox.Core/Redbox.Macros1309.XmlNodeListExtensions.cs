using System;
using System.Xml;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EXmlNodeListExtensions
{
	public static void ForEach(this XmlNodeList nodelist, Action<XmlNode> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		int count = nodelist.Count;
		for (int i = 0; i < count; i++)
		{
			action(nodelist[i]);
		}
	}
}
