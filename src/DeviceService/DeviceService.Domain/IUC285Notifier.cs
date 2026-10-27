using System;
using DeviceService.Client.Core;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;

namespace DeviceService.Domain;

public class IUC285Notifier : IIUC285Notifier
{
	private IDeviceServiceClientCore _deviceServiceClient;

	private IApplicationSettings _settings;

	private ILogger<IUC285Notifier> _logger;

	private string _url;

	public IUC285Notifier(ILogger<IUC285Notifier> logger, IHttpService httpService, IApplicationSettings settings)
	{
		_settings = settings;
		_logger = logger;
		_deviceServiceClient = new DeviceServiceClient(httpService);
		_deviceServiceClient.OnLog += _deviceServiceClient_OnLog;
		_url = GetDeviceServiceClientUrl();
		Log("Configuring IUC285Notifier with DeviceServiceClientUrl: " + _url);
	}

	private void _deviceServiceClient_OnLog(string logText)
	{
		Log(logText);
	}

	private Uri GetDeviceServiceUri()
	{
		return new Uri(_settings.DeviceServiceUrl);
	}

	private string GetDeviceServiceClientUrl()
	{
		string result = null;
		try
		{
			UriBuilder uriBuilder = new UriBuilder(GetDeviceServiceUri());
			uriBuilder.Path += _settings.DeviceServiceClientPath;
			result = uriBuilder.ToString();
		}
		catch (Exception arg)
		{
			_logger?.LogInformation($"Startup.GetDeviceServiceUrl: Unable to create DeviceServiceClientUrl.  {arg}");
		}
		return result;
	}

	private void Log(string s)
	{
		_logger?.LogInformation(s);
	}

	private bool ConnectToDeviceServiceClient()
	{
		bool flag = false;
		if (_deviceServiceClient != null)
		{
			flag = _deviceServiceClient.IsConnectedToDeviceService;
			if (!flag)
			{
				int connectionTimeout = 5000;
				flag = _deviceServiceClient.ConnectToDeviceService(_url, connectionTimeout);
			}
		}
		if (!flag)
		{
			Log("IUC285Notifier: device service client is null or not connected.  event not sent");
		}
		return flag;
	}

	public void SendCardReaderConnectedEvent()
	{
		if (ConnectToDeviceServiceClient())
		{
			_deviceServiceClient.SendCardReaderConnectedEvent();
		}
	}

	public void SendCardReaderStateEvent(CardReaderState cardReaderState)
	{
		if (ConnectToDeviceServiceClient())
		{
			_deviceServiceClient.SendCardReaderStateEvent(cardReaderState);
		}
	}

	public void SendCardReaderDisconnectedEvent(Exception exception)
	{
		if (ConnectToDeviceServiceClient())
		{
			_deviceServiceClient.SendCardReaderDisconnectedEvent();
		}
	}

	public void SendDeviceServiceCanShutDownEvent()
	{
		if (ConnectToDeviceServiceClient())
		{
			_deviceServiceClient.SendDeviceServiceCanShutDownEvent();
		}
	}

	public void SendDeviceServiceShutDownStartingEvent(ShutDownReason shutDownReason)
	{
		if (ConnectToDeviceServiceClient())
		{
			_deviceServiceClient.SendDeviceServiceShutDownStartingEvent(shutDownReason);
		}
	}

	public void SetCommandQueueState(bool commandQueuePaused)
	{
		if (ConnectToDeviceServiceClient())
		{
			_deviceServiceClient.SetCommandQueueState(commandQueuePaused);
		}
	}

	public void SendDeviceTamperedEvent()
	{
		if (ConnectToDeviceServiceClient())
		{
			_deviceServiceClient.SendDeviceTamperedEvent();
		}
	}
}
