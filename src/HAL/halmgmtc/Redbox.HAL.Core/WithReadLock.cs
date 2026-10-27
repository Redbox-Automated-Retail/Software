using System;
using System.Threading;

namespace Redbox.HAL.Core;

public struct WithReadLock : IDisposable
{
	private readonly ReaderWriterLockSlim TheLock;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			TheLock.ExitReadLock();
		}
	}

	public WithReadLock(ReaderWriterLockSlim _lock)
	{
		this = default(WithReadLock);
		Disposed = false;
		TheLock = _lock;
		TheLock.EnterReadLock();
	}
}
