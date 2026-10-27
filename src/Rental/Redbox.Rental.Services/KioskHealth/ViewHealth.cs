using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskHealth;

public class ViewHealth : IViewHealth, IKioskHealthItem
{
	private IKioskEventService _kioskEvents;

	private IResourceBundleService _resourceBundleService;

	private IViewService _viewService;

	private bool _viewActive;

	private bool _viewReporting;

	private int _viewHealingType;

	public int _viewInActivity;

	public int _viewInActivtiyABEMode;

	private string _viewName;

	private string _viewEventType;

	private string _viewEventValue;

	private string _sessionId;

	private System.Threading.Timer _noActivityTimer;

	private bool _resetOccurred;

	private Guid? _relatedId;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 5000;

	private int InActivitySeconds
	{
		get
		{
			if (!IsInABEMode)
			{
				return _viewInActivity;
			}
			return _viewInActivtiyABEMode;
		}
	}

	private bool IsOnStartScreen
	{
		get
		{
			bool flag = false;
			if (_viewService != null)
			{
				flag = _viewService.Peek() == _viewService.GetStartOverViewName();
			}
			return IsRentalRunning & flag;
		}
	}

	private bool IsInMaintenanceMode => ServiceLocator.Instance.GetService<IApplicationState>()?.IsInMaintenanceMode ?? false;

	private bool IsOnMaintenanceSync => _viewService?.Peek() == "maintenance_sync_view";

	private string CurrentViewName
	{
		get
		{
			if (_viewService != null)
			{
				return _viewService.Peek();
			}
			return string.Empty;
		}
	}

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

	private bool IsInABEMode => ServiceLocator.Instance.GetService<IApplicationState>()?.IsABEOn ?? false;

	public ViewHealth(IKioskEventService kioskEvents)
	{
		_kioskEvents = kioskEvents;
		if (_noActivityTimer == null)
		{
			_noActivityTimer = new System.Threading.Timer(delegate
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
			LogHelper.Instance.LogError("ViewHealth.Start", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ViewHealth.Start - exception occurred", e);
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
			LogHelper.Instance.LogError("ViewHealth.Stop", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStop();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ViewHealth.Stop - exception occurred", e);
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
			if (!_viewActive)
			{
				LogHelper.Instance.Log("ViewHealth is not active");
				return;
			}
			LogHelper.Instance.Log("ViewHealth.InternalStart");
			Reset();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ViewHealth.InternalStart - exception occurred", e);
		}
	}

	private void InternalStop()
	{
		try
		{
			LogHelper.Instance.Log("ViewHealth.InternalStop");
			StopTimer();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ViewHealth.InternalStop - exception occurred", e);
		}
	}

	public void Restart()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("ViewHealth.Restart", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LogHelper.Instance.Log("ViewHealth.ReStart");
			InternalStop();
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ViewHealth.Restart - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void PostActivity(string viewEventType = "", string viewEventValue = "", string sessionId = "")
	{
		if (!_viewActive)
		{
			StopTimer();
			return;
		}
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("ViewHealth.PostActivity", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			_viewName = CurrentViewName;
			_viewEventType = viewEventType;
			_viewEventValue = viewEventValue;
			_sessionId = sessionId;
			LogHelper.Instance.Log($"ViewHealth.PostActivity ViewName: {_viewName} ViewHealingType: {_viewHealingType} ViewEventType: {_viewEventType} ViewEventValue: {_viewEventValue} SessionId: {_sessionId}");
			StopTimer();
			StartTimer();
			if (_resetOccurred)
			{
				if (_viewReporting)
				{
					IKioskEventService kioskEvents = _kioskEvents;
					Guid? relatedId = _relatedId;
					kioskEvents.AddEvent(KioskEvent.NewEvent(5, 502, success: true, null, relatedId));
				}
				Reset();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ViewHealth.PostActivity - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private bool CanStartTimer()
	{
		if (!_viewActive)
		{
			return false;
		}
		if (!IsRentalRunning)
		{
			return false;
		}
		if (IsOnStartScreen)
		{
			return false;
		}
		if (IsInMaintenanceMode)
		{
			return false;
		}
		if (IsOnMaintenanceSync)
		{
			return false;
		}
		return true;
	}

	private void StartTimer()
	{
		if (CanStartTimer())
		{
			LogHelper.Instance.Log("ViewHealth.StartTimer - started");
			_noActivityTimer.Change(InActivitySeconds * 1000, -1);
		}
		else
		{
			LogHelper.Instance.Log("ViewHealth.StartTimer - not started");
		}
	}

	private void StopTimer()
	{
		_noActivityTimer.Change(-1, -1);
	}

	private void NoActivity()
	{
		if (!_viewActive)
		{
			StopTimer();
			return;
		}
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("ViewHealth.NoActivity", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LogHelper.Instance.Log($"ViewHealth.NoActivity - Kiosk view {_viewName} has been inactive for {InActivitySeconds} seconds ");
			KioskEvent kioskEvent = null;
			if (_viewReporting)
			{
				string data = new
				{
					InactivitySeconds = InActivitySeconds,
					ViewName = _viewName,
					ViewEventType = _viewEventType,
					ViewEventValue = _viewEventValue,
					SessionId = _sessionId
				}.ToJson();
				kioskEvent = KioskEvent.NewEvent(5, 500, success: true, null, null, data);
				if (!_relatedId.HasValue)
				{
					_relatedId = kioskEvent.Id;
					kioskEvent.RelatedId = _relatedId;
				}
				kioskEvent.RelatedId = _relatedId;
				_kioskEvents.AddEvent(kioskEvent);
			}
			ServiceLocator.Instance.GetService<ICallbackService>()?.LogCallbackTracking();
			ServiceLocator.Instance.GetService<IKernelService>()?.LogLuaFunctions();
			Action action = null;
			if (_viewHealingType > 0)
			{
				_resetOccurred = true;
				LogHelper.Instance.Log($"ViewHealth.CorrectProblem - Attemping to heal stuck view with type {_viewHealingType}");
				bool success = true;
				_ = string.Empty;
				if (_viewReporting)
				{
					KioskEvent kioskEvent2 = KioskEvent.NewEvent(5, 501, success, null, _relatedId);
					_kioskEvents.AddEvent(kioskEvent2);
				}
				if (_viewHealingType == 1)
				{
					IEngineApplication eng = ServiceLocator.Instance.GetService<IEngineApplication>();
					Task.Run(delegate
					{
						eng.ThreadSafeHostUpdate(delegate
						{
							ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("ViewHealth::CorrectProblem - stuck on single screen too long.");
						});
					});
				}
				else if (_viewHealingType == 2)
				{
					action = Shutdown;
				}
			}
			if (kioskEvent != null)
			{
				SendAlert(kioskEvent, action);
			}
			else
			{
				action?.Invoke();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ViewHealth.NoActivity - exception occurred", e);
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
				service.SendAlert(service2.KioskId.ToString(), "ApplicationCrash", "ViewTimeout", kioskEvent.Data, DateTime.Now, null, action, skipQueue: true);
			}
		}
	}

	private void Shutdown()
	{
		LogHelper.Instance.Log("ViewHealth.Shutdown - ShutDownEngine");
		_kioskEvents.Stop();
		Task.Run(delegate
		{
			AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
			autoResetEvent.WaitOne(14000);
			LogHelper.Instance.Log("ViewHealth.Shutdown - ShutDownEngine Environment.Exit -FORCED");
			autoResetEvent.WaitOne(1000);
			Environment.Exit(0);
		});
		Application.Exit();
	}

	private void Reset()
	{
		_resetOccurred = false;
		_relatedId = null;
	}

	private void ReadConfiguration()
	{
		KioskHealthConfiguration kioskHealth = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth;
		_viewActive = kioskHealth.ViewActive;
		_viewReporting = kioskHealth.ViewReporting;
		_viewHealingType = kioskHealth.ViewHealingType;
		_viewInActivity = kioskHealth.ViewInActivity;
		_viewInActivtiyABEMode = kioskHealth.ViewInActivityABEMode;
		LogHelper.Instance.Log("ViewHealth Active {0}", _viewActive);
		LogHelper.Instance.Log("ViewHealth Reporting {0}", _viewReporting);
		LogHelper.Instance.Log("ViewHealth Healing Type {0}", _viewHealingType);
		LogHelper.Instance.Log("ViewHealth Inactivity {0}", _viewInActivity);
		LogHelper.Instance.Log("ViewHealth InactivityABEMode {0}", _viewInActivtiyABEMode);
	}
}
