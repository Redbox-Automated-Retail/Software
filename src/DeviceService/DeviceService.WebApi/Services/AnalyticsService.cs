using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Analytics;
using Microsoft.Extensions.Logging;

namespace DeviceService.WebApi.Services;

public class AnalyticsService : IAnalyticsService
{
	private ILogger<AnalyticsService> _logger;

	private IApplicationSettings _applicationSettings;

	public AnalyticsService(IApplicationSettings applicationSettings, ILogger<AnalyticsService> logger)
	{
		_logger = logger;
		_applicationSettings = applicationSettings;
	}

	public void StartWebHost()
	{
		_logger.LogInformation("Analytics: Start WebHost");
	}

	public void StopWebHost()
	{
		_logger.LogInformation("Analytics: Stop WebHost");
	}

	public void ClientConnectedToHub()
	{
		_logger.LogInformation("Analytics: client connected to hub");
	}

	public void ClientDisconnectedFromHub()
	{
		_logger.LogInformation("Analytics: client disconnected from hub");
	}

	public void ServiceStarted()
	{
		_logger.LogInformation("Analytics: service started");
	}

	public void ServiceStarting()
	{
		_logger.LogInformation("Analytics: service starting");
	}

	public void ServiceStopped()
	{
		_logger.LogInformation("Analytics: service stopped");
	}

	public void ServiceStopping()
	{
		_logger.LogInformation("Analytics: service stopping");
	}
}
