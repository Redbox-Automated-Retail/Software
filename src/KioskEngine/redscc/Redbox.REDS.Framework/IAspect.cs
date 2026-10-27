using System.Collections.Generic;
using System.Xml;

namespace Redbox.REDS.Framework;

public interface IAspect
{
	string Name { get; }

	IAspectType Type { get; }

	AspectFlags Flags { get; }

	IDictionary<string, string> Properties { get; }

	object GetContent();

	void SetContent(object value);

	ErrorList Save(XmlTextWriter writer);
}
