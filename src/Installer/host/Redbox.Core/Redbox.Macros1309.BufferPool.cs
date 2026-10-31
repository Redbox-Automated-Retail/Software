using System.Collections.Generic;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EBufferPool
{
	private int m_bufferSize;

	private int m_numberOfBuffers;

	private Queue<byte[]> m_freeBuffers;

	public static Redbox_002EMacros1309_002EBufferPool Instance => Redbox_002EMacros1309_002ESingleton<Redbox_002EMacros1309_002EBufferPool>.Instance;

	public void Initialize(int bufferSize, int numberOfBuffers)
	{
		m_bufferSize = bufferSize;
		m_numberOfBuffers = numberOfBuffers;
		m_freeBuffers = new Queue<byte[]>(m_numberOfBuffers);
		for (int i = 0; i < m_numberOfBuffers; i++)
		{
			m_freeBuffers.Enqueue(new byte[m_bufferSize]);
		}
	}

	public void Shutdown()
	{
		lock (m_freeBuffers)
		{
			m_freeBuffers.Clear();
		}
	}

	public byte[] Checkout()
	{
		if (m_freeBuffers.Count > 0)
		{
			lock (m_freeBuffers)
			{
				if (m_freeBuffers.Count > 0)
				{
					return m_freeBuffers.Dequeue();
				}
			}
		}
		return new byte[m_bufferSize];
	}

	public void Checkin(byte[] buffer)
	{
		if (buffer == null)
		{
			return;
		}
		lock (m_freeBuffers)
		{
			if (!m_freeBuffers.Contains(buffer))
			{
				m_freeBuffers.Enqueue(buffer);
			}
		}
	}

	private Redbox_002EMacros1309_002EBufferPool()
	{
	}
}
