using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class IdleTimerService : IIdleTimerService
{
	private Dictionary<string, IIdleTimer> _innerIdleTimers = new Dictionary<string, IIdleTimer>();

	private readonly object _syncLock = new object();

	public static IdleTimerService Instance => Singleton<IdleTimerService>.Instance;

	public void Reset()
	{
		LogHelper.Instance.Log("Reset idletimer service.");
		lock (_syncLock)
		{
			_innerIdleTimers.Clear();
		}
	}

	public IIdleTimer CreateIdleTimer(string name)
	{
		lock (_syncLock)
		{
			IIdleTimer value = null;
			_innerIdleTimers.TryGetValue(name, out value);
			if (value == null)
			{
				value = new IdleTimer(name);
				_innerIdleTimers.Add(name, value);
			}
			return value;
		}
	}

	public void RemoveIdleTimer(string name)
	{
		lock (_syncLock)
		{
			if (_innerIdleTimers.ContainsKey(name))
			{
				_innerIdleTimers[name]?.Stop();
				ServiceLocator.Instance.GetService<ITimerService>()?.RemoveTimer(name);
				_innerIdleTimers.Remove(name);
			}
		}
	}

	public IIdleTimer GetIdleTimer(string name)
	{
		lock (_syncLock)
		{
			return _innerIdleTimers.ContainsKey(name) ? _innerIdleTimers[name] : null;
		}
	}

	private IdleTimerService()
	{
	}
}
