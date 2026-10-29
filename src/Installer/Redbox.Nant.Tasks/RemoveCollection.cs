using System.Collections;

namespace Redbox.Nant.Tasks;

public class RemoveCollection : CollectionBase
{
	public RemoveElement this[int index] => (RemoveElement)base.InnerList[index];

	public int Add(RemoveElement element)
	{
		return base.InnerList.Add(element);
	}
}
