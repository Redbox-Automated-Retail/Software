using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Script.Framework;

internal sealed class ExecutionContextList
{
	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private readonly List<ExecutionContext> m_list = new List<ExecutionContext>();

	public ExecutionContext GetByID(string id)
	{
		using (new WithReadLock(Lock))
		{
			return m_list.Find((ExecutionContext each) => string.Compare(id, each.ID, ignoreCase: false) == 0);
		}
	}

	public ExecutionContext[] GetByLabel(string label)
	{
		using (new WithReadLock(Lock))
		{
			List<ExecutionContext> list = m_list.FindAll((ExecutionContext each) => string.Compare(each.Label, label, ignoreCase: true) == 0);
			using (new DisposeableList<ExecutionContext>(list))
			{
				return list.ToArray();
			}
		}
	}

	public void ForEach(Action<ExecutionContext> action)
	{
		using (new WithReadLock(Lock))
		{
			m_list.ForEach(action);
		}
	}

	public ExecutionContext[] GetByName(string name)
	{
		List<ExecutionContext> res = new List<ExecutionContext>();
		using (new DisposeableList<ExecutionContext>(res))
		{
			using (new WithReadLock(Lock))
			{
				m_list.ForEach(delegate(ExecutionContext each)
				{
					if (each.ProgramName.Equals(name))
					{
						res.Add(each);
					}
				});
				return res.ToArray();
			}
		}
	}

	public int CleanupGarbage(bool force)
	{
		int cleaned = 0;
		using (new WithWriteLock(Lock))
		{
			List<ExecutionContext> list = m_list.FindAll((ExecutionContext each) => (force && each.IsComplete) || each.Status == ExecutionContextStatus.Garbage);
			using (new DisposeableList<ExecutionContext>(list))
			{
				list.ForEach(delegate(ExecutionContext g)
				{
					if (!g.IsImmediate)
					{
						g.Dispose();
						m_list.Remove(g);
						int num = cleaned + 1;
						cleaned = num;
					}
				});
			}
		}
		return cleaned;
	}

	public ExecutionContext GetEligibleExecutionContext()
	{
		using (new WithReadLock(Lock))
		{
			List<ExecutionContext> list = m_list.FindAll((ExecutionContext context) => context.EligibleForScheduling);
			list.Sort(delegate(ExecutionContext x, ExecutionContext y)
			{
				int num = x.Priority.CompareTo(y.Priority);
				return (num != 0) ? num : x.CreatedOn.CompareTo(y.CreatedOn);
			});
			foreach (ExecutionContext item in list)
			{
				if (item.StartTime.HasValue)
				{
					if (DateTime.Now.CompareTo(item.StartTime) >= 0)
					{
						return item;
					}
					continue;
				}
				return item;
			}
			return null;
		}
	}

	public void Add(ExecutionContext item)
	{
		using (new WithWriteLock(Lock))
		{
			m_list.Add(item);
		}
	}

	public ExecutionContext Find(Predicate<ExecutionContext> predicate)
	{
		using (new WithReadLock(Lock))
		{
			return m_list.Find(predicate);
		}
	}

	public List<ExecutionContext> FindAll(Predicate<ExecutionContext> predicate)
	{
		using (new WithReadLock(Lock))
		{
			return m_list.FindAll(predicate);
		}
	}
}
