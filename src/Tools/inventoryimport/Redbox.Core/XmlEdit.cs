namespace Redbox.Core;

public sealed class XmlEdit
{
	public string XPath { get; set; }

	public string Value { get; set; }

	public XmlEdit(string xpath, string value)
	{
		XPath = xpath;
		Value = value;
	}
}
