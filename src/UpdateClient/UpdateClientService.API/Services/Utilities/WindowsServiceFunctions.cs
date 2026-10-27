using System;
using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Utilities;

public class WindowsServiceFunctions : IWindowsServiceFunctions
{
	private readonly ICommandLineService _powerShellService;

	private readonly ILogger<WindowsServiceFunctions> _logger;

	public WindowsServiceFunctions(ICommandLineService powerShellService, ILogger<WindowsServiceFunctions> logger)
	{
		_powerShellService = powerShellService;
		_logger = logger;
	}

	public bool ServiceExists(string name)
	{
		ServiceController[] services = ServiceController.GetServices();
		foreach (ServiceController serviceController in services)
		{
			if (serviceController.ServiceName == name)
			{
				return true;
			}
		}
		return false;
	}

	public void StartService(string name)
	{
		try
		{
			ServiceController serviceController = new ServiceController(name);
			try
			{
				serviceController.Start();
				serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(5.0));
				_logger.LogInfoWithSource($"Exiting from startService. Service name: {name} was {serviceController.Status}", "StartService", "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
			}
			finally
			{
				((IDisposable)serviceController)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception was raised while starting service: " + name + ".", "StartService", "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
		}
	}

	public void StopService(string name)
	{
		try
		{
			ServiceController serviceController = new ServiceController(name);
			try
			{
				serviceController.Stop();
				serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(5.0));
				_logger.LogInfoWithSource($"Exiting from stopService. Service name: {name} was {serviceController.Status}", "StopService", "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
			}
			finally
			{
				((IDisposable)serviceController)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception was raised while stopping service: " + name + ".", "StopService", "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
		}
	}

	public void InstallService(string name, string displayName, string binPath, string type = null, string startType = null, string dependencies = null)
	{
		try
		{
			string text = string.Format("create {0} displayName= \"{1}\" binPath= \"{2}\" type= {3} start= {4}", name, displayName, binPath, type ?? "own", startType ?? "auto");
			if (!string.IsNullOrEmpty(dependencies))
			{
				text += $" depend= {dependencies}";
			}
			_powerShellService.TryExecuteCommand("SC.exe", text);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception was raised while installing service " + name, "InstallService", "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
		}
	}
}
