using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskHealth;

public class RouterHealth : IRouterHealth, IKioskHealthItem
{
	private IKioskEventService _kioskEvents;

	private IResourceBundleService _resourceBundleService;

	private bool _routerActive;

	private bool _routerReporting;

	private bool _routerHealing;

	private int _routerResetRetryMinutes;

	private int _routerResetAfterXMessageFailures;

	private int _routerResetMaxAttempts;

	private DateTime? _lastActive;

	private DateTime? _lastNoActivity;

	private DateTime? _firstNoActivity;

	private DateTime? _lastReset;

	private int _noActivityCount;

	private int _resetAttempts;

	private Guid? _relatedId;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 5000;

	private bool IsRentalRunning
	{
		get
		{
			bool result = false;
			if (_resourceBundleService != null)
			{
				result = _resourceBundleService.IsRentalRunning;
			}
			return result;
		}
	}

	private bool CanReset
	{
		get
		{
			if (!IsRentalRunning)
			{
				return false;
			}
			if (OrchestrationService.IsRentalSessionActive())
			{
				return false;
			}
			if (_noActivityCount < _routerResetAfterXMessageFailures)
			{
				return false;
			}
			if (_resetAttempts >= _routerResetMaxAttempts)
			{
				return false;
			}
			if (_lastReset.HasValue && (DateTime.Now - _lastReset.Value).TotalMinutes < (double)_routerResetRetryMinutes)
			{
				return false;
			}
			return true;
		}
	}

	public RouterHealth(IKioskEventService kioskEvents)
	{
		_kioskEvents = kioskEvents;
		_resourceBundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
	}

	public void Start()
	{
		try
		{
			LogHelper.Instance.Log("RouterHealth.Start");
			ReadConfiguration();
			ServiceLocator.Instance.GetService<IKernelExtensionService>().Extensions.FirstOrDefault((IKernelExtension x) => x.Name == "Redbox.KioskServices.KernelExtension.KioskServicesHost")?.Host?.RegisterCommunicationActivity(OnCommunicationUpdate);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("RouterHealth.Start - exception occurred", e);
		}
	}

	public void Stop()
	{
		LogHelper.Instance.Log("RouterHealth.Stop");
		_routerActive = false;
		_routerReporting = false;
	}

	public void Restart()
	{
		try
		{
			LogHelper.Instance.Log("RouterHealth.Restart");
			ReadConfiguration();
			Reset();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("RouterHealth.Restart - exception occurred", e);
		}
	}

	private void OnCommunicationUpdate(object sender, EventArgs args)
	{
		if ((bool)sender)
		{
			PostActivity();
		}
		else
		{
			NoActivity();
		}
	}

	public void PostActivity()
	{
		if (!_routerActive)
		{
			return;
		}
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("RouterHealth.PostActivity", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LogHelper.Instance.Log("RouterHealth.PostActivity");
			if (_resetAttempts > 0)
			{
				if (_routerReporting)
				{
					_kioskEvents.AddEvent(KioskEvent.NewEvent(3, 302, success: true, relatedId: _relatedId, elapsed: (int)(DateTime.Now - _firstNoActivity.Value).TotalMilliseconds, data: new
					{
						NoActivityCount = _noActivityCount
					}.ToJson()));
				}
				Reset();
				_lastActive = DateTime.Now;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("RouterHealth.PostActivity - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void NoActivity()
	{
		if (!_routerActive)
		{
			return;
		}
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("RouterHealth.NoActivity", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			_noActivityCount++;
			_lastNoActivity = DateTime.Now;
			if (!_firstNoActivity.HasValue)
			{
				_firstNoActivity = DateTime.Now;
				_relatedId = Guid.NewGuid();
			}
			if (!CanReset)
			{
				return;
			}
			LogHelper.Instance.Log("RouterHealth.NoActivity - Resetting - OfflineFor: {0} minutes, NoActivitycount: {1}, ResetAttempts {2}", (DateTime.Now - _firstNoActivity.Value).TotalMinutes, _noActivityCount, _resetAttempts);
			if (_routerReporting)
			{
				int? elapsed = (int)(DateTime.Now - _firstNoActivity.Value).TotalMilliseconds;
				string data = new
				{
					NoActivityCount = _noActivityCount
				}.ToJson();
				KioskEvent kioskEvent = KioskEvent.NewEvent(3, 300, success: true, elapsed, null, data);
				if (!_relatedId.HasValue)
				{
					_relatedId = kioskEvent.Id;
					kioskEvent.RelatedId = _relatedId;
				}
				kioskEvent.RelatedId = _relatedId;
				_kioskEvents.AddEvent(kioskEvent);
			}
			CorrectProblem();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("RouterHealth.NoActivity - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void CorrectProblem()
	{
		_resetAttempts++;
		_lastReset = DateTime.Now;
		if (!_routerHealing)
		{
			return;
		}
		LogHelper.Instance.Log("RouterHealth.CorrectProblem - Asking HAL to reset the router");
		bool flag = true;
		string text = string.Empty;
		int? elapsed = null;
		bool flag2 = true;
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			text = $"RouterHealth.CorrectProblem - IHardwareService is not available.";
			LogHelper.Instance.Log(text);
			flag = false;
		}
		else
		{
			flag2 = SupportsRouterReset(service);
			if (flag2)
			{
				string jobId = null;
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				ErrorList errorList = service.PowerCycleRouter(delegate
				{
				}, delegate(string jobIdentity, string status)
				{
					LogHelper.Instance.Log($"Message: {status}", LogEntryType.Info);
				}, out jobId);
				stopwatch.Stop();
				elapsed = (int)stopwatch.ElapsedMilliseconds;
				if (errorList != null && errorList.Any())
				{
					flag = false;
					Redbox.KioskEngine.ComponentModel.Error error = errorList[0];
					text = $"Code: {error.Code} Description: {error.Description} Details: {error.Details}";
				}
			}
		}
		if (_routerReporting)
		{
			_kioskEvents.AddEvent(KioskEvent.NewEvent(3, 301, flag, elapsed, _relatedId, flag ? new
			{
				SupportsRouterReset = flag2
			}.ToJson() : new
			{
				Error = text
			}.ToJson()));
		}
	}

	private bool SupportsRouterReset(IHardwareService hal)
	{
		_ = string.Empty;
		ErrorList errorList = hal.SupportsRouterReset(out var isSupported);
		if (errorList != null && errorList.Any())
		{
			errorList.ToLogHelper();
			return false;
		}
		return isSupported;
	}

	private void ReadConfiguration()
	{
		KioskHealthConfiguration kioskHealth = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth;
		_routerActive = kioskHealth.RouterActive;
		_routerReporting = kioskHealth.RouterReporting;
		_routerHealing = kioskHealth.RouterHealing;
		_routerResetRetryMinutes = kioskHealth.RouterResetRetryMinutes;
		_routerResetAfterXMessageFailures = kioskHealth.RouterResetAfterXMessageFailures;
		_routerResetMaxAttempts = kioskHealth.RouterResetMaxAttempts;
		LogHelper.Instance.Log("RouterHealth Active {0}", _routerActive);
		LogHelper.Instance.Log("RouterHealth Reporting {0}", _routerReporting);
		LogHelper.Instance.Log("RouterHealth Healing {0}", _routerHealing);
		LogHelper.Instance.Log("RouterHealth ResetRetryMinutes {0}", _routerResetRetryMinutes);
		LogHelper.Instance.Log("RouterHealth ResetAfterXMessageFailures {0}", _routerResetAfterXMessageFailures);
		LogHelper.Instance.Log("RouterHealth ResetMaxAttempts {0}", _routerResetMaxAttempts);
	}

	private void Reset()
	{
		_lastActive = null;
		_lastNoActivity = null;
		_firstNoActivity = null;
		_resetAttempts = 0;
		_relatedId = null;
		_noActivityCount = 0;
		_lastReset = null;
	}
}
