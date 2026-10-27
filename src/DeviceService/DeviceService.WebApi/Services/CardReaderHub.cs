using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Analytics;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using DeviceService.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.WebApi.Services;

public class CardReaderHub : Hub
{
	private class QueuedCommand
	{
		public BaseCommandRequest Command { get; set; }

		public string ConnectionId { get; set; }

		public IClientProxy Caller { get; set; }

		public bool Cancelled { get; set; }
	}

	private IIUC285Proxy _proxy;

	private ILogger<CardReaderHub> _logger;

	private IActivationService _activationService;

	private static object _syncObject = new object();

	private IDeviceStatusService _deviceStatusService;

	private IApplicationControl _applicationControl;

	private IAnalyticsService _analytics;

	private static ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationTokenSources = new ConcurrentDictionary<Guid, CancellationTokenSource>();

	private static int _configureOccurred = 0;

	private static QueuedCommand ActiveCommand = null;

	private static Task _processCommandQueue;

	private static Dictionary<string, Action<QueuedCommand>> _commandProcessors;

	private static ConcurrentQueue<QueuedCommand> _commandQueue = new ConcurrentQueue<QueuedCommand>();

	private static bool _commandQueuePaused = false;

	public static bool IsProcessingCommands
	{
		get
		{
			if (_processCommandQueue != null)
			{
				return !_processCommandQueue.IsCompleted;
			}
			return false;
		}
	}

	private bool CommandQueuePaused
	{
		get
		{
			return _commandQueuePaused;
		}
		set
		{
			if (value != _commandQueuePaused)
			{
				_commandQueuePaused = value;
				string text = (_commandQueuePaused ? "paused" : "restarted");
				_logger?.LogInformation("Command Queue " + text);
				if (!_commandQueuePaused)
				{
					StartProcessingQueuedCommands();
				}
			}
		}
	}

	public CardReaderHub(IIUC285Proxy proxy, ILogger<CardReaderHub> logger, IActivationService activationService, IDeviceStatusService deviceStatusService, IApplicationControl applicationControl, IAnalyticsService analytics)
	{
		_proxy = proxy;
		_logger = logger;
		_activationService = activationService;
		_deviceStatusService = deviceStatusService;
		_applicationControl = applicationControl;
		_analytics = analytics;
		ConfigureCommandProcessors();
	}

	public void SendMessage(string message)
	{
		base.Clients.All.SendAsync("ReceiveMessage", "username", message);
	}

	public void SendSimpleEvent(SimpleEvent simpleEvent)
	{
		if (simpleEvent != null)
		{
			switch (simpleEvent.EventName)
			{
			case "CardReaderConnectedEvent":
				base.Clients.All.SendAsync("CardReaderConnectedEvent", simpleEvent);
				break;
			case "CardReaderDisconnectedEvent":
				base.Clients.All.SendAsync("CardReaderDisconnectedEvent", simpleEvent);
				break;
			case "CardRemovedResponseEvent":
				base.Clients.All.SendAsync("CardRemovedResponseEvent", simpleEvent);
				break;
			case "DeviceServiceCanShutDownEvent":
				base.Clients.All.SendAsync("DeviceServiceCanShutDownEvent", simpleEvent);
				break;
			default:
				base.Clients.All.SendAsync(simpleEvent.EventName, simpleEvent);
				break;
			}
		}
	}

	private void AddQueuedCommand(QueuedCommand queuedCommand)
	{
		if (queuedCommand?.Command == null)
		{
			return;
		}
		queuedCommand.ConnectionId = base.Context.ConnectionId;
		lock (_syncObject)
		{
			Action<QueuedCommand> value;
			if (queuedCommand.Command.IsQueuedCommand && queuedCommand.Command.CommandName != "IsConnected" && queuedCommand.Command.CommandName != "SupportsEMV")
			{
				_commandQueue.Enqueue(queuedCommand);
			}
			else if (_commandProcessors.TryGetValue(queuedCommand.Command.CommandName, out value))
			{
				value?.Invoke(queuedCommand);
			}
		}
		_logger?.LogInformation($"Added command {queuedCommand.Command.CommandName} to Queued commands with request id: {queuedCommand.Command.RequestId} with {_commandQueue.Count} commands in queue.");
		StartProcessingQueuedCommands();
	}

	private void StartProcessingQueuedCommands()
	{
		if (_processCommandQueue == null || _processCommandQueue.IsCompleted)
		{
			_processCommandQueue = Task.Run(delegate
			{
				ProcessQueuedCommands();
			});
		}
		else
		{
			_logger?.LogInformation("ProcessQueuedCommands task already running");
		}
	}

	private void ConfigureCommandProcessors()
	{
		if (_commandProcessors == null && Interlocked.CompareExchange(ref _configureOccurred, 1, 0) == 0)
		{
			_commandProcessors = new Dictionary<string, Action<QueuedCommand>>();
			_commandProcessors["IsConnected"] = ProcessIsConnectedCommand;
			_commandProcessors["GetUnitHealth"] = ProcessGetUnitHealthCommand;
			_commandProcessors["RebootCardReader"] = ProcessRebootCardReaderCommand;
			_commandProcessors["ReadConfiguration"] = ProcessReadConfigurationCommand;
			_commandProcessors["WriteConfiguration"] = ProcessWriteConfigurationCommand;
			_commandProcessors["ReadCard"] = ProcessReadCardCommand;
			_commandProcessors["ValidateVersion"] = ProcessValidateVersionCommand;
			_commandProcessors["CheckActivation"] = ProcessCheckActivationCommand;
			_commandProcessors["CheckDeviceStatus"] = ProcessCheckDeviceStatusCommand;
			_commandProcessors["GetCardInsertedStatus"] = ProcessGetCardInsertedStatusCommand;
			_commandProcessors["SupportsEMV"] = ProcessSupportsEMVCommand;
		}
	}

	private void ProcessQueuedCommands()
	{
		ActiveCommand = null;
		int num = 0;
		lock (_syncObject)
		{
			if (!_commandQueuePaused)
			{
				num = _commandQueue.Count;
				if (num > 0)
				{
					_commandQueue.TryDequeue(out ActiveCommand);
				}
			}
		}
		while (num > 0 && ActiveCommand != null)
		{
			_logger?.LogInformation($"Begining processing for queued command {ActiveCommand?.Command?.CommandName} with ID {ActiveCommand?.Command.RequestId}");
			if (_commandProcessors.TryGetValue(ActiveCommand?.Command?.CommandName, out var value))
			{
				_proxy.StopHealthTimer();
				value(ActiveCommand);
			}
			else
			{
				_logger?.LogError("ProcessQueuedCommands - error.  No handler for command " + ActiveCommand?.Command?.CommandName);
			}
			_logger?.LogInformation($"Finished processing for queued command {ActiveCommand?.Command?.CommandName} with ID {ActiveCommand?.Command.RequestId}");
			lock (_syncObject)
			{
				if (_commandQueuePaused)
				{
					continue;
				}
				num = _commandQueue.Count;
				if (num <= 0)
				{
					continue;
				}
				List<string> list = new List<string>();
				foreach (QueuedCommand item in _commandQueue)
				{
					list.Add(item?.Command?.CommandName);
				}
				string arg = string.Join(", ", list.ToArray());
				_logger?.LogInformation($"Queued Command count: {num}   Commands: {arg}");
				_commandQueue.TryDequeue(out ActiveCommand);
			}
		}
		ActiveCommand = null;
		string text = (_commandQueuePaused ? $"paused.  Command queue count = {_commandQueue.Count}" : "empty");
		_logger?.LogInformation("Command queue is " + text + ".");
		_proxy.StartHealthTimer();
	}

	private void ProcessIsConnectedCommand(QueuedCommand queuedCommand)
	{
		bool isConnected = _proxy?.IsConnected ?? false;
		IsConnectedResponseEvent isConnectedResponseEvent = new IsConnectedResponseEvent(queuedCommand.Command)
		{
			IsConnected = isConnected,
			Success = true
		};
		LogEvent(">>> IsConnectedResponseEvent", isConnectedResponseEvent);
		queuedCommand.Caller.SendAsync(isConnectedResponseEvent.EventName, isConnectedResponseEvent);
	}

	private void ProcessGetUnitHealthCommand(QueuedCommand queuedCommand)
	{
		UnitHealthModel unitHealthModel = _proxy?.GetUnitHealth();
		GetUnitHealthResponseEvent getUnitHealthResponseEvent = new GetUnitHealthResponseEvent(queuedCommand.Command)
		{
			UnitHealthModel = unitHealthModel,
			Success = (unitHealthModel != null)
		};
		LogEvent(">>> GetUnitHealthResponseEvent", getUnitHealthResponseEvent);
		queuedCommand.Caller.SendAsync(getUnitHealthResponseEvent.EventName, getUnitHealthResponseEvent);
	}

	private void ProcessRebootCardReaderCommand(QueuedCommand queuedCommand)
	{
		bool success = _proxy.Reboot();
		RebootCardReaderResponseEvent rebootCardReaderResponseEvent = new RebootCardReaderResponseEvent(queuedCommand.Command)
		{
			Success = success
		};
		LogEvent(">>> Reboot Card Reader", rebootCardReaderResponseEvent);
		queuedCommand.Caller.SendAsync(rebootCardReaderResponseEvent.EventName, rebootCardReaderResponseEvent);
	}

	private void ProcessReadConfigurationCommand(QueuedCommand queuedCommand)
	{
		ConfigCommand configCommand = queuedCommand.Command as ConfigCommand;
		Task<string> task = Task.Run(() => (!_proxy.ReadConfig(configCommand.GroupNumber, configCommand.IndexNumber, out var Data)) ? null : Data);
		task.Wait();
		SimpleResponseEvent simpleResponseEvent = new SimpleResponseEvent(queuedCommand.Command, "ReadConfiguration")
		{
			Data = task?.Result
		};
		LogEvent(">>> ReadConfiguration", simpleResponseEvent);
		queuedCommand.Caller.SendAsync("ReadConfiguration", simpleResponseEvent);
	}

	private void ProcessWriteConfigurationCommand(QueuedCommand queuedCommand)
	{
		ConfigCommand configCommand = queuedCommand.Command as ConfigCommand;
		Task<bool> task = Task.Run(() => _proxy.WriteConfig(configCommand.GroupNumber, configCommand.IndexNumber, configCommand.Value));
		task.Wait();
		SimpleResponseEvent simpleResponseEvent = new SimpleResponseEvent(queuedCommand.Command, "WriteConfiguration")
		{
			Data = task?.Result.ToString()
		};
		LogEvent(">>> WriteConfiguration", simpleResponseEvent);
		queuedCommand.Caller.SendAsync("WriteConfiguration", simpleResponseEvent);
	}

	private void ProcessValidateVersionCommand(QueuedCommand queuedCommand)
	{
		if (queuedCommand?.Command is ValidateVersionCommand validateVersionCommand)
		{
			bool isCompatible = DeviceService.Domain.DeviceService.IsClientVersionCompatible(validateVersionCommand.DeviceServiceClientVersion);
			ValidateVersionResponseEvent validateVersionResponseEvent = new ValidateVersionResponseEvent(validateVersionCommand)
			{
				ValidateVersionModel = new ValidateVersionModel
				{
					IsCompatible = isCompatible,
					DeviceServiceVersion = DeviceService.Domain.DeviceService.AssemblyVersion
				},
				Success = true
			};
			LogEvent(">>> validateVersionResponseEvent", validateVersionResponseEvent);
			queuedCommand.Caller.SendAsync("ValidateVersionReponseEvent", validateVersionResponseEvent);
		}
	}

	private void ProcessGetCardInsertedStatusCommand(QueuedCommand queuedCommand)
	{
		if (queuedCommand != null)
		{
			InsertedStatus cardInsertedStatus = _proxy.CheckIfCardInserted();
			GetCardInsertedStatusResponseEvent getCardInsertedStatusResponseEvent = new GetCardInsertedStatusResponseEvent(queuedCommand.Command)
			{
				CardInsertedStatus = cardInsertedStatus,
				Success = true
			};
			LogEvent(">>> GetCardInsertedStatusResponseEvent", getCardInsertedStatusResponseEvent);
			queuedCommand.Caller.SendAsync("GetCardInsertedStatusResponseEvent", getCardInsertedStatusResponseEvent);
		}
	}

	private void ProcessSupportsEMVCommand(QueuedCommand queuedCommand)
	{
		if (queuedCommand != null)
		{
			bool supportsEMV = _proxy.SupportsEMV;
			SupportsEMVResponseEvent supportsEMVResponseEvent = new SupportsEMVResponseEvent(queuedCommand.Command)
			{
				Success = true,
				SuportsEMV = supportsEMV
			};
			LogEvent(">>> SupportsEMVResponseEvent", supportsEMVResponseEvent);
			queuedCommand.Caller.SendAsync("SupportsEMVResponseEvent", supportsEMVResponseEvent);
		}
	}

	private void ProcessCheckActivationCommand(QueuedCommand queuedCommand)
	{
		if (queuedCommand?.Command is CheckActivationCommand checkActivationCommand)
		{
			_logger?.LogDebug($"CheckActivation called for Kiosk ID {checkActivationCommand.Request.KioskId}");
			bool result = _activationService.CheckAndActivate(checkActivationCommand.Request).Result;
			CheckActivationResponseEvent checkActivationResponseEvent = new CheckActivationResponseEvent(checkActivationCommand)
			{
				Success = result
			};
			LogEvent(">>> checkActivationResponseEvent", checkActivationResponseEvent);
			queuedCommand.Caller.SendAsync("CheckActivationResponseEvent", checkActivationResponseEvent);
		}
	}

	private async void ProcessCheckDeviceStatusCommand(QueuedCommand queuedCommand)
	{
		if (queuedCommand.Command is CheckDeviceStatusCommand checkDeviceStatusCommand)
		{
			CheckDeviceStatusResponseEvent responseEvent = new CheckDeviceStatusResponseEvent(checkDeviceStatusCommand);
			try
			{
				StandardResponse standardResponse = ((checkDeviceStatusCommand.Request == null) ? (await _deviceStatusService.PostDeviceStatus()) : (await _deviceStatusService.PostDeviceStatus(checkDeviceStatusCommand.Request)));
				responseEvent.Success = standardResponse.Success && standardResponse.StatusCode == 200;
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "ProcessCheckDeviceStatusCommand Error: " + ex.Message);
				responseEvent.Success = false;
			}
			await queuedCommand.Caller.SendAsync(responseEvent.EventName, responseEvent);
		}
	}

	private async void ProcessReadCardCommand(QueuedCommand queuedCommand)
	{
		ReadCardCommand command = queuedCommand?.Command as ReadCardCommand;
		if (command == null)
		{
			return;
		}
		_logger?.LogInformation("<<<CardReaderHub.ExecuteBaseCommand " + JsonConvert.SerializeObject(command));
		if (queuedCommand.Cancelled)
		{
			_logger?.LogInformation($"Queued ReadCardCommand not run because it was Canceled.  RequestId: {command.RequestId}");
			return;
		}
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		_cancellationTokenSources[command.RequestId] = cancellationTokenSource;
		CancellationToken cancellationToken = cancellationTokenSource.Token;
		await _proxy.ReadCard(command.Request, cancellationToken, delegate(Base87CardReadModel cardResult)
		{
			IClientProxy caller = queuedCommand.Caller;
			if (caller != null)
			{
				BaseResponseEvent baseResponseEvent = null;
				if (cardResult is EMVCardReadModel data)
				{
					baseResponseEvent = new EMVCardReadResponseEvent(command)
					{
						Data = data
					};
					ProcessCardReadModel(baseResponseEvent, cancellationToken);
				}
				else if (cardResult is EncryptedCardReadModel data2)
				{
					baseResponseEvent = new EncryptedCardReadResponseEvent(command)
					{
						Data = data2
					};
					ProcessCardReadModel(baseResponseEvent, cancellationToken);
				}
				else
				{
					baseResponseEvent = new UnencryptedCardReadResponseEvent(command)
					{
						Data = (cardResult as UnencryptedCardReadModel)
					};
					ProcessCardReadModel(baseResponseEvent, cancellationToken);
				}
				caller.SendAsync(baseResponseEvent.EventName, baseResponseEvent);
			}
			else
			{
				_logger.LogInformation($"Unable to find active read card client proxy for {command?.RequestId}.");
			}
		}, delegate(string eventName, string eventData)
		{
			IClientProxy caller = queuedCommand.Caller;
			if (caller != null)
			{
				if (eventName != null && eventName == "CardRemovedResponseEvent")
				{
					caller.SendAsync("CardRemovedResponseEvent", new CardRemoveResponseEvent(command));
				}
				else
				{
					caller.SendAsync(eventName, new SimpleResponseEvent(command, eventName, eventData));
				}
			}
			else
			{
				_logger.LogInformation($"Unable to find remove card client proxy for {command?.RequestId}.");
			}
		});
		_cancellationTokenSources.TryRemove(command.RequestId, out var _);
	}

	public void ExecuteBaseCommand(BaseCommandRequest command)
	{
		if (command != null)
		{
			_logger?.LogInformation("<<< " + command?.CommandName + " Command " + JsonConvert.SerializeObject(command));
			AddQueuedCommand(new QueuedCommand
			{
				Command = command,
				Caller = base.Clients.Caller
			});
		}
	}

	public void ExecuteDeviceServiceShutDownCommand(DeviceServiceShutDownCommand command)
	{
		if (command == null)
		{
			return;
		}
		_logger?.LogInformation("<<< " + command?.CommandName + " Command " + JsonConvert.SerializeObject(command));
		IClientProxy clientProxy = base.Clients?.Caller;
		if (_applicationControl != null)
		{
			Task.Run(delegate
			{
				bool success = _applicationControl.ShutDown(command.ForceShutDown, command.Reason);
				DeviceServiceShutDownResponseEvent deviceServiceShutDownResponseEvent = new DeviceServiceShutDownResponseEvent(command)
				{
					Success = success
				};
				LogEvent(">>> DeviceServiceShutDownResponseEvent", deviceServiceShutDownResponseEvent);
				clientProxy?.SendAsync("DeviceServiceShutDownResponseEvent", deviceServiceShutDownResponseEvent);
			});
		}
	}

	public void ExecuteDeviceServiceCanShutDownCommand(DeviceServiceCanShutDownCommand command)
	{
		if (command != null)
		{
			_logger?.LogInformation("<<< " + command?.CommandName + " Command " + JsonConvert.SerializeObject(command));
			if (command != null && _applicationControl != null)
			{
				_applicationControl.SetCanShutDownClientResponse(command.CanShutDown);
			}
		}
	}

	public void SetCommandQueueState(SetCommandQueueStateCommand command)
	{
		if (command != null)
		{
			_logger?.LogInformation("<<< " + command?.CommandName + " Command " + JsonConvert.SerializeObject(command));
			CommandQueuePaused = command.CommandQueuePaused;
		}
	}

	public void ExecuteReadCardCommand(ReadCardCommand command)
	{
		if (command != null)
		{
			_logger?.LogInformation("<<< ReadCardCommand " + JsonConvert.SerializeObject(command));
			AddQueuedCommand(new QueuedCommand
			{
				Command = command,
				Caller = base.Clients.Caller
			});
		}
	}

	private void ProcessCardReadModel(BaseResponseEvent responseEvent, CancellationToken cancellationToken)
	{
		ICardReadResponseEvent cardReadResponseEvent = responseEvent as ICardReadResponseEvent;
		if (cancellationToken.IsCancellationRequested)
		{
			responseEvent.Success = false;
			if (cardReadResponseEvent != null)
			{
				if (cardReadResponseEvent.GetBase87CardReadModel() == null && responseEvent is UnencryptedCardReadResponseEvent unencryptedCardReadResponseEvent)
				{
					unencryptedCardReadResponseEvent.Data = new UnencryptedCardReadModel();
				}
				cardReadResponseEvent.GetBase87CardReadModel().Status = ResponseStatus.Cancelled;
			}
		}
		responseEvent.Success = cardReadResponseEvent != null && cardReadResponseEvent.GetBase87CardReadModel()?.Status == ResponseStatus.Success;
		LogEvent($"CardReaderHub.ExecuteBaseCommand {responseEvent?.GetType()}", responseEvent);
	}

	public void ExecuteCancelCommand(CancelCommandRequest cancelCommandRequest)
	{
		if (cancelCommandRequest == null)
		{
			return;
		}
		_logger?.LogInformation("CardReaderHub.ExecuteCancelCommand " + JsonConvert.SerializeObject(cancelCommandRequest));
		bool success = false;
		_cancellationTokenSources.TryGetValue(cancelCommandRequest.CommandToCancelRequestId, out var value);
		if (value != null)
		{
			value.Cancel();
			_proxy.ReadCancel();
			success = true;
		}
		else
		{
			lock (_syncObject)
			{
				foreach (QueuedCommand item in _commandQueue)
				{
					if (item?.Command.RequestId == cancelCommandRequest?.CommandToCancelRequestId)
					{
						item.Cancelled = true;
						_logger?.LogInformation($"Canceled queued command {item?.Command.RequestId}");
						success = true;
					}
				}
			}
		}
		CancelCommandResponseEvent cancelCommandResponseEvent = new CancelCommandResponseEvent(cancelCommandRequest)
		{
			Success = success
		};
		LogEvent("CardReaderHub.ExecuteCancelCommand cancelCommandResponseEvent", cancelCommandResponseEvent);
		base.Clients.Caller.SendAsync("CancelCommandResponseEvent", cancelCommandResponseEvent);
	}

	public void SendShutDownStartingEvent(DeviceServiceShutDownStartingEvent deviceServiceShutDownStartingEvent)
	{
		if (deviceServiceShutDownStartingEvent != null)
		{
			_logger?.LogInformation(">>> SendShutDownStartingEvent Event " + JsonConvert.SerializeObject(deviceServiceShutDownStartingEvent));
			base.Clients.All.SendAsync("DeviceServiceShutDownStartingEvent", deviceServiceShutDownStartingEvent);
		}
	}

	public void SendCardReaderStateEvent(CardReaderStateEvent cardReaderStateEvent)
	{
		if (cardReaderStateEvent != null)
		{
			_logger?.LogInformation(">>> SendCardReaderStateEvent " + JsonConvert.SerializeObject(cardReaderStateEvent));
			base.Clients.All.SendAsync("CardReaderStateEvent", cardReaderStateEvent);
		}
	}

	public void ExecuteValidateVersionCommand(ValidateVersionCommand validateVersionCommand)
	{
		if (validateVersionCommand != null)
		{
			_logger?.LogInformation("<<< ValidateVersion Command " + JsonConvert.SerializeObject(validateVersionCommand));
			AddQueuedCommand(new QueuedCommand
			{
				Command = validateVersionCommand,
				Caller = base.Clients.Caller
			});
		}
	}

	public void ExecuteCheckActivationCommand(CheckActivationCommand checkActivaionCommand)
	{
		if (checkActivaionCommand != null)
		{
			_logger?.LogInformation($"<<< CheckActivation Command {checkActivaionCommand.Scrub()}");
			AddQueuedCommand(new QueuedCommand
			{
				Command = checkActivaionCommand,
				Caller = base.Clients.Caller
			});
		}
	}

	public void ExecuteCheckDeviceStatusCommand(CheckDeviceStatusCommand checkDeviceStatusCommand)
	{
		if (checkDeviceStatusCommand != null)
		{
			_logger?.LogInformation("<<< CheckDeviceStatus Command " + JsonConvert.SerializeObject(checkDeviceStatusCommand.Scrub()));
			AddQueuedCommand(new QueuedCommand
			{
				Command = checkDeviceStatusCommand,
				Caller = base.Clients.Caller
			});
		}
	}

	public void ExecuteReportAuthorizeResultCommand(ReportAuthorizeResultCommand reportAuthorizeResultCommand)
	{
		if (reportAuthorizeResultCommand != null)
		{
			_logger?.LogInformation("<<< ReportAuthorizeResult Command " + JsonConvert.SerializeObject(reportAuthorizeResultCommand));
			bool success = reportAuthorizeResultCommand.Success;
			_proxy.SetAuthorizationResponse(success);
			ReportAuthorizationResultResponseEvent reportAuthorizationResultResponseEvent = new ReportAuthorizationResultResponseEvent(reportAuthorizeResultCommand)
			{
				Success = success
			};
			LogEvent(">>> ReportAuthorizeResultResponseEvent", reportAuthorizationResultResponseEvent);
			base.Clients.Caller.SendAsync("ReportAuthorizationResultResponseEvent", reportAuthorizationResultResponseEvent);
		}
	}

	private void LogEvent(string eventContext, BaseEvent baseEvent)
	{
		if (baseEvent != null)
		{
			string logText = null;
			LoggerExtensions.LogInformation(message: eventContext + " " + ((!(baseEvent is EMVCardReadResponseEvent responseEvent)) ? ((!(baseEvent is EncryptedCardReadResponseEvent responseEvent2)) ? ((!(baseEvent is UnencryptedCardReadResponseEvent responseEvent3)) ? JsonConvert.SerializeObject(baseEvent) : ObfuscateUnencryptedCardReadResponseEvent(responseEvent3, logText)) : ObfuscateEncryptedCardReadResponseEvent(responseEvent2, logText)) : ObfuscateEMVCardReadResponseEvent(responseEvent, logText)), logger: _logger?, args: Array.Empty<object>());
		}
	}

	private string ObfuscateEMVCardReadResponseEvent(EMVCardReadResponseEvent responseEvent, string logText)
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

	private string ObfuscateEncryptedCardReadResponseEvent(EncryptedCardReadResponseEvent responseEvent, string logText)
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

	private string ObfuscateUnencryptedCardReadResponseEvent(UnencryptedCardReadResponseEvent responseEvent, string logText)
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
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			string arg = JsonConvert.SerializeObject(responseEvent, jsonSerializerSettings);
			result = JsonConvert.SerializeObject(ObfuscateJsonAction(arg));
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unhandled exception in CardReaderHub.CreateResponseEventLogText while deserializing a CardReadResponseEvent");
		}
		return result;
	}

	public override Task OnConnectedAsync()
	{
		_logger.LogInformation("CardReaderHub.OnConnectedAsync event");
		_analytics?.ClientConnectedToHub();
		SendMessage("Hello");
		if (_proxy?.UnitData?.IsTampered == true)
		{
			SendSimpleEvent(new DeviceTamperedEvent());
		}
		CardReaderState cardReaderState = _proxy?.GetCardReaderState();
		if (cardReaderState != null)
		{
			SendCardReaderStateEvent(new CardReaderStateEvent
			{
				CardReaderState = cardReaderState
			});
		}
		return base.OnConnectedAsync();
	}

	private void ClearCommands()
	{
		CommandQueuePaused = true;
		ConcurrentQueue<QueuedCommand> concurrentQueue = new ConcurrentQueue<QueuedCommand>();
		foreach (QueuedCommand item in _commandQueue)
		{
			if (item.ConnectionId != base.Context.ConnectionId)
			{
				concurrentQueue.Enqueue(item);
				continue;
			}
			_cancellationTokenSources.TryRemove(item.Command.RequestId, out var value);
			value?.Cancel();
		}
		_commandQueue = concurrentQueue;
		if (ActiveCommand?.ConnectionId == base.Context.ConnectionId)
		{
			_cancellationTokenSources[ActiveCommand.Command.RequestId].Cancel();
			ActiveCommand.Cancelled = true;
			if (ActiveCommand.Command.SignalRCommand == "ExecuteReadCardCommand")
			{
				_proxy.ReadCancel();
			}
		}
		CommandQueuePaused = false;
	}

	public override Task OnDisconnectedAsync(Exception exception)
	{
		_logger.LogInformation("CardReaderHub.OnDisconnectedAsync event");
		_analytics?.ClientDisconnectedFromHub();
		if (exception != null)
		{
			_logger.LogError(exception, "CardReaderHub.OnDisconnectedAsync - Unhandled exception.");
		}
		ClearCommands();
		return base.OnDisconnectedAsync(exception);
	}
}
