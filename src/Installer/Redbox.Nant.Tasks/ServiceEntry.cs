using System.Xml;

namespace Redbox.Nant.Tasks;

public class ServiceEntry
{
	private XmlNode _node;

	private string _type;

	private string _mode = "SingleCall";

	public string Mode => _mode;

	public string Type => _type;

	public ServiceEntry(XmlNode node)
	{
		_node = node;
	}
}
