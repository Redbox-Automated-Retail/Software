using System;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using Microsoft.AspNetCore.SignalR.Client;

namespace DeviceService.Client.Core;

public class SignalRClient
{
	private string _url;

	private HubConnection _hubConnection;

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

	public async void Connect(string url)
	{
		_url = url;
		_hubConnection = new HubConnectionBuilder().WithUrl(_url).Build();
		if (_hubConnection != null)
		{
			AddMessageHandlers(_hubConnection);
			try
			{
				await _hubConnection.StartAsync();
				if (_hubConnection.State == HubConnectionState.Connected)
				{
					OnConnectedHandler?.Invoke();
				}
				return;
			}
			catch (Exception arg)
			{
				Log($"Exception in SignalRClient.Connect: {arg}");
				return;
			}
		}
		Log("Unable to create SignalR HubConnection.");
	}

	public void SendSimpleEvent(SimpleEvent simpleEvent)
	{
		_hubConnection.SendAsync("SendSimpleEvent", simpleEvent);
	}

	public void SendDeviceServiceShutDownStartingEvent(DeviceServiceShutDownStartingEvent deviceServiceShutDownStartingEvent)
	{
		_hubConnection.SendAsync("SendShutDownStartingEvent", deviceServiceShutDownStartingEvent);
	}

	public void SendCardReaderStateEvent(CardReaderStateEvent cardReaderStateEvent)
	{
		_hubConnection.SendAsync("SendCardReaderStateEvent", cardReaderStateEvent);
	}

	public void SendDeviceTamperedEvent(DeviceTamperedEvent deviceTamperedEvent)
	{
		_hubConnection.SendAsync("SendDeviceTamperedEvent", deviceTamperedEvent);
	}

	public void SetCommandQueueState(SetCommandQueueStateCommand setCommandQueueStateCommand)
	{
		_hubConnection.SendAsync("SetCommandQueueState", setCommandQueueStateCommand);
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
		}
	}

	private Task HubConnection_Closed(Exception arg)
	{
		OnDisconnectedHandler?.Invoke(arg);
		return null;
	}
}
