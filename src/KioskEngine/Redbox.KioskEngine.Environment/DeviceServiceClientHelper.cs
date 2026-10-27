using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using DeviceService.Client;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskEngine.Environment;

public class DeviceServiceClientHelper : IDeviceServiceClientHelper
{
	private OnDeviceServiceCanShutDown _onDeviceServiceCanShutDownHandler;

	private int _pollingInterval;

	private readonly object _pollingLock = new object();

	private Timer _pollingTimer;

	private IReadCardJob _readCardJob;

	private IDeviceServiceClient _deviceServiceClient;

	private CardReaderState _cardReaderState;

	private bool _statusChecked;

	public static DeviceServiceClientHelper Instance => Singleton<DeviceServiceClientHelper>.Instance;

	public bool IsConnectedToDeviceService
	{
		get
		{
			if (_deviceServiceClient != null)
			{
				return _deviceServiceClient.IsConnectedToDeviceService;
			}
			return false;
		}
	}

	public bool IsCardReaderConnected
	{
		get
		{
			if (IsConnectedToDeviceService)
			{
				return _deviceServiceClient.IsCardReaderConnected();
			}
			return false;
		}
	}

	public bool IsCardReaderTampered { get; set; }

	public bool SupportsEMV => _cardReaderState?.SupportsEMV ?? false;

	public bool SupportsVas => _cardReaderState?.SupportsVas ?? false;

	public Version DeviceServiceVersion => _cardReaderState?.Version;

	public Version DeviceServiceClientVersion => _deviceServiceClient?.AssemblyVersion;

	public IDeviceServiceShutDownInfo DeviceServiceShutDownInfo => _deviceServiceClient?.DeviceServiceShutDownInfo;

	public bool ConnectToDeviceService()
	{
		bool result = IsConnectedToDeviceService;
		if (!result)
		{
			if (_deviceServiceClient == null)
			{
				CreateDeviceServiceClient();
			}
			if (_deviceServiceClient != null)
			{
				string url = ServiceLocator.Instance.GetService<IConfiguration>()?.Lifetime.DeviceService.DeviceServiceClientUrl;
				Task.Run(delegate
				{
					result = _deviceServiceClient.ConnectToDeviceService(url, 5000);
				}).Wait();
				LogHelper.Instance.Log($"DeviceServiceClientHelper connected: {result}");
			}
			else
			{
				LogHelper.Instance.Log("Unable to create DeviceServiceClient.");
			}
		}
		return result;
	}

	public void RegisterOnDeviceServiceCanShutDownHandler(OnDeviceServiceCanShutDown onDeviceServiceCanShutDown)
	{
		_onDeviceServiceCanShutDownHandler = onDeviceServiceCanShutDown;
	}

	public void PollForCardReaderConnectionAndValidVersion()
	{
		lock (_pollingLock)
		{
			if (_pollingTimer == null)
			{
				_pollingTimer = new Timer
				{
					AutoReset = false
				};
				_pollingTimer.Elapsed += PollingTimerElapsed;
			}
			if (!_pollingTimer.Enabled)
			{
				ResetPollingInterval();
				_pollingTimer.Interval = GetPollingInterval();
				LogHelper.Instance.Log("Starting DeviceServiceClientHelpder polling timer");
				_pollingTimer.Enabled = true;
			}
		}
	}

	private void PollingTimerElapsed(object sender, ElapsedEventArgs e)
	{
		lock (_pollingLock)
		{
			bool flag = _deviceServiceClient.IsCardReaderConnected();
			bool flag2 = false;
			ValidateVersionModel validateVersionModel = null;
			if (flag)
			{
				validateVersionModel = _deviceServiceClient.ValidateVersion();
				flag2 = validateVersionModel?.IsCompatible ?? false;
			}
			if (flag & flag2)
			{
				LogHelper.Instance.Log("DeviceServiceClientHelper polling succeeded.");
				RaiseValidDeviceServiceCardReader();
				return;
			}
			if (!flag)
			{
				LogHelper.Instance.Log("DeviceServiceClientHelper polling failed.  card reader is not connected");
			}
			else if (!flag2)
			{
				LogHelper.Instance.Log($"DeviceServiceClientHelper polling failed.  device service version {validateVersionModel?.DeviceServiceVersion} is not compatible with device service client version {_deviceServiceClient?.AssemblyVersion}");
			}
			int pollingInterval = GetPollingInterval();
			LogHelper.Instance.Log($"Delay DeviceServiceClientHelper polling attempt {TimeSpan.FromMilliseconds(pollingInterval)}");
			_pollingTimer.Interval = pollingInterval;
			_pollingTimer.Enabled = true;
			PollForCardReaderConnectionAndValidVersion();
		}
	}

	private int GetPollingInterval()
	{
		if (_pollingInterval == 0)
		{
			_pollingInterval = 5000;
		}
		else
		{
			_pollingInterval *= 2;
		}
		int num = 300000;
		if (_pollingInterval > num)
		{
			_pollingInterval = num;
		}
		return _pollingInterval;
	}

	private void ResetPollingInterval()
	{
		_pollingInterval = 0;
	}

	public IReadCardJob StartCardRead(DeviceInputType deviceInputType, Action<BaseResponseEvent> readCardResponseHandler, Action<BaseResponseEvent> readCardEventHandler, int commandTimeout, VasMode vasMode)
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IShoppingSession currentSession = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetCurrentSession();
		IEnumerable<CardBrandAndSource> deactivatedCardBrandSources = GetDeactivatedCardBrandSources();
		decimal valueOrDefault = (service?.CurrentRentalShoppingCart?.Totals.GrandTotal).GetValueOrDefault();
		valueOrDefault = Math.Round(valueOrDefault, 2);
		CardReadRequest request = new CardReadRequest
		{
			InputType = deviceInputType,
			VasMode = vasMode,
			Amount = valueOrDefault,
			Timeout = commandTimeout + 5000,
			ExcludeCardBrandBySource = deactivatedCardBrandSources,
			SessionId = currentSession.Id,
			AppleVasUrl = null,
			GoogleVasUrl = null
		};
		_readCardJob = _deviceServiceClient.CreateReadCardJob(request, readCardResponseHandler, readCardEventHandler, commandTimeout);
		LogHelper.Instance.Log($"Starting DeviceServiceClient.ReadCard with id {_readCardJob?.RequestId}");
		_readCardJob.Execute();
		return _readCardJob;
	}

	public void CancelCommand(Guid id)
	{
		LogHelper.Instance.Log($"Cancelling DeviceService command with id {id}");
		_deviceServiceClient?.CancelCommand(id, null);
	}

	public void ReportAuthorizeResult(bool authorizeSuccessful)
	{
		LogHelper.Instance.Log($"Reporting Authorize Result of {authorizeSuccessful} to DeviceService");
		_deviceServiceClient.ReportAuthorizeResult(authorizeSuccessful, delegate(BaseResponseEvent baseResponseEvent)
		{
			LogHelper.Instance.Log($"DeviceServhice has acknowledged receipt of authorize result for request {baseResponseEvent?.RequestId}");
		});
	}

	private void CreateDeviceServiceClient()
	{
		LogHelper.Instance.Log("Creating Device Service Client");
		Task.Run(delegate
		{
			if (_deviceServiceClient == null)
			{
				_deviceServiceClient = new DeviceServiceClient
				{
					AutoReconnect = true
				};
				_deviceServiceClient.OnLog += AddLogText;
				_deviceServiceClient.OnConnectedHandler += DeviceServiceClient_OnConnectedHandler;
				_deviceServiceClient.OnDisconnectedHandler += DeviceServiceClient_OnDisconnectedHandler;
				_deviceServiceClient.OnCardReaderConnectedHandler += DeviceServiceClient_OnCardReaderConnectedHandler;
				_deviceServiceClient.OnCardReaderDisconnectedHandler += DeviceServiceClient_OnCardReaderDisconnectedHandler;
				_deviceServiceClient.OnDeviceServiceCanShutDownHandler += DeviceServiceClient_OnDeviceServiceCanShutDownHandler;
				_deviceServiceClient.OnDeviceServiceShutDownInfoChangeHandler += DeviceServiceClient_OnDeviceServiceShutDownInfoChangeHandler;
				_deviceServiceClient.OnDeviceTamperedEventHandler += DeviceServiceClient_OnDeviceTamperedEventHandler;
				_deviceServiceClient.OnCardReaderStateHandler += DeviceServiceClient_OnCardReaderStateHandler;
			}
		})?.Wait();
		LogHelper.Instance.Log($"Device Service Client created: {_deviceServiceClient != null}");
	}

	private void DeviceServiceClient_OnCardReaderStateHandler(CardReaderState cardReaderState)
	{
		_cardReaderState = cardReaderState;
	}

	private void DeviceServiceClient_OnDeviceServiceShutDownInfoChangeHandler(IDeviceServiceShutDownInfo deviceServiceShutDownInfo)
	{
		LogHelper.Instance.Log($"DeviceService ShutDownInfo Changed: ShutDown Reason: {deviceServiceShutDownInfo?.ShutDownReason}  ShutDown Time: {deviceServiceShutDownInfo?.ShutDownTime}");
	}

	private void DeviceServiceClient_OnDeviceTamperedEventHandler()
	{
		LogHelper.Instance.Log("DeviceService Tampered Event received; put Kiosk into Maintenance Mode...");
		if (!IsCardReaderTampered)
		{
			IsCardReaderTampered = true;
			SendTamperedCardReaderKioskAlertMessage();
		}
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseTamperedCardReader();
	}

	private bool DeviceServiceClient_OnDeviceServiceCanShutDownHandler()
	{
		bool result = true;
		if (_onDeviceServiceCanShutDownHandler != null)
		{
			result = _onDeviceServiceCanShutDownHandler();
		}
		return result;
	}

	private void DeviceServiceClient_OnCardReaderDisconnectedHandler(Exception exception)
	{
		_cardReaderState = null;
		ResetPollingInterval();
		RaiseInvalidDeviceServiceCardReader();
	}

	private void DeviceServiceClient_OnCardReaderConnectedHandler()
	{
		CheckIfCardReaderIsConnectedAndValid(cardReaderConnectionAlreadyVerified: true);
	}

	private IEnumerable<CardBrandAndSource> GetDeactivatedCardBrandSources()
	{
		List<CardBrandAndSource> list = new List<CardBrandAndSource>();
		Dictionary<CardBrandAndSource, bool> dictionary = new Dictionary<CardBrandAndSource, bool>();
		DeviceServiceConfiguration deviceServiceConfiguration = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskSession?.DeviceService;
		dictionary.Add(CardBrandAndSource.VisaTap, deviceServiceConfiguration.ContactlessVisaEnabled);
		dictionary.Add(CardBrandAndSource.AmexTap, deviceServiceConfiguration.ContactlessAmexEnabled);
		dictionary.Add(CardBrandAndSource.MasterCardTap, deviceServiceConfiguration.ContactlessMasterCardEnabled);
		dictionary.Add(CardBrandAndSource.DiscoverTap, deviceServiceConfiguration.ContactlessDiscoverEnabled);
		dictionary.Add(CardBrandAndSource.VisaChip, deviceServiceConfiguration.VisaInsertChipEnabled);
		dictionary.Add(CardBrandAndSource.AmexChip, deviceServiceConfiguration.AmexInsertChipEnabled);
		dictionary.Add(CardBrandAndSource.MasterCardChip, deviceServiceConfiguration.MasterCardInsertChipEnabled);
		dictionary.Add(CardBrandAndSource.DiscoverChip, deviceServiceConfiguration.DiscoverInsertChipEnabled);
		foreach (KeyValuePair<CardBrandAndSource, bool> item in dictionary)
		{
			if (!item.Value)
			{
				list.Add(item.Key);
			}
		}
		return list;
	}

	private void CheckIfCardReaderIsConnectedAndValid(bool cardReaderConnectionAlreadyVerified)
	{
		bool flag = false;
		if (!cardReaderConnectionAlreadyVerified)
		{
			IDeviceServiceClient deviceServiceClient = _deviceServiceClient;
			if (deviceServiceClient == null || !deviceServiceClient.IsCardReaderConnected())
			{
				goto IL_003b;
			}
		}
		ValidateVersionModel obj = _deviceServiceClient?.ValidateVersion();
		if (obj != null && obj.IsCompatible)
		{
			flag = true;
		}
		goto IL_003b;
		IL_003b:
		if (flag)
		{
			RaiseValidDeviceServiceCardReader();
		}
		else
		{
			PollForCardReaderConnectionAndValidVersion();
		}
	}

	private void DeviceServiceClient_OnDisconnectedHandler(Exception exception)
	{
		_cardReaderState = null;
		ResetPollingInterval();
		RaiseInvalidDeviceServiceCardReader();
	}

	private void DeviceServiceClient_OnConnectedHandler()
	{
		AddLogText("DeviceService is connected.");
		CheckIfCardReaderIsConnectedAndValid(cardReaderConnectionAlreadyVerified: false);
	}

	private void RaiseInvalidDeviceServiceCardReader()
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>().RaiseInvalidDeviceServiceCardReader();
	}

	private void RaiseValidDeviceServiceCardReader()
	{
		CheckDeviceActivation();
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseValidDeviceServiceCardReader();
	}

	public void CheckDeviceActivation()
	{
		DeviceServiceConfiguration deviceServiceConfiguration = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskSession?.DeviceService;
		if (deviceServiceConfiguration.BluefinAutoActivate)
		{
			if (IsCardReaderConnected)
			{
				LogHelper.Instance.Log("CheckDeviceActivation: Sending CheckActivation command to Device Service Client.");
				IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
				if (service != null)
				{
					BluefinActivationRequest request = new BluefinActivationRequest
					{
						KioskId = service.KioskId,
						BluefinServiceUrl = deviceServiceConfiguration.BluefinServiceUrl,
						ApiKey = deviceServiceConfiguration.BluefinServiceApiKey,
						Timeout = deviceServiceConfiguration.BluefinServiceTimeout
					};
					_deviceServiceClient.CheckActivation(request);
				}
				else
				{
					LogHelper.Instance.Log("CheckDeviceActivation: StoreManager not initialized.");
				}
			}
			else
			{
				LogHelper.Instance.Log("CheckDeviceActivation: card reader not connected.");
			}
		}
		else
		{
			LogHelper.Instance.Log("CheckDeviceActivation: BluefinAutoActivate is disabled.");
		}
	}

	public void CheckDeviceStatus()
	{
		if (_statusChecked)
		{
			LogHelper.Instance.Log("CheckDeviceStatus: Status already checked. Skipping.");
			return;
		}
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		if (IsConnectedToDeviceService)
		{
			LogHelper.Instance.Log("CheckDeviceStatus: Sending CheckDeviceStatus command to Device Service Client.");
			IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
			if (service2 != null)
			{
				DeviceStatusRequest request = new DeviceStatusRequest
				{
					KioskId = service2.KioskId,
					ApiUrl = service.KioskDataServicesUrl,
					ApiKey = service.KioskDataServicesKey
				};
				_deviceServiceClient.CheckDeviceStatus(request);
				_statusChecked = true;
			}
			else
			{
				LogHelper.Instance.Log("CheckDeviceStatus: StoreManager not initialized.");
			}
		}
		else
		{
			LogHelper.Instance.Log("CheckDeviceStatus: Device Service not connected.");
		}
	}

	public void AddLogText(string logline)
	{
		LogHelper.Instance.Log(logline, LogEntryType.Info);
	}

	public ValidateVersionModel ValidateVersion()
	{
		if (!IsConnectedToDeviceService)
		{
			return null;
		}
		return _deviceServiceClient?.ValidateVersion();
	}

	public void SendTamperedCardReaderKioskAlertMessage()
	{
		try
		{
			long num = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0;
			ServiceLocator.Instance.GetService<IHealthServices>().SendAlert(num.ToString(), "ApplicationCrash", "mm:TamperedCardReaderError, 0099CDRR", string.Format("<h2>Kiosk Engine Card Reader Tampered Error</h2><p>Card Reader Tampering detected on Kiosk: {0}!<br><br>Placing Kiosk into Maintenance Mode.<br>{1}</p>", num, "0099CDRR"), DateTime.Now, null, null, skipQueue: true);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceClientHelper.SendTamperdCardReaderKioskAlertMessage!", e);
		}
	}

	public void GetCardInsertedStatus(Action<GetCardInsertedStatusResponseEvent> completeCallback)
	{
		_deviceServiceClient?.GetCardInsertedStatus(delegate(BaseResponseEvent baseResponseEventCompleteCallback)
		{
			GetCardInsertedStatusResponseEvent obj = baseResponseEventCompleteCallback as GetCardInsertedStatusResponseEvent;
			completeCallback?.Invoke(obj);
		});
	}

	public bool ShutDownDeviceService(bool forceShutdown, ShutDownReason shutDownReason)
	{
		return _deviceServiceClient?.ShutDownDeviceService(forceShutdown, shutDownReason) ?? false;
	}

	private DeviceServiceClientHelper()
	{
	}
}
