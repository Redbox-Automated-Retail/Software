using System;

namespace Redbox.HAL.Component.Model.Threading;

public struct AtomicFlagHelper : IDisposable
{
	private bool Disposed;

	private readonly AtomicFlag Flag;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Flag.Clear();
		}
	}

	public AtomicFlagHelper(AtomicFlag flag)
	{
		this = default(AtomicFlagHelper);
		Disposed = false;
		Flag = flag;
	}
}
