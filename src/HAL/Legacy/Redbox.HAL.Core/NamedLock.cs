using System;
using System.Globalization;
using System.Threading;

namespace Redbox.HAL.Core;

public class NamedLock : IDisposable
{
	private bool m_ownsMutex;

	private Mutex m_instanceMutex;

	private readonly string m_name;

	private readonly LockScope m_scope;

	public bool IsOwned
	{
		get
		{
			if (m_instanceMutex != null)
			{
				return m_ownsMutex;
			}
			return false;
		}
	}

	public NamedLock(string name, LockScope scope)
	{
		m_name = name;
		m_scope = scope;
		AcquireLock();
	}

	public void Dispose()
	{
		m_instanceMutex.Close();
		m_instanceMutex = null;
	}

	internal void AcquireLock()
	{
		m_instanceMutex = new Mutex(initiallyOwned: false, string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", m_scope, m_name), out m_ownsMutex);
	}
}
