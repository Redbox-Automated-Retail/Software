using System;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskHealth;

public class CCReaderHealth : IKioskHealthItem, ICCReaderHealth
{
	private IKioskEventService _kioskEvents;

	private bool _CCReaderActive;

	private bool _CCReaderSendNoDataEvents;

	private bool _CCReaderSendBadDataEvents;

	private int _CCReaderNoDataAttemptsToAlert;

	private int _CCReaderBadDataAttemptsToAlert;

	private bool _inactivityOccurred;

	private Guid? _relatedId;

	private int _failedAttemptCount;

	private int _readErrorCount;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 5000;

	public CCReaderHealth(IKioskEventService kioskEvents)
	{
		_kioskEvents = kioskEvents;
	}

	public void Start()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("CCReaderHealth.Start", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("CCReaderHealth.Start - exception occurred", e);
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
			LogHelper.Instance.LogError("CCReaderHealth.Stop", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStop();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("CCReaderHealth.Stop - exception occurred", e);
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
			if (!_CCReaderActive)
			{
				LogHelper.Instance.Log("CCReaderHealth is not active");
				return;
			}
			LogHelper.Instance.Log("CCReaderHealth.InternalStart");
			Reset();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("CCReaderHealth.InternalStart - exception occurred", e);
		}
	}

	private void InternalStop()
	{
		try
		{
			LogHelper.Instance.Log("CCReaderHealth.InternalStop");
			_CCReaderActive = false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("CCReaderHealth.InternalStop - exception occurred", e);
		}
	}

	public void Restart()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("CCReaderHealth.Restart", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LogHelper.Instance.Log("CCReaderHealth.ReStart");
			InternalStop();
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("CCReaderHealth.Restart - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void EventOccurred(bool dataWasReceived, bool hasError)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("CCReaderHealth.EventOccurred", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (!_CCReaderActive)
			{
				return;
			}
			LogHelper.Instance.Log($"CCReaderHealth.EventOccurred dataWasReceived: {dataWasReceived}, hasError: {hasError}.");
			if (dataWasReceived)
			{
				if (hasError && _CCReaderSendBadDataEvents)
				{
					_readErrorCount++;
					_inactivityOccurred = true;
					string data = new
					{
						ReadErrors = _readErrorCount
					}.ToJson();
					KioskEvent kioskEvent = KioskEvent.NewEvent(6, 601, success: false, null, null, data);
					if (!_relatedId.HasValue)
					{
						_relatedId = kioskEvent.Id;
						kioskEvent.RelatedId = _relatedId;
					}
					kioskEvent.RelatedId = _relatedId;
					_kioskEvents.AddEvent(kioskEvent);
					if (_readErrorCount % _CCReaderBadDataAttemptsToAlert == 0)
					{
						SendAlert(kioskEvent, "CCReaderReadErrors");
					}
				}
				else
				{
					if (_inactivityOccurred && _CCReaderSendNoDataEvents)
					{
						IKioskEventService kioskEvents = _kioskEvents;
						Guid? relatedId = _relatedId;
						kioskEvents.AddEvent(KioskEvent.NewEvent(6, 602, success: true, null, relatedId));
					}
					Reset();
				}
				return;
			}
			_failedAttemptCount++;
			_inactivityOccurred = true;
			if (_CCReaderSendNoDataEvents)
			{
				string data = new
				{
					FailedReadAttempts = _failedAttemptCount
				}.ToJson();
				KioskEvent kioskEvent2 = KioskEvent.NewEvent(6, 600, success: true, null, null, data);
				if (!_relatedId.HasValue)
				{
					_relatedId = kioskEvent2.Id;
					kioskEvent2.RelatedId = _relatedId;
				}
				kioskEvent2.RelatedId = _relatedId;
				_kioskEvents.AddEvent(kioskEvent2);
				if (_failedAttemptCount % _CCReaderNoDataAttemptsToAlert == 0)
				{
					SendAlert(kioskEvent2, "CCReaderInactivity");
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("CCReaderHealth.EventOccurred - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void SendAlert(KioskEvent kioskEvent, string subType, Action action = null)
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service != null)
		{
			IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
			if (service2 != null)
			{
				service.SendAlert(service2.KioskId.ToString(), "ApplicationCrash", subType, kioskEvent.Data, DateTime.Now, null, action);
			}
		}
	}

	private void Reset()
	{
		_inactivityOccurred = false;
		_relatedId = null;
		_failedAttemptCount = 0;
		_readErrorCount = 0;
	}

	private void ReadConfiguration()
	{
		KioskHealthConfiguration kioskHealth = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth;
		_CCReaderActive = kioskHealth.CCReaderActive;
		_CCReaderSendNoDataEvents = kioskHealth.CCReaderSendNoDataEvents;
		_CCReaderSendBadDataEvents = kioskHealth.CCReaderSendBadDataEvents;
		_CCReaderNoDataAttemptsToAlert = kioskHealth.CCReaderNoDataAttemptsToAlert;
		_CCReaderBadDataAttemptsToAlert = kioskHealth.CCReaderBadDataAttemptsToAlert;
		LogHelper.Instance.Log("CCReaderHealth Active {0}", _CCReaderActive);
		LogHelper.Instance.Log("CCReaderHealth Send No Data Events {0}", _CCReaderSendNoDataEvents);
		LogHelper.Instance.Log("CCReaderHealth Failed Attempts {0}", _CCReaderNoDataAttemptsToAlert);
		LogHelper.Instance.Log("CCReaderHealth Send Read Errors {0}", _CCReaderSendBadDataEvents);
		LogHelper.Instance.Log("CCReaderHealth Error Attempts {0}", _CCReaderBadDataAttemptsToAlert);
	}
}
