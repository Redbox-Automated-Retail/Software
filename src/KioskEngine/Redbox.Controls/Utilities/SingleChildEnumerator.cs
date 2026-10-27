using System.Collections;

namespace Redbox.Controls.Utilities;

public class SingleChildEnumerator : IEnumerator
{
	private int _index = -1;

	private int _count;

	private object _child;

	object IEnumerator.Current
	{
		get
		{
			if (_index == 0)
			{
				return _child;
			}
			return null;
		}
	}

	public SingleChildEnumerator(object Child)
	{
		_child = Child;
		_count = ((Child != null) ? 1 : 0);
	}

	bool IEnumerator.MoveNext()
	{
		_index++;
		return _index < _count;
	}

	void IEnumerator.Reset()
	{
		_index = -1;
	}
}
