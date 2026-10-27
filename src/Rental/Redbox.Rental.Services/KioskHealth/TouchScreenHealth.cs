using System;
using System.Threading;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskHealth;

public class TouchScreenHealth : ITouchScreenHealth, IKioskHealthItem
{
	private IKioskEventService _kioskEvents;

	private IResourceBundleService _resourceBundleService;

	private IViewService _viewService;

	private bool _touchScreenActive;

	private bool _touchScreenReporting;

	private bool _touchScreenHealing;

	public TimeSpan _touchScreenStartCheck;

	public TimeSpan _touchScreenEndCheck;

	public int _touchScreenInActivity;

	private int _resetsBeforeAlert;

	private Timer _changeTimer;

	private DateTime _inActiveFrom;

	private bool _resetOccurred;

	private Guid? _relatedId;

	private int _contiguousResetCount;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 5000;

	private bool IsOnStartScreen
	{
		get
		{
			bool flag = false;
			if (_resourceBundleService != null)
			{
				flag = _resourceBundleService.IsRentalRunning;
			}
			bool flag2 = false;
			if (_viewService != null)
			{
				flag2 = _viewService.Peek() == _viewService.GetStartOverViewName();
			}
			return flag & flag2;
		}
	}

	private bool IsInactive => (DateTime.Now - _inActiveFrom).TotalMinutes >= (double)_touchScreenInActivity;

	public TouchScreenHealth(IKioskEventService kioskEvents)
	{
		_kioskEvents = kioskEvents;
		if (_changeTimer == null)
		{
			_changeTimer = new Timer(delegate
			{
				NoActivity();
			});
		}
		_resourceBundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
		_viewService = ServiceLocator.Instance.GetService<IViewService>();
	}

	public void Start()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("TouchScreenHealth.Start", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("TouchScreenHealth.Start - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void Stop()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("TouchScreenHealth.Stop", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStop();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("TouchScreenHealth.Stop - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void InternalStart()
	{
		try
		{
			ReadConfiguration();
			if (!_touchScreenActive)
			{
				LogHelper.Instance.Log("TouchScreenHealth is not active");
				return;
			}
			LogHelper.Instance.Log("TouchScreenHealth.InternalStart");
			Reset();
			ResetInActiveFrom();
			_changeTimer.Change(new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 0));
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("TouchScreenHealth.InternalStart - exception occurred", e);
		}
	}

	private void InternalStop()
	{
		try
		{
			LogHelper.Instance.Log("TouchScreenHealth.InternalStop");
			_changeTimer.Change(-1, -1);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("TouchScreenHealth.InternalStop - exception occurred", e);
		}
	}

	public void Restart()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("TouchScreenHealth.Restart", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LogHelper.Instance.Log("TouchScreenHealth.ReStart");
			InternalStop();
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("TouchScreenHealth.Restart - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void PostActivity()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("TouchScreenHealth.PostActivity", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LogHelper.Instance.Log("TouchScreenHealth.PostActivity");
			ResetInActiveFrom();
			if (_resetOccurred)
			{
				if (_touchScreenReporting)
				{
					IKioskEventService kioskEvents = _kioskEvents;
					Guid? relatedId = _relatedId;
					kioskEvents.AddEvent(KioskEvent.NewEvent(1, 102, success: true, null, relatedId));
				}
				Reset();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("TouchScreenHealth.PostActivity - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void NoActivity()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("TouchScreenHealth.NoActivity", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			DateTime now = DateTime.Now;
			if (now.TimeOfDay >= _touchScreenStartCheck && now.TimeOfDay < _touchScreenEndCheck && IsOnStartScreen)
			{
				if (!IsInactive)
				{
					return;
				}
				_contiguousResetCount++;
				LogHelper.Instance.Log("TouchScreenHealth.NoActivity - Kiosk touch screen has been inactive for {0} minutes between {1} and {2}. There are {3} resets without activity.", _touchScreenInActivity, _touchScreenStartCheck, _touchScreenEndCheck, _contiguousResetCount);
				if (_touchScreenReporting)
				{
					string data = new
					{
						Inactivity = _touchScreenInActivity
					}.ToJson();
					KioskEvent kioskEvent = KioskEvent.NewEvent(1, 100, success: true, null, null, data);
					if (!_relatedId.HasValue)
					{
						_relatedId = kioskEvent.Id;
						kioskEvent.RelatedId = _relatedId;
					}
					kioskEvent.RelatedId = _relatedId;
					_kioskEvents.AddEvent(kioskEvent);
					if (_contiguousResetCount % _resetsBeforeAlert == 0)
					{
						SendAlert(kioskEvent);
					}
				}
				CorrectProblem();
			}
			else
			{
				ResetInActiveFrom();
				_contiguousResetCount = 0;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("TouchScreenHealth.NoActivity - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void SendAlert(KioskEvent kioskEvent, Action action = null)
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service != null)
		{
			IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
			if (service2 != null)
			{
				service.SendAlert(service2.KioskId.ToString(), "ApplicationCrash", "TouchScreenInactivity", kioskEvent.Data, DateTime.Now, null, action);
			}
		}
	}

	private void CorrectProblem()
	{
		_resetOccurred = true;
		if (_touchScreenHealing)
		{
			LogHelper.Instance.Log("TouchScreenHealth.CorrectProblem - Asking HAL to reset the touchscreen");
			bool flag = true;
			string error = string.Empty;
			int? elapsed = null;
			IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
			if (service == null)
			{
				LogHelper.Instance.Log("TouchScreenHealth.CorrectProblem - IHardwareService is not available.");
				flag = false;
			}
			else
			{
				DateTime now = DateTime.Now;
				string jobId = null;
				ErrorList errorList = service.ResetTouchScreenController(delegate
				{
				}, delegate(string jobIdentity, string status)
				{
					LogHelper.Instance.Log($"Message: {status}", LogEntryType.Info);
				}, out jobId);
				if (errorList != null && errorList.Count > 0)
				{
					flag = false;
					Redbox.KioskEngine.ComponentModel.Error error2 = errorList[0];
					error = $"Code: {error2.Code} Description: {error2.Description} Details: {error2.Details}";
				}
				elapsed = (int)(DateTime.Now - now).TotalMilliseconds;
			}
			if (_touchScreenReporting)
			{
				KioskEvent kioskEvent = KioskEvent.NewEvent(1, 101, flag, elapsed, _relatedId, flag ? null : new
				{
					Error = error
				}.ToJson());
				_kioskEvents.AddEvent(kioskEvent);
			}
		}
		ResetInActiveFrom();
	}

	private void ResetInActiveFrom()
	{
		_inActiveFrom = DateTime.Now;
	}

	private void Reset()
	{
		_resetOccurred = false;
		_relatedId = null;
		_contiguousResetCount = 0;
	}

	private void ReadConfiguration()
	{
		KioskHealthConfiguration kioskHealth = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth;
		_touchScreenActive = kioskHealth.TouchScreenActive;
		_touchScreenReporting = kioskHealth.TouchScreenReporting;
		_touchScreenHealing = kioskHealth.TouchScreenHealing;
		_touchScreenStartCheck = GetTimeSpan(kioskHealth.TouchScreenStartCheck, new TimeSpan(16, 0, 0));
		_touchScreenEndCheck = GetTimeSpan(kioskHealth.TouchScreenEndCheck, new TimeSpan(22, 0, 0));
		_touchScreenInActivity = kioskHealth.TouchScreenInActivity;
		_resetsBeforeAlert = kioskHealth.TouchScreenResetsToAlert;
		LogHelper.Instance.Log("TouchScreenHealth Active {0}", _touchScreenActive);
		LogHelper.Instance.Log("TouchScreenHealth Reporting {0}", _touchScreenReporting);
		LogHelper.Instance.Log("TouchScreenHealth Healing {0}", _touchScreenHealing);
		LogHelper.Instance.Log("TouchScreenHealth StartCheck {0}", _touchScreenStartCheck);
		LogHelper.Instance.Log("TouchScreenHealth EndCheck {0}", _touchScreenEndCheck);
		LogHelper.Instance.Log("TouchScreenHealth Inactivity {0}", _touchScreenInActivity);
	}

	private TimeSpan GetTimeSpan(string value, TimeSpan defaultValue)
	{
		if (value != null && TimeSpan.TryParse(value, out var result))
		{
			LogHelper.Instance.Log($"Parsed '{value}' to {result}");
			return result;
		}
		LogHelper.Instance.Log($"failed to parse '{value}' to timespan.  returning default value {defaultValue}");
		return defaultValue;
	}
}
