using System;
using System.Threading;

namespace Redbox.HAL.Component.Model.Threading;

public sealed class CountdownEvent : IDisposable
{
	private readonly ManualResetEvent Event = new ManualResetEvent(initialState: false);

	private readonly int m_initialCount;

	private int m_currentCount;

	private bool m_disposed;

	public bool IsSet => Thread.VolatileRead(ref m_currentCount) <= 0;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			Event.Close();
		}
	}

	public void Signal()
	{
		if (Interlocked.Decrement(ref m_currentCount) == 0)
		{
			Event.Set();
		}
	}

	public void Wait()
	{
		Event.WaitOne();
	}

	public CountdownEvent(int waiterCount)
	{
		m_initialCount = (m_currentCount = waiterCount);
	}
}
