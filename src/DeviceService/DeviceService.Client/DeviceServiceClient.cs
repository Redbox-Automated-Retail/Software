using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DeviceService.Client.Jobs;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;

namespace DeviceService.Client;

public class DeviceServiceClient : IDeviceServiceClient
{
	private class DeviceServiceShutDownInformation : IDeviceServiceShutDownInfo
	{
		public DateTime ShutDownTime { get; set; }

		public ShutDownReason ShutDownReason { get; set; }
	}

	private class CommandData
	{
		public Action<BaseResponseEvent> CommandCompleteCallback;

		public Action<BaseResponseEvent> EventCallback;

		public BaseCommandRequest CommandRequest { get; set; }

		public AutoResetEvent AutoResetEvent { get; set; }

		public BaseResponseEvent ResponseEvent { get; set; }

		public bool IsAsync => AutoResetEvent == null;

		public bool IsTimedOut
		{
			get
			{
				double value = CommandRequest.CommandTimeout ?? 180000;
				return CommandRequest.CreatedDate.AddMilliseconds(value) < DateTime.Now;
			}
		}
	}

	private IDeviceServiceShutDownInfo _deviceServiceShutDownInfo;

	private Task _connectionTask;

	private int _reconnectionAttemptInterval;

	private System.Timers.Timer _asyncCommandTimeoutTimer;

	private readonly object _lockConnectionObject = new object();

	private readonly object _lockSendCommand = new object();

	private AutoResetEvent _connectionAutoResetEvent;

	private int _defaultCommandTimeout = 180000;

	private string _url;

	private int _connectionTimeout;

	private readonly ConcurrentDictionary<Guid, CommandData> _activeCommands = new ConcurrentDictionary<Guid, CommandData>();

	private SignalRClient _signalRClient;

	private DateTime? _lastCardReaderDisconnect;

	private bool _autoReconnect;

	public IDeviceServiceShutDownInfo DeviceServiceShutDownInfo
	{
		get
		{
			return _deviceServiceShutDownInfo;
		}
		private set
		{
			_deviceServiceShutDownInfo = value;
			OnDeviceServiceShutDownInfoChangeHandler?.Invoke(_deviceServiceShutDownInfo);
		}
	}

	public bool IsConnectedToDeviceService
	{
		get
		{
			if (_signalRClient != null)
			{
				return _signalRClient.IsConnectedToDeviceService;
			}
			return false;
		}
	}

	public string Url => _url;

	public int DefaultCommandTimeout
	{
		get
		{
			return _defaultCommandTimeout;
		}
		set
		{
			_defaultCommandTimeout = value;
		}
	}

	public bool AutoReconnect
	{
		get
		{
			return _autoReconnect;
		}
		set
		{
			_autoReconnect = value;
		}
	}

	public Version AssemblyVersion => GetType().Assembly.GetName().Version;

	public event LogHandler OnLog;

	public event OnConnected OnConnectedHandler;

	public event OnDisconnected OnDisconnectedHandler;

	public event OnConnected OnCardReaderConnectedHandler;

	public event OnDisconnected OnCardReaderDisconnectedHandler;

	public event OnDeviceServiceCanShutDown OnDeviceServiceCanShutDownHandler;

	public event OnCardReaderState OnCardReaderStateHandler;

	public event OnDeviceServiceShutDownInfoChange OnDeviceServiceShutDownInfoChangeHandler;

	public event Action OnDeviceTamperedEventHandler;

	public DeviceServiceClient()
	{
		InitializeSignalRClient();
	}

	private void InitializeSignalRClient()
	{
		_signalRClient = new SignalRClient();
		_signalRClient.OnLog += delegate(string logText)
		{
			Log(logText);
		};
		_signalRClient.OnConnectedHandler += SignalRConnected;
		_signalRClient.OnDisconnectedHandler += _signalRClient_OnDisconnectedHandler;
		_signalRClient.OnCardReaderConnectedHandler += _signalRClient_OnCardReaderConnectedHandler;
		_signalRClient.OnCardReaderDisconnectedHandler += _signalRClient_OnCardReaderDisconnectedHandler;
		_signalRClient.OnIsConnectedResponseEventHandler += _signalRClient_OnIsConnectedResponseEventHandler;
		_signalRClient.OnGetUnitHealthResponseEventHandler += _signalRClient_OnGetUnitHealthResponseEventHandler;
		_signalRClient.OnCardReadResponseEventHandler += _signalRClient_OnCardReadResponseEventHandler;
		_signalRClient.OnReadConfigurationEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnCardRemovedEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnCardReadEvent += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnWriteConfigurationEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnCancelCommandResponseEventHandler += _signalRClient_OnCancelCommandResponseEventHandler;
		_signalRClient.OnRebootCardReaderResponseEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnReportAuthorizationResultResponseEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnValidateVersionResponseEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnCheckActivationResponseEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnCheckDeviceStatusResponseEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnGetCardInsertedStatusResponseEventHandler += HandleAsyncCommandResponseEvent;
		_signalRClient.OnDeviceServiceShutDownResponseEventHandler += _signalRClient_OnDeviceServiceShutDownResponseEventHandler;
		_signalRClient.OnDeviceServiceCanShutDownEventHandler += _signalRClient_OnDeviceServiceCanShutDownEventHandler;
		_signalRClient.OnDeviceServiceShutDownStartingEventHandler += _signalRClient_OnDeviceServiceShutDownStartingEventHandler;
		_signalRClient.OnSupportsEMVResponseEventHandler += _signalRClient_OnGeneralEventHandler;
		_signalRClient.OnDeviceTamperedEventHandler += _signalRClient_OnDeviceTamperedHandler;
		_signalRClient.OnCardReaderStateEventHandler += _signalRClient_OnCardReaderStateEventHandler;
	}

	private void _signalRClient_OnCardReaderStateEventHandler(CardReaderStateEvent cardReaderStateEvent)
	{
		HandleEvent(cardReaderStateEvent, delegate
		{
			Task.Run(delegate
			{
				OnCardReaderStateHandler?.Invoke(cardReaderStateEvent?.CardReaderState);
			});
		});
	}

	private void _signalRClient_OnDeviceServiceShutDownStartingEventHandler(DeviceServiceShutDownStartingEvent deviceServiceShutDownStartingEvent)
	{
		HandleEvent(deviceServiceShutDownStartingEvent, delegate
		{
			if (deviceServiceShutDownStartingEvent != null)
			{
				Task.Run(delegate
				{
					DeviceServiceShutDownInfo = new DeviceServiceShutDownInformation
					{
						ShutDownReason = deviceServiceShutDownStartingEvent.ShutDownReason,
						ShutDownTime = DateTime.Now
					};
				});
			}
		});
	}

	private void _signalRClient_OnDeviceServiceCanShutDownEventHandler(SimpleResponseEvent baseEvent)
	{
		HandleEvent(baseEvent, delegate
		{
			Task.Run(delegate
			{
				bool canShutDown = OnDeviceServiceCanShutDownHandler?.Invoke() ?? true;
				CommandData commandData = new CommandData
				{
					CommandRequest = new DeviceServiceCanShutDownCommand(AssemblyVersion)
					{
						CanShutDown = canShutDown
					}
				};
				SendCommand(commandData);
			});
		});
	}

	private void _signalRClient_OnDeviceServiceShutDownResponseEventHandler(DeviceServiceShutDownResponseEvent deviceServiceShutDownResponseEvent)
	{
		HandleResponseEvent(deviceServiceShutDownResponseEvent);
	}

	private void _signalRClient_OnGeneralEventHandler(BaseResponseEvent baseResponseEvent)
	{
		Task.Factory.StartNew(delegate
		{
			HandleResponseEvent(baseResponseEvent, delegate(CommandData commandData)
			{
				if (baseResponseEvent?.EventName != null)
				{
					string eventName = baseResponseEvent.EventName;
					if (eventName == "CardRemovedResponseEvent" || eventName == "CardProcessingStartedResponseEvent")
					{
						commandData?.EventCallback?.Invoke(baseResponseEvent);
					}
				}
			});
		}, TaskCreationOptions.LongRunning);
	}

	private void _signalRClient_OnCancelCommandResponseEventHandler(CancelCommandResponseEvent cancelCommandResponseEvent)
	{
		HandleAsyncCommandResponseEvent(cancelCommandResponseEvent);
	}

	private void _signalRClient_OnCardReaderDisconnectedHandler(BaseEvent baseEvent)
	{
		HandleEvent(baseEvent, delegate
		{
			Task.Run(delegate
			{
				OnCardReaderDisconnectedHandler?.Invoke(null);
			});
			_lastCardReaderDisconnect = DateTime.Now;
		});
	}

	private void _signalRClient_OnCardReaderConnectedHandler(BaseEvent baseEvent)
	{
		HandleEvent(baseEvent, delegate
		{
			if (_lastCardReaderDisconnect.HasValue)
			{
				TimeSpan timeSpan = DateTime.Now.Subtract(_lastCardReaderDisconnect.Value);
				Log($"<<< Duration of card reader disconnection: {timeSpan}");
			}
			Task.Run(delegate
			{
				OnCardReaderConnectedHandler?.Invoke();
			});
		});
	}

	private void _signalRClient_OnDeviceTamperedHandler(BaseEvent baseEvent)
	{
		HandleEvent(baseEvent, delegate
		{
			Task.Run(delegate
			{
				OnDeviceTamperedEventHandler?.Invoke();
			});
		});
	}

	private void HandleEvent(BaseEvent baseEvent, Action action = null)
	{
		LogEvent(baseEvent);
		action?.Invoke();
	}

	private void _signalRClient_OnCardReadResponseEventHandler(BaseResponseEvent crre)
	{
		HandleAsyncCommandResponseEvent(crre);
	}

	public bool ConnectToDeviceService(string url)
	{
		return ConnectToDeviceService(url, _defaultCommandTimeout);
	}

	public bool ConnectToDeviceService(string url, int connectionTimeout)
	{
		bool flag = false;
		lock (_lockConnectionObject)
		{
			_connectionAutoResetEvent = new AutoResetEvent(initialState: false);
			StartConnectionToDeviceService(url, connectionTimeout);
			_connectionAutoResetEvent.WaitOne(connectionTimeout);
			return IsConnectedToDeviceService;
		}
	}

	public void StartConnectionToDeviceService(string url, int? connectionTimeout)
	{
		_url = url;
		_connectionTimeout = connectionTimeout ?? ((_connectionTimeout != 0) ? _connectionTimeout : _defaultCommandTimeout);
		if (_connectionTask == null || _connectionTask.IsCompleted)
		{
			Log($"Begin Connecting to DeviceService at url: {_url}   timeout: {_connectionTimeout}");
			_connectionTask = _signalRClient.Connect(_url, _connectionTimeout);
			_connectionTask.ContinueWith(delegate
			{
				if (!IsConnectedToDeviceService && AutoReconnect)
				{
					AttemptReconnection();
				}
			});
		}
		else
		{
			Log("DeviceService Connect attempt rejected.  Only one connection attempt allowed at a time");
		}
	}

	public bool DisconnectFromDeviceService()
	{
		Log("Begin disconnect from DeviceService.");
		if (_signalRClient != null)
		{
			return _signalRClient.Disconnect();
		}
		return false;
	}

	public bool RebootCardReader()
	{
		return (RunCommand(new BaseCommandRequest("RebootCardReader", AssemblyVersion)) as SimpleResponseEvent)?.Success ?? false;
	}

	public bool ShutDownDeviceService(bool forceShutDown, ShutDownReason shutDownReason)
	{
		return (RunCommand(new DeviceServiceShutDownCommand(AssemblyVersion)
		{
			ForceShutDown = forceShutDown,
			Reason = shutDownReason
		}) as DeviceServiceShutDownResponseEvent)?.Success ?? false;
	}

	public bool IsCardReaderConnected()
	{
		return (RunCommand(new BaseCommandRequest("IsConnected", AssemblyVersion)) as IsConnectedResponseEvent)?.IsConnected ?? false;
	}

	public void ReadCard(Guid requestId, CardReadRequest request, Action<BaseResponseEvent> completeCallback, Action<BaseResponseEvent> eventCallback, int? timeout = null)
	{
		RunAsyncReadCardCommand(requestId, request, timeout.HasValue ? timeout.Value : _defaultCommandTimeout, completeCallback, eventCallback);
	}

	public IReadCardJob CreateReadCardJob(CardReadRequest request, Action<BaseResponseEvent> readCardJobCompleteCallback, Action<BaseResponseEvent> jobEventCallback, int? timeout = null)
	{
		return new ReadCardJob(this, request, readCardJobCompleteCallback, jobEventCallback, timeout);
	}

	public void ReportAuthorizeResult(bool success, Action<BaseResponseEvent> completeCallback)
	{
		ReportAuthorizeResultCommand baseCommandRequest = new ReportAuthorizeResultCommand(AssemblyVersion)
		{
			Success = success
		};
		RunAsyncCommand(baseCommandRequest, completeCallback, null);
	}

	public UnitHealthModel GetUnitHealth()
	{
		return (RunCommand(new BaseCommandRequest("GetUnitHealth", AssemblyVersion)) as GetUnitHealthResponseEvent)?.UnitHealthModel;
	}

	public bool GetCardInsertedStatus(Action<BaseResponseEvent> completedCallback)
	{
		return RunAsyncCommand(new BaseCommandRequest("GetCardInsertedStatus", AssemblyVersion), completedCallback, null);
	}

	public string ReadConfig(string group, string index)
	{
		return (RunCommand(new ConfigCommand("ReadConfiguration", AssemblyVersion)) as SimpleResponseEvent)?.Data;
	}

	public string WriteConfig(string group, string index, string value)
	{
		return (RunCommand(new ConfigCommand("ReadConfiguration", AssemblyVersion)) as SimpleResponseEvent)?.Data;
	}

	public bool CancelCommand(Guid commandToCancelRequestId, Action<BaseResponseEvent> cancelCompleteCallback)
	{
		CancelCommandRequest baseCommandRequest = new CancelCommandRequest(AssemblyVersion)
		{
			CommandToCancelRequestId = commandToCancelRequestId,
			RequestId = Guid.NewGuid()
		};
		return RunAsyncCommand(baseCommandRequest, cancelCompleteCallback, null);
	}

	public ValidateVersionModel ValidateVersion()
	{
		ValidateVersionModel result = new ValidateVersionModel
		{
			IsCompatible = false,
			DeviceServiceVersion = new Version(0, 0)
		};
		Version assemblyVersion = AssemblyVersion;
		if (assemblyVersion != null)
		{
			ValidateVersionResponseEvent validateVersionResponseEvent = RunCommand(new ValidateVersionCommand(assemblyVersion)) as ValidateVersionResponseEvent;
			if (validateVersionResponseEvent?.ValidateVersionModel != null)
			{
				result = validateVersionResponseEvent.ValidateVersionModel;
			}
		}
		return result;
	}

	[Obsolete("Supports is deprecated, please get this value from the ONCardReaderState event.")]
	public bool SupportsEMV()
	{
		bool result = false;
		if (RunCommand(new BaseCommandRequest("SupportsEMV", AssemblyVersion)) is SupportsEMVResponseEvent supportsEMVResponseEvent)
		{
			result = supportsEMVResponseEvent.SuportsEMV;
		}
		return result;
	}

	public bool CheckActivation(BluefinActivationRequest request)
	{
		if (request.KioskId > 0 && RunCommand(new CheckActivationCommand(AssemblyVersion)
		{
			Request = request
		}) is CheckActivationResponseEvent checkActivationResponseEvent)
		{
			return checkActivationResponseEvent.Success;
		}
		return false;
	}

	public bool CheckDeviceStatus(DeviceStatusRequest request)
	{
		if (request.KioskId > 0 && RunCommand(new CheckDeviceStatusCommand(AssemblyVersion)
		{
			Request = request
		}) is CheckDeviceStatusResponseEvent checkDeviceStatusResponseEvent)
		{
			return checkDeviceStatusResponseEvent.Success;
		}
		return false;
	}

	private void Log(string logText)
	{
		OnLog?.Invoke(logText);
	}

	private void _signalRClient_OnDisconnectedHandler(Exception exception)
	{
		_lastCardReaderDisconnect = null;
		Log($"DeviceService disconnected. {exception}");
		if (_autoReconnect)
		{
			AttemptReconnection();
		}
		Task.Run(delegate
		{
			OnDisconnectedHandler?.Invoke(exception);
		});
	}

	private void AttemptReconnection()
	{
		Task.Run(async delegate
		{
			int reconnectionInterval = GetReconnectionInterval();
			Log($"Delay DeviceServiceClient reconnection attempt {TimeSpan.FromMilliseconds(reconnectionInterval)}");
			await Task.Delay(reconnectionInterval);
			StartConnectionToDeviceService(_url, null);
		});
	}

	private void ResetReconnectionInterval()
	{
		_reconnectionAttemptInterval = 0;
	}

	private int GetReconnectionInterval()
	{
		if (_reconnectionAttemptInterval == 0)
		{
			_reconnectionAttemptInterval = 5000;
		}
		else
		{
			_reconnectionAttemptInterval *= 2;
		}
		int num = 300000;
		if (_reconnectionAttemptInterval > num)
		{
			_reconnectionAttemptInterval = num;
		}
		return _reconnectionAttemptInterval;
	}

	private void _signalRClient_OnGetUnitHealthResponseEventHandler(GetUnitHealthResponseEvent getUnitHealthResponseEvent)
	{
		LogResponseEvent(getUnitHealthResponseEvent);
		if (_activeCommands.TryGetValue(getUnitHealthResponseEvent.RequestId, out var value))
		{
			value.ResponseEvent = getUnitHealthResponseEvent;
			value?.AutoResetEvent.Set();
		}
		else
		{
			Log("Response Event Handler error - unable to find active command: " + JsonConvert.SerializeObject(getUnitHealthResponseEvent));
		}
	}

	private void HandleResponseEvent(BaseResponseEvent baseResponseEvent, Action<CommandData> action = null)
	{
		if (_activeCommands.TryGetValue(baseResponseEvent.RequestId, out var value))
		{
			LogResponseEvent(baseResponseEvent);
			value.ResponseEvent = baseResponseEvent;
			if (value.AutoResetEvent != null)
			{
				value?.AutoResetEvent.Set();
			}
			action?.Invoke(value);
		}
		else
		{
			Log("Response Event Handler error - unable to find active command: " + JsonConvert.SerializeObject(baseResponseEvent));
		}
	}

	private void HandleAsyncCommandResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		HandleResponseEvent(baseResponseEvent, delegate(CommandData commandData)
		{
			_activeCommands.TryRemove(commandData.CommandRequest.RequestId, out var _);
			Task.Run(delegate
			{
				commandData.CommandCompleteCallback?.Invoke(baseResponseEvent);
			});
		});
	}

	private void _signalRClient_OnIsConnectedResponseEventHandler(IsConnectedResponseEvent isConnectedResponseEvent)
	{
		HandleResponseEvent(isConnectedResponseEvent);
	}

	private void LogCommand(BaseCommandRequest baseCommandRequest)
	{
		Log($">>> {baseCommandRequest.CommandName}: {baseCommandRequest.Scrub()}");
	}

	private void LogCommandTimeout(BaseCommandRequest baseCommandRequest)
	{
		Log($"*** Timeout for {baseCommandRequest.CommandName}: {baseCommandRequest.Scrub()}");
	}

	private void LogResponseEvent(BaseResponseEvent responseEvent)
	{
		Log(string.Concat(str3: (!(responseEvent is EMVCardReadResponseEvent responseEvent2)) ? ((!(responseEvent is EncryptedCardReadResponseEvent responseEvent3)) ? ((!(responseEvent is UnencryptedCardReadResponseEvent responseEvent4)) ? JsonConvert.SerializeObject(responseEvent) : ObfuscateUnencryptedCardReadResponseEvent(responseEvent4)) : ObfuscateEncryptedCardReadResponseEvent(responseEvent3)) : ObfuscateEMVCardReadResponseEvent(responseEvent2), str0: "<<< ", str1: responseEvent.EventName, str2: ": "));
	}

	private void LogEvent(BaseEvent baseEvent)
	{
		Log("<<< " + baseEvent?.EventName + ": " + JsonConvert.SerializeObject(baseEvent));
	}

	private string ObfuscateEMVCardReadResponseEvent(EMVCardReadResponseEvent responseEvent)
	{
		return CreateResponseEventLogText(responseEvent, delegate(string json)
		{
			EMVCardReadResponseEvent eMVCardReadResponseEvent = JsonConvert.DeserializeObject<EMVCardReadResponseEvent>(json);
			if (eMVCardReadResponseEvent != null)
			{
				EMVCardReadModel data = eMVCardReadResponseEvent.Data;
				if (data != null)
				{
					data.ObfuscateSensitiveData();
					return eMVCardReadResponseEvent;
				}
				return eMVCardReadResponseEvent;
			}
			return eMVCardReadResponseEvent;
		});
	}

	private string ObfuscateEncryptedCardReadResponseEvent(EncryptedCardReadResponseEvent responseEvent)
	{
		return CreateResponseEventLogText(responseEvent, delegate(string json)
		{
			EncryptedCardReadResponseEvent encryptedCardReadResponseEvent = JsonConvert.DeserializeObject<EncryptedCardReadResponseEvent>(json);
			if (encryptedCardReadResponseEvent != null)
			{
				EncryptedCardReadModel data = encryptedCardReadResponseEvent.Data;
				if (data != null)
				{
					data.ObfuscateSensitiveData();
					return encryptedCardReadResponseEvent;
				}
				return encryptedCardReadResponseEvent;
			}
			return encryptedCardReadResponseEvent;
		});
	}

	private string ObfuscateUnencryptedCardReadResponseEvent(UnencryptedCardReadResponseEvent responseEvent)
	{
		return CreateResponseEventLogText(responseEvent, delegate(string json)
		{
			UnencryptedCardReadResponseEvent unencryptedCardReadResponseEvent = JsonConvert.DeserializeObject<UnencryptedCardReadResponseEvent>(json);
			if (unencryptedCardReadResponseEvent != null)
			{
				UnencryptedCardReadModel data = unencryptedCardReadResponseEvent.Data;
				if (data != null)
				{
					data.ObfuscateSensitiveData();
					return unencryptedCardReadResponseEvent;
				}
				return unencryptedCardReadResponseEvent;
			}
			return unencryptedCardReadResponseEvent;
		});
	}

	private string CreateResponseEventLogText(BaseEvent responseEvent, Func<string, object> ObfuscateJsonAction)
	{
		string result = null;
		try
		{
			string arg = JsonConvert.SerializeObject(responseEvent);
			result = JsonConvert.SerializeObject(ObfuscateJsonAction(arg));
		}
		catch (Exception arg2)
		{
			Log($"Unhandled exception in DeviceServiceClient.CreateResponseEventLogText while deserializing a CardReadResponseEvent: {arg2}");
		}
		return result;
	}

	private void SignalRConnected()
	{
		ResetReconnectionInterval();
		_lastCardReaderDisconnect = null;
		DeviceServiceShutDownInfo = null;
		Log("DeviceService connected.");
		Task.Run(delegate
		{
			OnConnectedHandler?.Invoke();
		});
		if (_connectionAutoResetEvent != null)
		{
			_connectionAutoResetEvent.Set();
		}
	}

	private bool RunAsyncReadCardCommand(Guid? requestId, CardReadRequest request, int timeout, Action<BaseResponseEvent> commandCompleteCallback = null, Action<BaseResponseEvent> eventCallback = null)
	{
		return RunAsyncCommand(new ReadCardCommand(AssemblyVersion)
		{
			RequestId = (requestId ?? Guid.NewGuid()),
			Request = request,
			CommandTimeout = timeout
		}, commandCompleteCallback, eventCallback);
	}

	private BaseResponseEvent RunCommand(BaseCommandRequest baseCommandRequest)
	{
		CommandData commandData = new CommandData
		{
			CommandRequest = baseCommandRequest,
			AutoResetEvent = new AutoResetEvent(initialState: false)
		};
		SendCommand(commandData);
		int millisecondsTimeout = (baseCommandRequest.CommandTimeout.HasValue ? baseCommandRequest.CommandTimeout.Value : _defaultCommandTimeout);
		commandData.AutoResetEvent.WaitOne(millisecondsTimeout);
		if (commandData.ResponseEvent == null)
		{
			LogCommandTimeout(commandData.CommandRequest);
		}
		_activeCommands.TryRemove(commandData.CommandRequest.RequestId, out var _);
		return commandData?.ResponseEvent;
	}

	private bool RunAsyncCommand(BaseCommandRequest baseCommandRequest, Action<BaseResponseEvent> commandCompleteCallback, Action<BaseResponseEvent> eventCallback)
	{
		bool result = false;
		if (baseCommandRequest != null && (commandCompleteCallback != null || eventCallback != null))
		{
			CommandData commandData = new CommandData
			{
				CommandRequest = baseCommandRequest
			};
			if (commandCompleteCallback != null)
			{
				commandData.CommandCompleteCallback = commandCompleteCallback;
			}
			if (eventCallback != null)
			{
				commandData.EventCallback = eventCallback;
			}
			SendCommand(commandData);
			StartTimeoutTimer();
			result = true;
		}
		return result;
	}

	private void SendCommand(CommandData commandData)
	{
		lock (_lockSendCommand)
		{
			_activeCommands[commandData.CommandRequest.RequestId] = commandData;
			LogCommand(commandData.CommandRequest);
			if (commandData.CommandRequest != null)
			{
				_signalRClient.SendCommand(commandData.CommandRequest);
			}
			else
			{
				Log($"DeviceServiceClient.SendCommand unknown command type {commandData?.CommandRequest?.GetType()}");
			}
		}
	}

	private void StartTimeoutTimer()
	{
		if (_asyncCommandTimeoutTimer == null)
		{
			_asyncCommandTimeoutTimer = new System.Timers.Timer
			{
				Interval = 60000.0,
				AutoReset = true
			};
			_asyncCommandTimeoutTimer.Elapsed += AsyncCommandTimeoutTimer_Elapsed;
		}
		if (!_asyncCommandTimeoutTimer.Enabled)
		{
			Log("Starting DeviceServiceClient aysnc command timeout timer");
			_asyncCommandTimeoutTimer.Enabled = true;
		}
	}

	private void AsyncCommandTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			int num = 0;
			foreach (KeyValuePair<Guid, CommandData> activeCommand in _activeCommands)
			{
				CommandData value = activeCommand.Value;
				if (value == null || !value.IsAsync)
				{
					continue;
				}
				if (value.IsTimedOut)
				{
					if (_activeCommands.TryRemove(value.CommandRequest.RequestId, out var removedCommandData))
					{
						TimeoutResponseEvent timeoutResponseEvent = new TimeoutResponseEvent(removedCommandData.CommandRequest)
						{
							Success = false
						};
						Task.Run(delegate
						{
							removedCommandData?.CommandCompleteCallback(timeoutResponseEvent);
						});
						CancelCommand(removedCommandData.CommandRequest.RequestId, null);
						Log("DeviceServiceClient removing timed out aync command " + JsonConvert.SerializeObject(removedCommandData.CommandRequest));
					}
				}
				else
				{
					num++;
				}
			}
			if (num == 0)
			{
				Log("DeviceServiceClient no active aysnc commands.  Stopping timeout timer");
				_asyncCommandTimeoutTimer.Enabled = false;
			}
			else
			{
				Log($"DeviceServiceClient active aysnc commands: {num}");
			}
		}
		catch (Exception arg)
		{
			Log($"Unhandled exception in DeviceServiceClient.AsyncCommandTimeoutTimer_Elapsed. {arg}");
		}
	}
}
