using System.Collections;

namespace Redbox.Nant.Tasks;

public class AddCollection : CollectionBase
{
	public AddElement this[int index] => (AddElement)base.InnerList[index];

	public int Add(AddElement element)
	{
		return base.InnerList.Add(element);
	}
}
