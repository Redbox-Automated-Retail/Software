using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[ElementName("add")]
public class AddElement : Element
{
	private string _keyName;

	private string _keyValue;

	private bool _preserveExisting = false;

	[TaskAttribute("key-name", Required = true)]
	public string KeyName
	{
		get
		{
			return _keyName;
		}
		set
		{
			_keyName = value;
		}
	}

	[TaskAttribute("key-value", Required = true)]
	public string KeyValue
	{
		get
		{
			return _keyValue;
		}
		set
		{
			_keyValue = value;
		}
	}

	[TaskAttribute("preserve", Required = false)]
	public bool PreserveExisting
	{
		get
		{
			return _preserveExisting;
		}
		set
		{
			_preserveExisting = value;
		}
	}
}
