using System.Collections.Generic;

namespace Redbox.HAL.Core;

internal class BufferManager
{
	internal static BufferManager Instance => Singleton<BufferManager>.Instance;

	private List<byte[]> Pool { get; set; }

	private object PoolLock { get; set; }

	internal byte[] Allocate()
	{
		lock (PoolLock)
		{
			return null;
		}
	}

	internal void Return(byte[] buffer)
	{
	}

	private BufferManager()
	{
		Pool = new List<byte[]>();
		PoolLock = new object();
	}
}
