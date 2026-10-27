using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API;

public class ApplicationControl : IApplicationControl
{
	private IApplicationLifetime _applicationLifetime;

	private ILogger<ApplicationControl> _logger;

	private string _serviceName;

	private AutoResetEvent _canShutDownAutoResetEvent;

	private bool _canShutDownClientResponse;

	public static bool IsService { get; set; }

	public ApplicationControl(string serviceName, IApplicationLifetime applicationLifetime, ILogger<ApplicationControl> logger)
	{
		_applicationLifetime = applicationLifetime;
		_logger = logger;
		_serviceName = "kioskclient$service";
	}

	public bool CanShutDown(ShutDownReason shutDownReason)
	{
		string text = null;
		_canShutDownClientResponse = true;
		_canShutDownAutoResetEvent = new AutoResetEvent(initialState: false);
		_canShutDownAutoResetEvent.WaitOne(5000);
		if (!_canShutDownClientResponse)
		{
			text = "something prevented shutdown";
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = "  Reason: " + text;
		}
		_logger?.LogInformation($"ApplicationControl.CanShutDown -  {_canShutDownClientResponse} {text}");
		return _canShutDownClientResponse;
	}

	public void SetCanShutDownClientResponse(bool clientAllowsShutDown)
	{
		_canShutDownClientResponse &= clientAllowsShutDown;
		if (!clientAllowsShutDown && _canShutDownAutoResetEvent != null)
		{
			_logger?.LogInformation($"ApplicationControl.SetCanShutDownClientResponse - value: {clientAllowsShutDown}");
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
				Task.Run(delegate
				{
					if (IsService)
					{
						string serviceName = _serviceName;
						ServiceController serviceController = new ServiceController(serviceName);
						if (serviceController != null)
						{
							_logger?.LogInformation("ApplicationControl.ShutDown - Stopping service application.");
							serviceController?.Stop();
						}
						else
						{
							_logger?.LogError("ApplicationControl.ShutDown - Unable to find service " + _serviceName + ".  Unable to shut down windows service.");
						}
					}
					else
					{
						_logger?.LogInformation("ApplicationControl.ShutDown  - Stopping non-service application.");
						_applicationLifetime?.StopApplication();
					}
				});
				result = true;
			}
			catch (Exception exception)
			{
				_logger?.LogError(exception, "ApplicationControl.ShutDown failed.");
			}
		}
		return result;
	}
}
