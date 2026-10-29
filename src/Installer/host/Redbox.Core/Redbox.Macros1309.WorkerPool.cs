using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EWorkerPool
{
	internal sealed class WorkItemEntry
	{
		public Redbox_002EMacros1309_002EWorkItem Item;

		public object State;

		public void Execute()
		{
			if (Item != null)
			{
				Item(State);
			}
		}
	}

	private int m_runningThreadCount;

	private volatile bool m_isRunning;

	private readonly object m_syncObject = new object();

	private readonly Queue<WorkItemEntry> m_workItems = new Queue<WorkItemEntry>();

	public string PoolName { get; private set; }

	public int MinimumThreads { get; private set; }

	public int MaximumThreads { get; private set; }

	public Redbox_002EMacros1309_002EWorkerPool(int minThreads, int maxThreads)
		: this(minThreads, maxThreads, "IPC Worker Pool")
	{
	}

	public Redbox_002EMacros1309_002EWorkerPool(int minThreads, int maxThreads, string poolName)
	{
		MinimumThreads = minThreads;
		MaximumThreads = maxThreads;
		PoolName = poolName;
	}

	public void Start()
	{
		m_workItems.Clear();
		m_isRunning = true;
		for (int i = 0; i < MaximumThreads; i++)
		{
			Thread thread = new Thread(ItemWorker);
			thread.Name = $"{PoolName} #{i}";
			thread.IsBackground = true;
			Thread thread2 = thread;
			thread2.Start();
		}
	}

	public void Shutdown()
	{
		lock (m_syncObject)
		{
			m_isRunning = false;
			Monitor.PulseAll(m_syncObject);
		}
	}

	public bool QueueWorkItem(Redbox_002EMacros1309_002EWorkItem item)
	{
		return QueueWorkItem(item, null);
	}

	public bool QueueWorkItem(Redbox_002EMacros1309_002EWorkItem item, object state)
	{
		lock (m_syncObject)
		{
			if (!m_isRunning || item == null || m_runningThreadCount + 1 > MaximumThreads)
			{
				return false;
			}
			m_workItems.Enqueue(new WorkItemEntry
			{
				Item = item,
				State = state
			});
			Monitor.PulseAll(m_syncObject);
			return true;
		}
	}

	internal WorkItemEntry GetWorkItem()
	{
		lock (m_syncObject)
		{
			while (m_workItems.Count == 0)
			{
				if (!m_isRunning)
				{
					return null;
				}
				Monitor.Wait(m_syncObject);
			}
			return m_workItems.Dequeue();
		}
	}

	private void ItemWorker()
	{
		while (m_isRunning)
		{
			try
			{
				WorkItemEntry workItem = GetWorkItem();
				if (workItem != null)
				{
					try
					{
						Interlocked.Increment(ref m_runningThreadCount);
						workItem.Execute();
					}
					finally
					{
						Interlocked.Decrement(ref m_runningThreadCount);
					}
				}
			}
			catch (Exception e)
			{
				Redbox_002EMacros1309_002ELogHelper.Instance.Log("An unhandled exception was raised in WorkerPool.ItemWorker.", e);
			}
		}
	}
}
