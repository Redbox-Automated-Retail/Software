using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class TimerService : ITimerService
{
	private IDictionary<string, SoftTimer> m_innerTimers;

	private readonly object m_syncLock = new object();

	public static TimerService Instance => Singleton<TimerService>.Instance;

	public ReadOnlyCollection<ITimer> Timers
	{
		get
		{
			List<ITimer> list = new List<ITimer>();
			foreach (KeyValuePair<string, SoftTimer> innerTimer in InnerTimers)
			{
				list.Add(innerTimer.Value);
			}
			return list.AsReadOnly();
		}
	}

	internal IDictionary<string, SoftTimer> InnerTimers
	{
		get
		{
			if (m_innerTimers == null)
			{
				m_innerTimers = new Dictionary<string, SoftTimer>();
			}
			return m_innerTimers;
		}
	}

	public void Reset()
	{
		LogHelper.Instance.Log("Reset timer service.");
		lock (m_syncLock)
		{
			InnerTimers.Clear();
		}
	}

	public void StopAll()
	{
		lock (m_syncLock)
		{
			foreach (SoftTimer value in InnerTimers.Values)
			{
				value.Stop();
			}
		}
	}

	public void StartAll()
	{
		lock (m_syncLock)
		{
			foreach (SoftTimer value in InnerTimers.Values)
			{
				value.Start();
			}
		}
	}

	public ITimer CreateTimer(string name, int? dueTime, int? period, TimerCallback callback)
	{
		lock (m_syncLock)
		{
			SoftTimer softTimer;
			if (InnerTimers.ContainsKey(name))
			{
				softTimer = InnerTimers[name];
			}
			else
			{
				softTimer = new SoftTimer
				{
					DueTime = dueTime,
					Period = period,
					Name = name
				};
				InnerTimers[name] = softTimer;
			}
			softTimer.ClearFire();
			softTimer.Fire += callback;
			return softTimer;
		}
	}

	public void RemoveTimer(string name)
	{
		lock (m_syncLock)
		{
			InnerTimers.Remove(name);
		}
	}

	public ITimer GetTimer(string name)
	{
		lock (m_syncLock)
		{
			return InnerTimers.ContainsKey(name) ? InnerTimers[name] : null;
		}
	}

	public void UpdateTimers()
	{
		lock (m_syncLock)
		{
			List<ITimer> list = new List<ITimer>();
			foreach (SoftTimer value in InnerTimers.Values)
			{
				list.Add(value);
			}
			for (int i = 0; i < list.Count && i < list.Count; i++)
			{
				((SoftTimer)list[i]).UpdateTimer();
			}
		}
	}

	public void Wait(int milliseconds)
	{
		ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
		manualResetEvent.WaitOne(milliseconds);
		manualResetEvent.Close();
	}

	private TimerService()
	{
	}
}
