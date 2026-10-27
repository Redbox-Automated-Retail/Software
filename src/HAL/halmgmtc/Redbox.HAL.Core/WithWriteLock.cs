using System;
using System.Threading;

namespace Redbox.HAL.Core;

public struct WithWriteLock : IDisposable
{
	private readonly ReaderWriterLockSlim TheLock;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			TheLock.ExitWriteLock();
		}
	}

	public WithWriteLock(ReaderWriterLockSlim _lock)
	{
		this = default(WithWriteLock);
		Disposed = false;
		TheLock = _lock;
		TheLock.EnterWriteLock();
	}
}
