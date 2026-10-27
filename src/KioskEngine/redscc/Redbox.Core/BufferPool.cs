using System.Collections.Generic;

namespace Redbox.Core;

public class BufferPool
{
	private int m_bufferSize;

	private int m_numberOfBuffers;

	private Queue<byte[]> m_freeBuffers;

	public static BufferPool Instance => Singleton<BufferPool>.Instance;

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

	private BufferPool()
	{
	}
}
