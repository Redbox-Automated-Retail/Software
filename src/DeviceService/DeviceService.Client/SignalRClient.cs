using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using Microsoft.AspNetCore.SignalR.Client;

namespace DeviceService.Client;

public class SignalRClient
{
	public delegate void OnIsConnectedResponseEvent(IsConnectedResponseEvent isConnectedResponseEvent);

	public delegate void OnGetUnitHealthResponseEvent(GetUnitHealthResponseEvent getUnitHealthResponseEvent);

	public delegate void OnValidateVersionResponseEvent(ValidateVersionResponseEvent validateVersionResponseEvent);

	public delegate void OnCheckActivationResponseEvent(CheckActivationResponseEvent checkActivationResponseEvent);

	public delegate void OnCheckDeviceStatusResponseEvent(CheckDeviceStatusResponseEvent checkDeviceStatusResponseEvent);

	public delegate void OnGetCardInsertedStatusResponseEvent(GetCardInsertedStatusResponseEvent getCardInsertedStatusResponseEvent);

	public delegate void OnDeviceServiceShutDownResponseEvent(DeviceServiceShutDownResponseEvent deviceServiceShutDownResponseEvent);

	public delegate void OnDeviceServiceShutDownStartingEvent(DeviceServiceShutDownStartingEvent deviceServiceShutDownStartingEvent);

	public delegate void OnSupportsEMVResponseEvent(SupportsEMVResponseEvent supportsEMVResponseEvent);

	public delegate void OnCardReaderStateEvent(CardReaderStateEvent cardReaderStateEvent);

	public delegate void OnEvent(BaseEvent baseEvent);

	private string _url;

	private HubConnection _hubConnection;

	public string Url => _url;

	public bool IsConnectedToDeviceService
	{
		get
		{
			if (_hubConnection != null)
			{
				return _hubConnection.State == HubConnectionState.Connected;
			}
			return false;
		}
	}

	public event LogHandler OnLog;

	public event OnConnected OnConnectedHandler;

	public event OnDisconnected OnDisconnectedHandler;

	public event OnEvent OnCardReaderConnectedHandler;

	public event OnEvent OnCardReaderDisconnectedHandler;

	public event OnIsConnectedResponseEvent OnIsConnectedResponseEventHandler;

	public event OnGetUnitHealthResponseEvent OnGetUnitHealthResponseEventHandler;

	public event Action<BaseResponseEvent> OnCardReadResponseEventHandler;

	public event Action<SimpleResponseEvent> OnReadConfigurationEventHandler;

	public event Action<SimpleResponseEvent> OnWriteConfigurationEventHandler;

	public event Action<SimpleResponseEvent> OnCardRemovedEventHandler;

	public event Action<SimpleResponseEvent> OnDeviceServiceCanShutDownEventHandler;

	public event Action<BaseResponseEvent> OnCardReadEvent;

	public event Action<CancelCommandResponseEvent> OnCancelCommandResponseEventHandler;

	public event Action<RebootCardReaderResponseEvent> OnRebootCardReaderResponseEventHandler;

	public event Action<ReportAuthorizationResultResponseEvent> OnReportAuthorizationResultResponseEventHandler;

	public event OnValidateVersionResponseEvent OnValidateVersionResponseEventHandler;

	public event OnCheckActivationResponseEvent OnCheckActivationResponseEventHandler;

	public event OnCheckDeviceStatusResponseEvent OnCheckDeviceStatusResponseEventHandler;

	public event OnGetCardInsertedStatusResponseEvent OnGetCardInsertedStatusResponseEventHandler;

	public event OnDeviceServiceShutDownResponseEvent OnDeviceServiceShutDownResponseEventHandler;

	public event OnDeviceServiceShutDownStartingEvent OnDeviceServiceShutDownStartingEventHandler;

	public event OnSupportsEMVResponseEvent OnSupportsEMVResponseEventHandler;

	public event OnEvent OnDeviceTamperedEventHandler;

	public event OnCardReaderStateEvent OnCardReaderStateEventHandler;

	public async Task Connect(string url, int? connectionTimeout)
	{
		_url = url;
		_hubConnection = new HubConnectionBuilder().WithUrl(_url).Build();
		if (_hubConnection != null)
		{
			AddMessageHandlers(_hubConnection);
			try
			{
				CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
				CancellationToken cancellationToken = cancellationTokenSource.Token;
				bool timedout = false;
				Task task = Task.Run(delegate
				{
					int num = connectionTimeout ?? 5000;
					int num2 = 0;
					while (!cancellationToken.IsCancellationRequested && num2 < num)
					{
						Thread.Sleep(1000);
						num2 += 1000;
					}
					if (num2 >= num && !cancellationToken.IsCancellationRequested)
					{
						timedout = true;
					}
					cancellationTokenSource.Cancel();
				}, cancellationToken);
				try
				{
					_hubConnection.StartAsync(cancellationToken).ContinueWith(delegate(Task task2)
					{
						if (task2.IsFaulted)
						{
							timedout = true;
						}
						cancellationTokenSource.Cancel();
					});
					await task;
				}
				catch (OperationCanceledException)
				{
					if (_hubConnection.State == HubConnectionState.Disconnected)
					{
						Log("DeviceServiceClient SignalRClient connection attempt timed out. ");
					}
				}
				if (_hubConnection.State == HubConnectionState.Connected)
				{
					OnConnectedHandler?.Invoke();
				}
				else if (timedout)
				{
					Log("DeviceServiceClient SignalRClient connection attempt timed out. ");
				}
				else
				{
					Log("DeviceServiceClient SignalRClient connection failed. ");
				}
				return;
			}
			catch (Exception arg)
			{
				Log($"Exception in DeviceServiceClient SignalRClient.Connect: {arg}");
				return;
			}
		}
		Log("Unable to create DeviceServiceClient SignalR HubConnection.");
	}

	public bool Disconnect()
	{
		_hubConnection.StopAsync();
		return _hubConnection.State == HubConnectionState.Disconnected;
	}

	private void Log(string logText)
	{
		OnLog?.Invoke(logText);
	}

	private void AddMessageHandlers(HubConnection hubConnection)
	{
		if (hubConnection != null)
		{
			hubConnection.Closed += HubConnection_Closed;
			hubConnection.On("IsConnectedResponseEvent", delegate(IsConnectedResponseEvent isConnectedResponseEvent)
			{
				OnIsConnectedResponseEventHandler?.Invoke(isConnectedResponseEvent);
			});
			hubConnection.On("GetUnitHealthResponseEvent", delegate(GetUnitHealthResponseEvent getUnitHealthResponseEvent)
			{
				OnGetUnitHealthResponseEventHandler?.Invoke(getUnitHealthResponseEvent);
			});
			hubConnection.On("EMVCardReadResponseEvent", delegate(EMVCardReadResponseEvent readCardResponseEvent)
			{
				OnCardReadResponseEventHandler?.Invoke(readCardResponseEvent);
			});
			hubConnection.On("EncryptedCardReadResponseEvent", delegate(EncryptedCardReadResponseEvent readCardResponseEvent)
			{
				OnCardReadResponseEventHandler?.Invoke(readCardResponseEvent);
			});
			hubConnection.On("UnencryptedCardReadResponseEvent", delegate(UnencryptedCardReadResponseEvent readCardResponseEvent)
			{
				OnCardReadResponseEventHandler?.Invoke(readCardResponseEvent);
			});
			hubConnection.On("ReadConfiguration", delegate(SimpleResponseEvent response)
			{
				OnReadConfigurationEventHandler?.Invoke(response);
			});
			hubConnection.On("WriteConfiguration", delegate(SimpleResponseEvent response)
			{
				OnWriteConfigurationEventHandler?.Invoke(response);
			});
			hubConnection.On("CardRemovedResponseEvent", delegate(SimpleResponseEvent response)
			{
				OnCardRemovedEventHandler?.Invoke(response);
			});
			hubConnection.On("CardProcessingStartedResponseEvent", delegate(SimpleResponseEvent response)
			{
				OnCardReadEvent?.Invoke(response);
			});
			hubConnection.On("DeviceServiceCanShutDownEvent", delegate(SimpleResponseEvent response)
			{
				OnDeviceServiceCanShutDownEventHandler?.Invoke(response);
			});
			hubConnection.On("CardReaderConnectedEvent", delegate(CardReaderConnectedEvent response)
			{
				OnCardReaderConnectedHandler?.Invoke(response);
			});
			hubConnection.On("CardReaderDisconnectedEvent", delegate(CardReaderDisconnectedEvent response)
			{
				OnCardReaderDisconnectedHandler?.Invoke(response);
			});
			hubConnection.On("CancelCommandResponseEvent", delegate(CancelCommandResponseEvent response)
			{
				OnCancelCommandResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("RebootCardReaderResponseEvent", delegate(RebootCardReaderResponseEvent response)
			{
				OnRebootCardReaderResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("ReportAuthorizationResultResponseEvent", delegate(ReportAuthorizationResultResponseEvent response)
			{
				OnReportAuthorizationResultResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("ValidateVersionReponseEvent", delegate(ValidateVersionResponseEvent response)
			{
				OnValidateVersionResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("CheckActivationResponseEvent", delegate(CheckActivationResponseEvent response)
			{
				OnCheckActivationResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("CheckDeviceStatusResponseEvent", delegate(CheckDeviceStatusResponseEvent response)
			{
				OnCheckDeviceStatusResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("GetCardInsertedStatusResponseEvent", delegate(GetCardInsertedStatusResponseEvent response)
			{
				OnGetCardInsertedStatusResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("DeviceServiceShutDownResponseEvent", delegate(DeviceServiceShutDownResponseEvent response)
			{
				OnDeviceServiceShutDownResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("DeviceServiceShutDownStartingEvent", delegate(DeviceServiceShutDownStartingEvent response)
			{
				OnDeviceServiceShutDownStartingEventHandler?.Invoke(response);
			});
			hubConnection.On("SupportsEMVResponseEvent", delegate(SupportsEMVResponseEvent response)
			{
				Log("SignalRClient invoking OnSupportsEMVResponseEventHandler");
				OnSupportsEMVResponseEventHandler?.Invoke(response);
			});
			hubConnection.On("DeviceTamperedEvent", delegate(DeviceTamperedEvent response)
			{
				OnDeviceTamperedEventHandler?.Invoke(response);
			});
			hubConnection.On("CardReaderStateEvent", delegate(CardReaderStateEvent response)
			{
				OnCardReaderStateEventHandler?.Invoke(response);
			});
		}
	}

	private Task HubConnection_Closed(Exception arg)
	{
		OnDisconnectedHandler?.Invoke(arg);
		return null;
	}

	public void SendCommand(BaseCommandRequest baseCommandRequest)
	{
		_hubConnection.SendAsync(baseCommandRequest.SignalRCommand, baseCommandRequest);
	}
}
