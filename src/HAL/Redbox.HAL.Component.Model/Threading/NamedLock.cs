using System;
using System.Globalization;
using System.Threading;

namespace Redbox.HAL.Component.Model.Threading;

public sealed class NamedLock : IDisposable
{
	private readonly Mutex m_instanceMutex;

	public bool IsOwned { get; private set; }

	public void Dispose()
	{
		if (m_instanceMutex != null)
		{
			m_instanceMutex.Close();
		}
	}

	public NamedLock(string name)
	{
		try
		{
			m_instanceMutex = new Mutex(initiallyOwned: false, string.Format(CultureInfo.InvariantCulture, "Local\\{0}", name), out var createdNew);
			IsOwned = createdNew;
		}
		catch
		{
			IsOwned = false;
		}
	}
}
