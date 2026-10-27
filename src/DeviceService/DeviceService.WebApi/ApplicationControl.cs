using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace DeviceService.WebApi;

public class ApplicationControl : IApplicationControl
{
	private IApplicationLifetime _applicationLifetime;

	private ILogger<ApplicationControl> _logger;

	private IIUC285Notifier _iuc285Notifier;

	private AutoResetEvent _canShutDownAutoResetEvent;

	private bool _canShutDownClientResponse;

	public static bool IsService { get; set; }

	public ApplicationControl(IApplicationLifetime applicationLifetime, ILogger<ApplicationControl> logger, IIUC285Notifier iuc285Notifier)
	{
		_applicationLifetime = applicationLifetime;
		_logger = logger;
		_iuc285Notifier = iuc285Notifier;
	}

	public bool CanShutDown(ShutDownReason shutDownReason)
	{
		string text = null;
		_canShutDownClientResponse = true;
		if (_canShutDownClientResponse)
		{
			_iuc285Notifier?.SendDeviceServiceCanShutDownEvent();
			_canShutDownAutoResetEvent = new AutoResetEvent(initialState: false);
			_canShutDownAutoResetEvent.WaitOne(5000);
			if (!_canShutDownClientResponse)
			{
				text = "Kiosk Engine prevented shutdown";
			}
		}
		else
		{
			text = "Device Service Command(s) are currently processing";
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = "  Reason: " + text;
		}
		_logger?.LogInformation($"ApplicationControl.CanShutDown: {_canShutDownClientResponse} {text}");
		return _canShutDownClientResponse;
	}

	public void SetCanShutDownClientResponse(bool clientAllowsShutDown)
	{
		_canShutDownClientResponse &= clientAllowsShutDown;
		if (!clientAllowsShutDown && _canShutDownAutoResetEvent != null)
		{
			_logger?.LogInformation($"ApplicationControl.SetCanShutDownClientResponse value: {clientAllowsShutDown}");
			_canShutDownAutoResetEvent.Set();
		}
	}

	public bool ShutDown(bool forceShutdown, ShutDownReason shutDownReason)
	{
		_logger?.LogInformation($"ApplicationControl.ShutDown: forceShutdown: {forceShutdown}, ShutDown Reason: {shutDownReason}");
		bool result = false;
		if (forceShutdown || CanShutDown(shutDownReason))
		{
			try
			{
				_logger?.LogInformation("ApplicationControl.ShutDown: starting shut down");
				Task.Run(async delegate
				{
					_iuc285Notifier.SendDeviceServiceShutDownStartingEvent(shutDownReason);
					await Task.Delay(3000);
					if (IsService)
					{
						ServiceController serviceController = new ServiceController("device$service");
						if (serviceController != null)
						{
							_logger?.LogInformation("Stopping service application.");
							serviceController?.Stop();
						}
						else
						{
							_logger?.LogError("Unable to find service device$service.  Unable to shut down windows service.");
						}
					}
					else
					{
						_logger?.LogInformation("Stopping non-service application.");
						_applicationLifetime?.StopApplication();
					}
				});
				result = true;
			}
			catch (Exception arg)
			{
				_logger?.LogInformation($"ApplicationControl.ShutDown failed.  {arg}");
			}
		}
		return result;
	}
}
