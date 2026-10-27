using DeviceService.ComponentModel.Analytics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;

namespace DeviceService.WebApi;

public class DeviceServiceWebHostService : WebHostService
{
	private ILogger _logger;

	private IAnalyticsService _analytics;

	public DeviceServiceWebHostService(IWebHost host, ILogger<DeviceServiceWebHostService> logger, IAnalyticsService analytics)
		: base(host)
	{
		_logger = logger;
		_analytics = analytics;
	}

	protected override void OnStarting(string[] args)
	{
		_logger?.LogInformation("WebHost: OnStarting");
		_analytics?.ServiceStarting();
		base.OnStarting(args);
	}

	protected override void OnStarted()
	{
		_logger?.LogInformation("WebHost: OnStarted");
		_analytics?.ServiceStarted();
		base.OnStarted();
	}

	protected override void OnStopping()
	{
		_logger?.LogInformation("WebHost: OnStopping");
		_analytics?.ServiceStopping();
		base.OnStopping();
	}

	protected override void OnStopped()
	{
		_logger?.LogInformation("WebHost: OnStopped");
		_analytics?.ServiceStopped();
		base.OnStopped();
	}
}
