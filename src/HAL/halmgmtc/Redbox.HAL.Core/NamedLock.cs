using System;
using System.Globalization;
using System.Threading;

namespace Redbox.HAL.Core;

public sealed class NamedLock : IDisposable
{
	private Mutex m_instanceMutex;

	private readonly string m_name;

	private readonly LockScope m_scope;

	public bool IsOwned { get; private set; }

	public NamedLock(string name, LockScope scope)
	{
		m_name = name;
		m_scope = scope;
		try
		{
			m_instanceMutex = new Mutex(initiallyOwned: false, string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", m_scope, m_name), out var createdNew);
			IsOwned = createdNew;
		}
		catch
		{
			IsOwned = false;
		}
	}

	public void Dispose()
	{
		if (m_instanceMutex != null)
		{
			m_instanceMutex.Close();
			m_instanceMutex = null;
		}
	}
}
