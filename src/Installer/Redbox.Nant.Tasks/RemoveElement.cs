using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[ElementName("remove")]
public class RemoveElement : Element
{
	private string _keyName;

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
}
