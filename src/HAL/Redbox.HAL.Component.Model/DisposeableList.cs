using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public struct DisposeableList<T> : IDisposable
{
	private bool m_disposed;

	private readonly IList<T> TheList;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			if (TheList != null && TheList.Count > 0)
			{
				TheList.Clear();
			}
		}
	}

	public DisposeableList(IList<T> list)
	{
		this = default(DisposeableList<T>);
		m_disposed = false;
		TheList = list;
	}
}
