using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.KioskEngine.IDE;
using Redbox.USB;

namespace Redbox.KioskEngine.Environment;

public class EnvironmentManager : IEnvironmentNotificationService
{
	private CorruptDbCallback m_corruptDbCallback;

	private TamperedCardReaderCallback m_tamperedCardReaderCallback;

	private object _trackDataServiceLockObject = new object();

	private object _validDeviceServiceCardReaderLockObject = new object();

	private readonly IDictionary<string, ConfigurationCallback> m_configurationCallbacks = new Dictionary<string, ConfigurationCallback>();

	private Action _cardReaderConnectedCallback;

	private Action<ITrackDataService> _cardReaderDisconnectedCallback;

	public static EnvironmentManager Instance => Singleton<EnvironmentManager>.Instance;

	private ITrackDataService _previousTrackDataService { get; set; }

	public bool PreviousTrackDataServiceWasDeviceServiceTrackDataService => _previousTrackDataService is IDeviceServiceTrackDataService;

	public bool PreviousTrackDataServiceWasClassicTrackDataService => _previousTrackDataService is IClassicTrackDataService;

	private bool IsDeviceServiceCardReaderAttached
	{
		get
		{
			bool result = false;
			IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			if (service != null)
			{
				if (!service.IsConnectedToDeviceService)
				{
					service?.ConnectToDeviceService();
				}
				result = service?.IsCardReaderConnected ?? false;
			}
			return result;
		}
	}

	private bool IsDeviceServiceClientValid => (ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.ValidateVersion())?.IsCompatible ?? false;

	public bool IsDeviceServiceTrackDataServiceRegistered => ServiceLocator.Instance.GetService<ITrackDataService>() is IDeviceServiceTrackDataService;

	public bool IsClassicTrackDataServiceRegistered => ServiceLocator.Instance.GetService<ITrackDataService>() is IClassicTrackDataService;

	public bool IsTrackDataServiceRegistered => ServiceLocator.Instance.GetService<ITrackDataService>() != null;

	public bool HasValidCardReader { get; internal set; }

	public bool CheckCardReader
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Core", "CheckCardReader", defaultValue: true) ?? false;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "CheckCardReader", value);
		}
	}

	public int NullKioskId
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Core", "NullKioskId", 1) ?? 1;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "NullKioskId", value);
		}
	}

	public void Reset()
	{
		try
		{
			TouchScreenFactory.Instance.Initialize();
			LogHelper.Instance.Log("Reset environment manager:");
			GenericTouchScreen touchScreen = TouchScreenFactory.Instance.GetTouchScreen();
			if (touchScreen != null)
			{
				LogHelper.Instance.Log("...Touch screen state is: {0}.", touchScreen.State);
			}
			else
			{
				LogHelper.Instance.Log("...No touch screen instance available.");
			}
			LogHelper.Instance.Log("...Clear configuration callbacks.");
			m_configurationCallbacks.Clear();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.Reset", e);
		}
	}

	public ErrorList Initialize()
	{
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Initialize environment manager.");
		ServiceLocator.Instance.AddService(typeof(IEnvironmentNotificationService), Instance);
		DetectCardReaderAndRegisterTrackDataService();
		return result;
	}

	public void DetectCardReaderAndRegisterTrackDataService()
	{
		LogHelper.Instance.Log("Check for valid credit card reader.");
		if (IsDeviceServiceCardReaderAttached)
		{
			LogHelper.Instance.Log("...Valid device service credit card reader found.");
			if (IsDeviceServiceClientValid)
			{
				IDeviceServiceTrackDataService service = ServiceLocator.Instance.GetService<IDeviceServiceTrackDataService>();
				SetTrackDataService(service);
				HasValidCardReader = true;
			}
			else
			{
				LogHelper.Instance.Log("...DeviceServiceClient version is not compatible with DeviceService version.");
				PollForCardReaderConnectionAndValidVersion();
			}
		}
		else
		{
			_ = IsDeviceServiceClientValid;
		}
		if (!HasValidCardReader)
		{
			if (CardReaderDetector.IsUSBCardReaderAttached())
			{
				LogHelper.Instance.Log("...Valid credit card reader found.");
				HasValidCardReader = true;
				IClassicTrackDataService service2 = ServiceLocator.Instance.GetService<IClassicTrackDataService>();
				SetTrackDataService(service2);
			}
			else if (CheckCardReader)
			{
				LogHelper.Instance.Log("...No valid credit card reader found.");
				SetTrackDataService(null);
				HasValidCardReader = false;
			}
			else
			{
				HasValidCardReader = true;
				IClassicTrackDataService service3 = ServiceLocator.Instance.GetService<IClassicTrackDataService>();
				SetTrackDataService(service3);
			}
		}
	}

	private void SetTrackDataService(ITrackDataService trackDataService)
	{
		lock (_trackDataServiceLockObject)
		{
			ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
			if (service != null)
			{
				_previousTrackDataService = service;
				ServiceLocator.Instance.RemoveService(typeof(ITrackDataService));
			}
			if (trackDataService != null)
			{
				ServiceLocator.Instance.AddService(typeof(ITrackDataService), trackDataService);
			}
			LogHelper.Instance.Log($"Setting track data service to type {trackDataService?.GetType()}");
		}
	}

	private void PollForCardReaderConnectionAndValidVersion()
	{
		ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.PollForCardReaderConnectionAndValidVersion();
	}

	public void Register(IntPtr handle)
	{
		if (!CheckCardReader)
		{
			return;
		}
		try
		{
			UsbNotification.RegisterForUsbEvents(handle);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised by UsbNotification.RegisterForUsbEvents.", e);
		}
	}

	public void Unregister(IntPtr handle)
	{
		if (!CheckCardReader)
		{
			return;
		}
		try
		{
			UsbNotification.UnregisterForUsbEvents(handle);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised by UsbNotification.UnregisterForUsbEvents.", e);
		}
	}

	public void NotifyConfigurationCallbacks(string store, string action, string path, string key, string newValue)
	{
		ConfigurationCallback[] array = new ConfigurationCallback[m_configurationCallbacks.Count];
		m_configurationCallbacks.Values.CopyTo(array, 0);
		ConfigurationCallback[] array2 = array;
		foreach (ConfigurationCallback function in array2)
		{
			CallbackService.Instance.EnqueueCallback(new ConfigurationCallbackEntry
			{
				Store = store,
				Function = function,
				Key = key,
				Path = path,
				Action = action,
				NewValue = newValue
			});
		}
	}

	public void RegisterConfigurationCallback(string name, ConfigurationCallback callback)
	{
		m_configurationCallbacks[name] = callback;
	}

	public void UnregisterConfigurationCallback(string name)
	{
		m_configurationCallbacks.Remove(name);
	}

	public void RegisterCorruptDbCallback(CorruptDbCallback callback)
	{
		m_corruptDbCallback = callback;
	}

	public void RaiseCorruptDb()
	{
		try
		{
			m_corruptDbCallback?.Invoke();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseCorruptDb.", e);
		}
	}

	public void RegisterTamperedCardReaderCallback(TamperedCardReaderCallback callback)
	{
		m_tamperedCardReaderCallback = callback;
	}

	public void RaiseTamperedCardReader()
	{
		try
		{
			m_tamperedCardReaderCallback?.Invoke();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseTamperedCardReaderCallback.", e);
		}
	}

	public void RegisterCardReaderConnectedCallback(Action cardReaderConnectedCallback)
	{
		_cardReaderConnectedCallback = cardReaderConnectedCallback;
	}

	public void RegisterCardReaderDisconnectedCallback(Action<ITrackDataService> cardReaderDisconnectedCallback)
	{
		_cardReaderDisconnectedCallback = cardReaderDisconnectedCallback;
	}

	public EnvironmentNotificationType ProcessClassicCardReaderNotification(ref Message msg)
	{
		if (CheckCardReader)
		{
			NotificationEventType notificationEventType = UsbNotification.HandleMessage(ref msg);
			if (HasValidCardReader)
			{
				if ((IsClassicTrackDataServiceRegistered || !IsTrackDataServiceRegistered) && notificationEventType == NotificationEventType.DeviceRemoved && !CardReaderDetector.IsUSBCardReaderAttached())
				{
					return EnvironmentNotificationType.CreditCardReaderInvalid;
				}
			}
			else if ((IsClassicTrackDataServiceRegistered || !IsTrackDataServiceRegistered) && notificationEventType == NotificationEventType.DeviceArrived && CardReaderDetector.IsUSBCardReaderAttached())
			{
				return EnvironmentNotificationType.CreditCardReaderValid;
			}
		}
		return EnvironmentNotificationType.None;
	}

	public void ToggleTouchScreen()
	{
		TouchScreenFactory.Instance.GetTouchScreen()?.Toggle();
	}

	public TouchScreenState TouchScreenStatus()
	{
		return TouchScreenFactory.Instance.GetTouchScreen()?.State ?? TouchScreenState.Unavailable;
	}

	public string TouchScreenDriver()
	{
		return TouchScreenFactory.Instance.GetTouchScreen()?.Driver;
	}

	public void LaunchSecureBrowser(IDictionary<string, string> approvedUrls, NavigateUrlCallback navigateUrlCallback)
	{
		new SecureBrowser(approvedUrls, navigateUrlCallback).Show();
	}

	public void RaiseValidClassicCardReader()
	{
		try
		{
			if (!IsTrackDataServiceRegistered || IsClassicTrackDataServiceRegistered)
			{
				IClassicTrackDataService service = ServiceLocator.Instance.GetService<IClassicTrackDataService>();
				SetTrackDataService(service);
				HasValidCardReader = true;
				HandleConnectedCardReader();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseValidClassicCardReader.", e);
		}
	}

	public void RaiseValidDeviceServiceCardReader()
	{
		try
		{
			lock (_validDeviceServiceCardReaderLockObject)
			{
				if (!IsTrackDataServiceRegistered || IsDeviceServiceTrackDataServiceRegistered)
				{
					IDeviceServiceTrackDataService service = ServiceLocator.Instance.GetService<IDeviceServiceTrackDataService>();
					SetTrackDataService(service);
					HasValidCardReader = true;
					HandleConnectedCardReader();
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseValidDeviceServiceCardReader.", e);
		}
	}

	public void RaiseInvalidClassicCardReader()
	{
		try
		{
			if (IsClassicTrackDataServiceRegistered)
			{
				SetTrackDataService(null);
				HasValidCardReader = false;
				_cardReaderDisconnectedCallback?.Invoke(_previousTrackDataService);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseInvalidClassicCardReader.", e);
		}
	}

	public void RaiseInvalidDeviceServiceCardReader()
	{
		try
		{
			if (IsDeviceServiceTrackDataServiceRegistered)
			{
				SetTrackDataService(null);
				HasValidCardReader = false;
				_cardReaderDisconnectedCallback?.Invoke(_previousTrackDataService);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseInvalidDeviceServiceCardReader.", e);
		}
	}

	private void HandleConnectedCardReader()
	{
		LogHelper.Instance.Log("The card reader was connected, resuming operation.");
		_cardReaderConnectedCallback?.Invoke();
		LogHelper.Instance.LogTo("CardReaderLog", "Card Reader Re-Connected");
	}

	private EnvironmentManager()
	{
	}
}
