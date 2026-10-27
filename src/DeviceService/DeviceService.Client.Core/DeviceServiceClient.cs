using System;
using System.Reflection;
using System.Threading;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;

namespace DeviceService.Client.Core;

public class DeviceServiceClient : IDeviceServiceClientCore
{
	private readonly IHttpService _httpService;

	private object _lockConnectionObject = new object();

	private AutoResetEvent _connectionAutoResetEvent;

	private int _defaultCommandTimeout = 180000;

	private string _url;

	private SignalRClient _signalRClient;

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

	public event LogHandler OnLog;

	public event OnConnected OnConnectedHandler;

	public event OnDisconnected OnDisconnectedHandler;

	public DeviceServiceClient(IHttpService httpService)
	{
		_signalRClient = new SignalRClient();
		_signalRClient.OnLog += delegate(string logText)
		{
			Log(logText);
		};
		_signalRClient.OnConnectedHandler += SignalRConnected;
		_signalRClient.OnDisconnectedHandler += _signalRClient_OnDisconnectedHandler;
		_httpService = httpService;
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
			StartConnectionToDeviceService(url);
			_connectionAutoResetEvent.WaitOne(connectionTimeout);
			return IsConnectedToDeviceService;
		}
	}

	public void StartConnectionToDeviceService(string url)
	{
		_url = url;
		Log("Begin Connecting to DeviceService at url: " + _url);
		_signalRClient.Connect(_url);
	}

	public void SendCardReaderConnectedEvent()
	{
		SendSimpleEvent(new CardReaderConnectedEvent());
	}

	public void SendCardReaderStateEvent(CardReaderState cardReaderState)
	{
		_signalRClient.SendCardReaderStateEvent(new CardReaderStateEvent
		{
			CardReaderState = cardReaderState
		});
	}

	public void SendCardReaderDisconnectedEvent()
	{
		SendSimpleEvent(new CardReaderDisconnectedEvent());
	}

	public void SendDeviceServiceCanShutDownEvent()
	{
		SendSimpleEvent(new DeviceServiceCanShutDownEvent());
	}

	public void SendDeviceServiceShutDownStartingEvent(ShutDownReason shutDownReason)
	{
		_signalRClient.SendDeviceServiceShutDownStartingEvent(new DeviceServiceShutDownStartingEvent
		{
			ShutDownReason = shutDownReason
		});
	}

	public void SendDeviceTamperedEvent()
	{
		SendSimpleEvent(new DeviceTamperedEvent());
	}

	public void SetCommandQueueState(bool commandQueuePaused)
	{
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		_signalRClient.SetCommandQueueState(new SetCommandQueueStateCommand(version)
		{
			CommandQueuePaused = commandQueuePaused
		});
	}

	private void SendSimpleEvent(SimpleEvent simpleEvent)
	{
		LogEvent(simpleEvent);
		_signalRClient.SendSimpleEvent(simpleEvent);
	}

	private void LogEvent(BaseEvent baseEvent)
	{
		Log(">>> " + baseEvent.EventName + ": " + JsonConvert.SerializeObject(baseEvent));
	}

	private void Log(string logText)
	{
		OnLog?.Invoke(logText);
	}

	private void SignalRConnected()
	{
		Log("DeviceService connected.");
		OnConnectedHandler?.Invoke();
		if (_connectionAutoResetEvent != null)
		{
			_connectionAutoResetEvent.Set();
		}
	}

	private void _signalRClient_OnDisconnectedHandler(Exception exception)
	{
		Log($"DeviceService disconnected. {exception}");
		OnDisconnectedHandler?.Invoke(exception);
	}
}
