using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskEvents;

public class KioskEventService : IKioskEventService
{
	private List<KioskEvent> _kioskEvents = new List<KioskEvent>();

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	private Timer _changeTimer;

	private int _inRefresh;

	private string _kioskEventsFileName;

	private IStoreManager _storeManagerService;

	public KioskEventService(IStoreManager storeManagerService)
	{
		_storeManagerService = storeManagerService;
		string dataPath = ServiceLocator.Instance.GetService<IEngineApplication>().DataPath;
		_kioskEventsFileName = Path.Combine(dataPath, "kioskevents.json");
		if (_changeTimer == null)
		{
			_changeTimer = new Timer(delegate
			{
				OnTimer();
			});
		}
	}

	public void AddEvent(KioskEvent kioskEvent)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("KioskEventService.AddEvent", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			_kioskEvents.Add(kioskEvent);
			try
			{
				LogHelper.Instance.Log("KioskEventService.AddEvent - " + kioskEvent.ToJson().ToPrettyJson());
			}
			catch
			{
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("KioskEventService.AddEvent - an exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void Start()
	{
		try
		{
			LogHelper.Instance.Log("KioskEventService.Start");
			Load();
			int num = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth.KioskEventsUnloadInterval;
			LogHelper.Instance.Log("KioskEventService Config - KioskEventUnloadInterval - {0} minutes", num);
			if (num < 3)
			{
				num = 3;
			}
			if (num > 720)
			{
				num = 720;
			}
			int num2 = new Random().Next(0, 59);
			int num3 = new Random().Next(1, num);
			_changeTimer.Change(new TimeSpan(0, num3, num2), new TimeSpan(0, num, 0));
			LogHelper.Instance.Log("KioskEventService.Start - Unload timer set to start in {0}:{1} and to run again in {2}:00 ", num3, num2, num);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("KioskEventService.Start - an exception occurred.", e);
		}
	}

	public void Stop()
	{
		try
		{
			LogHelper.Instance.Log("KioskEventService.Stop");
			Save();
			_changeTimer.Change(-1, -1);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("KioskEventService.Stop - an exception occurred.", e);
		}
	}

	public void Restart()
	{
		Stop();
		Start();
	}

	private void OnTimer()
	{
		if (Interlocked.CompareExchange(ref _inRefresh, 1, 0) == 1)
		{
			LogHelper.Instance.Log("KioskEventService.Refresh - exiting already in OnTimer");
			return;
		}
		try
		{
			Unload();
		}
		finally
		{
			_inRefresh = 0;
		}
	}

	private void Unload()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("KioskEventService.Unload", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			PerformUnload();
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void PerformUnload()
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			LogHelper.Instance.Log("KioskEventService.PerformUnload - IHealthServices was not found");
			return;
		}
		LogHelper.Instance.Log("KioskEventService.PerformUnload - unloading all {0} events to send to kiosk services", _kioskEvents.Count);
		int count = 20;
		new List<KioskEvent>();
		while (_kioskEvents.Count > 0)
		{
			try
			{
				List<KioskEvent> list = _kioskEvents.Take(count).ToList();
				_kioskEvents.RemoveRange(0, list.Count());
				service.SendKioskEvents(_storeManagerService.KioskId, list);
				LogHelper.Instance.Log("KioskEventService.PerformUnload - unloading {0} events", list.Count());
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("KioskEventService.PerformUnload - an exception occurred unloading events, clearing all events.", e);
				_kioskEvents.Clear();
			}
		}
	}

	private void Save()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("KioskEventService.Save", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			WriteData(_kioskEvents);
			_kioskEvents.Clear();
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void Load()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("KioskEventService.Load", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			List<KioskEvent> list = ReadData();
			if (list != null)
			{
				list.ForEach(delegate(KioskEvent item)
				{
					_kioskEvents.Add(item);
				});
				LogHelper.Instance.Log("KioskEventService.Load - count {0}", list.Count);
			}
			DeleteData();
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void WriteData(List<KioskEvent> data)
	{
		try
		{
			if (data.Count == 0)
			{
				DeleteData();
				return;
			}
			LogHelper.Instance.Log("KioskEventService.WriteData = Writing events - count {0}", data.Count);
			string s = data.ToJson();
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			File.WriteAllBytes(_kioskEventsFileName, bytes);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"KioskEventService.WriteData - an exception occurred writing {_kioskEventsFileName}.", e);
		}
	}

	private List<KioskEvent> ReadData()
	{
		try
		{
			if (!File.Exists(_kioskEventsFileName))
			{
				return null;
			}
			byte[] bytes = File.ReadAllBytes(_kioskEventsFileName);
			return Encoding.ASCII.GetString(bytes).ToObject<List<KioskEvent>>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"KioskEventService.ReadData - an exception occurred reading {_kioskEventsFileName}.", e);
		}
		return null;
	}

	private void DeleteData()
	{
		try
		{
			if (File.Exists(_kioskEventsFileName))
			{
				File.Delete(_kioskEventsFileName);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"KioskEventService.DeleteData - an exception occurred deleting {_kioskEventsFileName}.", e);
		}
	}
}
