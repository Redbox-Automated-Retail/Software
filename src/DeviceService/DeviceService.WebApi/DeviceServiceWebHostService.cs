using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel.Analytics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeviceService.WebApi;

// On ASP.NET Core 2.2 this was a WebHostService subclass (the Windows service wrapper). On the generic host the
// same four notifications come from IHostApplicationLifetime; Program registers this only when running as a service.
public class DeviceServiceWebHostService : IHostedService
{
	private readonly ILogger _logger;

	private readonly IAnalyticsService _analytics;

	private readonly IHostApplicationLifetime _lifetime;

	public DeviceServiceWebHostService(IHostApplicationLifetime lifetime, ILogger<DeviceServiceWebHostService> logger, IAnalyticsService analytics)
	{
		_lifetime = lifetime;
		_logger = logger;
		_analytics = analytics;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_logger?.LogInformation("WebHost: OnStarting");
		_analytics?.ServiceStarting();
		_lifetime.ApplicationStarted.Register(delegate
		{
			_logger?.LogInformation("WebHost: OnStarted");
			_analytics?.ServiceStarted();
		});
		_lifetime.ApplicationStopping.Register(delegate
		{
			_logger?.LogInformation("WebHost: OnStopping");
			_analytics?.ServiceStopping();
		});
		_lifetime.ApplicationStopped.Register(delegate
		{
			_logger?.LogInformation("WebHost: OnStopped");
			_analytics?.ServiceStopped();
		});
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
