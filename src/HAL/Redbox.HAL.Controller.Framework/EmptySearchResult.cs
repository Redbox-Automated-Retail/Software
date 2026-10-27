using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmptySearchResult : IEmptySearchResult, IDisposable
{
	private bool m_disposed;

	public IList<ILocation> EmptyLocations { get; private set; }

	public int FoundEmpty => EmptyLocations.Count;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			EmptyLocations.Clear();
			GC.SuppressFinalize(this);
		}
	}

	internal EmptySearchResult()
	{
		EmptyLocations = new List<ILocation>();
	}
}
