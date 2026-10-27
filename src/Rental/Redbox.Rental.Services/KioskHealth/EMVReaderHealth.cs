using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Services.App;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskHealth;

public class EMVReaderHealth : IKioskHealthItem, IEMVReaderHealth
{
	private class EMVDisconnectData : IPersistentData
	{
		public bool InMaintentanceMode { get; set; }

		public bool ContactlessOff { get; set; }

		public string DeviceSerialNumber { get; set; }

		public List<DateTime> RebootEvents { get; private set; } = new List<DateTime>();

		[JsonConstructor]
		private EMVDisconnectData()
		{
		}

		private EMVDisconnectData(string serialNumber)
		{
			DeviceSerialNumber = serialNumber;
		}

		public static EMVDisconnectData CreateFor(string serialNumber)
		{
			return new EMVDisconnectData(serialNumber);
		}

		public bool SerialNumberSameAs(string right)
		{
			return DeviceSerialNumber.Equals(right, StringComparison.OrdinalIgnoreCase);
		}

		public void Increment()
		{
			RebootEvents.Add(DateTime.Now);
		}

		public void Trim(int hoursToHold)
		{
			RebootEvents.RemoveAll((DateTime dt) => dt < DateTime.Now.AddHours(-hoursToHold));
		}

		public void ReplaceReader(string newSerialNumber)
		{
			DeviceSerialNumber = newSerialNumber;
			InMaintentanceMode = false;
			ContactlessOff = false;
			RebootEvents.Clear();
		}

		public void UpdateSerialNumber(string serialNumber)
		{
			if (!string.IsNullOrEmpty(serialNumber) && !SerialNumberSameAs(serialNumber))
			{
				DeviceSerialNumber = serialNumber;
				InMaintentanceMode = false;
				ContactlessOff = false;
				RebootEvents.Clear();
			}
		}
	}

	private const string CardReaderDisconnectCachFile = "EMVDisconnectData";

	private readonly IKioskEventService _kioskEvents;

	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private readonly int _lockTimeout = 5000;

	private bool _EMVReaderActive;

	private bool _EMVReaderTrackMultipleFallbacks;

	private int _EMVReaderMultipleFallbacksThreshold;

	private bool _EMVReaderDisconnectAlertActive;

	private int _EMVReaderDisconnectAlertVelocity = 50;

	private int _EMVReaderDisconnectSampleSizeHours = 48;

	private int _technicalFallbackSessionCount;

	private Guid? _firstTechnicalFallbackEventId;

	public bool ContactlessOffDueToDisconnectThresholdReached
	{
		get
		{
			bool result = false;
			if (!_lock.TryEnterWriteLock(_lockTimeout))
			{
				LogHelper.Instance.LogError("EMVReaderHealth.ContactlessOffDueToDisconnectThresholdReached", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			}
			try
			{
				if (_EMVReaderActive)
				{
					IPersistentDataCacheService service = ServiceLocator.Instance.GetService<IPersistentDataCacheService>();
					if (service != null && service.Exists("EMVDisconnectData"))
					{
						PersistentDataWrapper<EMVDisconnectData> persistentDataWrapper = service.Read<EMVDisconnectData>("EMVDisconnectData");
						if (persistentDataWrapper != null && persistentDataWrapper.Data.SerialNumberSameAs(FetchReaderSerialNumber()))
						{
							result = _EMVReaderDisconnectAlertActive && persistentDataWrapper.Data.ContactlessOff;
						}
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("EMVReaderHealth.TrackCardReaderDisconnects - exception occurred", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
			return result;
		}
	}

	private string KioskId => ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId.ToString() ?? "0";

	private string TechnicalFallbackSessionEventData => new
	{
		TechnicalFallbackSessionCount = 1 + _technicalFallbackSessionCount,
		TechnicalFallbackSessionThreshold = _EMVReaderMultipleFallbacksThreshold
	}.ToJson();

	private string TechnicalFallbackSessionResetEventData => new
	{
		TechnicalFallbackSessionTrackingReset = true
	}.ToJson();

	public EMVReaderHealth(IKioskEventService kioskEvents)
	{
		_kioskEvents = kioskEvents;
	}

	public void Start()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("EMVReaderHealth.Start", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.Start - exception occurred", e);
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
			LogHelper.Instance.LogError("EMVReaderHealth.Stop", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStop();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.Stop - exception occurred", e);
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
			if (!_EMVReaderActive)
			{
				LogHelper.Instance.Log("EMVReaderHealth is not active");
				return;
			}
			LogHelper.Instance.Log("EMVReaderHealth.InternalStart");
			Reset();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.InternalStart - exception occurred", e);
		}
	}

	private void InternalStop()
	{
		try
		{
			LogHelper.Instance.Log("EMVReaderHealth.InternalStop");
			_EMVReaderActive = false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.InternalStop - exception occurred", e);
		}
	}

	public void Restart()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("EMVReaderHealth.Restart", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LogHelper.Instance.Log("EMVReaderHealth.ReStart");
			InternalStop();
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.Restart - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void SessionWasInTechnicalFallback(bool inFallback)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("EMVReaderHealth.SessionWasInTechnicalFallback", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (!_EMVReaderActive || !_EMVReaderTrackMultipleFallbacks)
			{
				return;
			}
			if (inFallback)
			{
				KioskEvent kioskEvent;
				if (_firstTechnicalFallbackEventId.HasValue)
				{
					string technicalFallbackSessionEventData = TechnicalFallbackSessionEventData;
					Guid? firstTechnicalFallbackEventId = _firstTechnicalFallbackEventId;
					kioskEvent = KioskEvent.NewEvent(7, 700, success: true, null, firstTechnicalFallbackEventId, technicalFallbackSessionEventData);
				}
				else
				{
					string technicalFallbackSessionEventData = TechnicalFallbackSessionEventData;
					kioskEvent = KioskEvent.NewEvent(7, 700, success: true, null, null, technicalFallbackSessionEventData);
					kioskEvent.RelatedId = kioskEvent.Id;
					_firstTechnicalFallbackEventId = kioskEvent.Id;
				}
				_technicalFallbackSessionCount++;
				if (_technicalFallbackSessionCount % _EMVReaderMultipleFallbacksThreshold == 0)
				{
					SendAlert(kioskEvent.Data, "MultipleTechnicalFallbackTransactions");
				}
				_kioskEvents.AddEvent(kioskEvent);
			}
			else
			{
				if (_technicalFallbackSessionCount > 0)
				{
					IKioskEventService kioskEvents = _kioskEvents;
					string technicalFallbackSessionEventData = TechnicalFallbackSessionResetEventData;
					Guid? firstTechnicalFallbackEventId = _firstTechnicalFallbackEventId;
					kioskEvents.AddEvent(KioskEvent.NewEvent(7, 701, success: true, null, firstTechnicalFallbackEventId, technicalFallbackSessionEventData));
				}
				Reset();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.SessionWasInTechnicalFallback - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void TrackCardReaderDisconnects()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("EMVReaderHealth.TrackCardReaderDisconnects", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (!_EMVReaderActive)
			{
				return;
			}
			LogHelper.Instance.Log("EMVReaderHealth.TrackCardReaderDisconnects - Start");
			string text = FetchReaderSerialNumber();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			LogHelper.Instance.Log("EMVReaderHealth.TrackCardReaderDisconnects - Found serial number");
			IPersistentDataCacheService service = ServiceLocator.Instance.GetService<IPersistentDataCacheService>();
			if (service != null)
			{
				EMVDisconnectData eMVDisconnectData;
				if (service.Exists("EMVDisconnectData"))
				{
					LogHelper.Instance.Log("EMVReaderHealth.TrackCardReaderDisconnects - cache file exists");
					eMVDisconnectData = service.Read<EMVDisconnectData>("EMVDisconnectData").Data;
				}
				else
				{
					LogHelper.Instance.Log("EMVReaderHealth.TrackCardReaderDisconnects - creating cache data");
					eMVDisconnectData = EMVDisconnectData.CreateFor(text);
				}
				eMVDisconnectData.UpdateSerialNumber(text);
				eMVDisconnectData.Trim(_EMVReaderDisconnectSampleSizeHours);
				eMVDisconnectData.Increment();
				CheckDisconnectAlertAndContactlessOffThreshold(eMVDisconnectData);
				service.Write(eMVDisconnectData, "EMVDisconnectData");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.TrackCardReaderDisconnects - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void SendAlert(string message, string subType)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(KioskId, "HardwareError", subType, message, DateTime.Now, null);
	}

	private void Reset()
	{
		_technicalFallbackSessionCount = 0;
		_firstTechnicalFallbackEventId = null;
	}

	private void ReadConfiguration()
	{
		KioskHealthConfiguration kioskHealth = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth;
		_EMVReaderActive = kioskHealth.EMVReaderActive;
		_EMVReaderTrackMultipleFallbacks = kioskHealth.EMVReaderTrackMultipleFallbacks;
		_EMVReaderMultipleFallbacksThreshold = kioskHealth.EMVReaderMultipleFallbacksThreshold;
		_EMVReaderDisconnectAlertActive = kioskHealth.EMVReaderDisconnectAlertActive;
		_EMVReaderDisconnectAlertVelocity = kioskHealth.EMVReaderDisconnectAlertVelocity;
		_EMVReaderDisconnectSampleSizeHours = kioskHealth.EMVReaderDisconnectSampleSizeHours;
		LogHelper.Instance.Log($"EMVReaderHealth Active {_EMVReaderActive}");
		LogHelper.Instance.Log($"EMVReaderHealth Track Multiple Technical Fallback Sessions Over Threshold {_EMVReaderTrackMultipleFallbacks}");
		LogHelper.Instance.Log($"EMVReaderHealth Multiple Technical Fallback Sessions Threshold {_EMVReaderMultipleFallbacksThreshold}");
		LogHelper.Instance.Log($"EMVReaderHealth Disconnect Alert Velocity {_EMVReaderDisconnectAlertVelocity}");
		LogHelper.Instance.Log($"EMVReaderHealth Disconnect Sample Size Hours {_EMVReaderDisconnectSampleSizeHours}");
	}

	private void CheckDisconnectAlertAndContactlessOffThreshold(EMVDisconnectData data)
	{
		LogHelper.Instance.Log("EMVReaderHealth.CheckDisconnectAlertAndContactlessOffThreshold - start");
		if (!data.ContactlessOff && data.RebootEvents.Count >= _EMVReaderDisconnectAlertVelocity)
		{
			LogHelper.Instance.Log("EMVReaderHealth.CheckDisconnectAlertAndContactlessOffThreshold - over contactless disconnect threshold");
			data.ContactlessOff = true;
			if (_EMVReaderDisconnectAlertActive)
			{
				SendAlert("EMV reader disconnect alert threshold reached.", "EMVReaderDisconnectAlertVelocityReached");
			}
		}
	}

	private string FetchReaderSerialNumber()
	{
		return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Store", "ReaderSerialNumber", "");
	}
}
