using System;
using System.Collections;

namespace Redbox.Controls.Utilities;

public class EmptyEnumerator : IEnumerator
{
	private static IEnumerator _instance;

	public static IEnumerator Instance => GetInstance();

	public object Current
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	private static IEnumerator GetInstance()
	{
		if (_instance == null)
		{
			return _instance = new EmptyEnumerator();
		}
		return _instance;
	}

	private EmptyEnumerator()
	{
	}

	public void Reset()
	{
	}

	public bool MoveNext()
	{
		return false;
	}
}
