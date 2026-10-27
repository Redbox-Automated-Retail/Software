using System;
using System.Collections.Generic;

namespace Redbox.HAL.Core;

public struct DisposeableList<T> : IDisposable
{
	private readonly IList<T> TheList;

	private bool m_disposed;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			TheList.Clear();
		}
	}

	public DisposeableList(IList<T> list)
	{
		this = default(DisposeableList<T>);
		m_disposed = false;
		TheList = list;
	}
}
