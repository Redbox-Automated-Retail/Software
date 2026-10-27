using System;
using System.Globalization;
using System.Threading;

namespace Redbox.Core;

public class NamedLock : IDisposable
{
	private bool m_ownsMutex;

	private Mutex m_instanceMutex;

	private readonly string m_name;

	private readonly LockScope m_scope;

	public NamedLock(string name, LockScope scope)
	{
		m_name = name;
		m_scope = scope;
		AcquireLock();
	}

	public bool Exists()
	{
		return m_instanceMutex != null && m_ownsMutex;
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
