using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment;

public class HealthService : IHealthService
{
	private class HealthItem
	{
		private int InCheckup;

		private string Key { get; set; }

		private DateTime LastCheckup { get; set; }

		private TimeSpan ExpireTime { get; set; }

		private Action Rescue { get; set; }

		public HealthItem(string key, TimeSpan expireTime, Action rescue)
		{
			LastCheckup = DateTime.Now;
			ExpireTime = expireTime;
			Rescue = rescue;
			Key = key;
		}

		internal void CheckHealth()
		{
			Checkup();
		}

		internal void UpdateHealth()
		{
			LastCheckup = DateTime.Now;
			LogHelper.Instance.Log("Health service: Updating {0}", Key);
		}

		private bool IsExpired()
		{
			return LastCheckup.Add(ExpireTime) < DateTime.Now;
		}

		private void Checkup()
		{
			try
			{
				if (Interlocked.CompareExchange(ref InCheckup, 1, 0) == 1)
				{
					LogHelper.Instance.Log($"HealthItem Checkup: {Key} already executing.");
					return;
				}
				try
				{
					if (IsExpired())
					{
						Rescue();
						UpdateHealth();
					}
				}
				finally
				{
					InCheckup = 0;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("There was an exception in HealthService.Checkup", e);
			}
		}
	}

	private Dictionary<string, HealthItem> _healthItemList = new Dictionary<string, HealthItem>();

	private Timer _healthTimer;

	private bool _isRunning;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	public static HealthService Instance => Singleton<HealthService>.Instance;

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ServiceLocator.Instance.AddService(typeof(IHealthService), this);
			_healthTimer = new Timer(delegate
			{
				UpdateHealth();
			});
			_isRunning = false;
			LogHelper.Instance.Log("Initialized the health service", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS999", "An unhandled exception occurred while initializing the health service.", e));
		}
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_healthTimer.Change(new TimeSpan(0L), new TimeSpan(0, 0, 60));
			_isRunning = true;
			LogHelper.Instance.Log("Starting the health service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS998", "An unhandled exception occurred while starting the health service.", e));
		}
		return errorList;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_healthTimer.Change(-1, -1);
			_isRunning = false;
			LogHelper.Instance.Log("Stopping the health service.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS997", "An unhandled exception occurred while stopping the health service.", e));
		}
		return errorList;
	}

	public ErrorList Add(string key, TimeSpan expireTime, Action rescue)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (!_lock.TryEnterWriteLock(_lockTimeout))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HealthService.Add", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
				return errorList;
			}
			try
			{
				if (_healthItemList.ContainsKey(key))
				{
					return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("HS8002", "An unhandled exception occurred in health service", $"Exception occurred when trying to add a health item with a duplicate key {key}") };
				}
				_healthItemList.Add(key, new HealthItem(key, expireTime, rescue));
				return new ErrorList();
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS996", "An unhandled exception occurred while adding a key to the health service.", e));
			return errorList;
		}
	}

	public ErrorList Remove(string key)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (!_lock.TryEnterWriteLock(_lockTimeout))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HealthService.Remove", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
				return errorList;
			}
			try
			{
				if (_healthItemList.TryGetValue(key, out var _))
				{
					_healthItemList.Remove(key);
					LogHelper.Instance.Log("Health service: Removing {0}", key);
					return new ErrorList();
				}
				return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("HS8001", "An unhandled exception occurred in health service", $"Exception occurred when trying to remove a health item with a non-existent key {key}") };
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS995", "An unhandled exception occurred while removing a key to the health service.", e));
			return errorList;
		}
	}

	public ErrorList Update(string key)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (!_lock.TryEnterWriteLock(_lockTimeout))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HealthService.Update", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
				return errorList;
			}
			try
			{
				if (_healthItemList.TryGetValue(key, out var value))
				{
					value.UpdateHealth();
					return new ErrorList();
				}
				return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("HS8000", "An unhandled exception occurred in health service", $"Exception occurred when trying to update a health item with a non-existent key {key}") };
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS994", "An unhandled exception occurred while updating a key to the health service.", e));
			return errorList;
		}
	}

	private HealthService()
	{
	}

	private void UpdateHealth()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			Redbox.UpdateManager.ComponentModel.Error.NewError("HealthService.UpdateHealth", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (!_isRunning)
			{
				return;
			}
			_healthItemList.ForEach(delegate(KeyValuePair<string, HealthItem> key)
			{
				try
				{
					key.Value.CheckHealth();
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("HS996 An unhandled exception was raised checking the health for {0}", e, key.Key);
				}
			});
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}
}
