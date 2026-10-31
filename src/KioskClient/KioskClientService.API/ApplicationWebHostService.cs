using System;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Analytics;
using KioskClientService.API.Services.Analytics.KioskDataService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API;

// On ASP.NET Core 2.2 this was a WebHostService subclass (the Windows service wrapper). On the generic host the
// same four notifications come from IHostApplicationLifetime; Program registers this only when running as a service.
public class ApplicationWebHostService : IHostedService
{
	private readonly IServiceProvider _services;

	private readonly IHostApplicationLifetime _lifetime;

	private readonly ILogger<ApplicationWebHostService> _logger;

	private IAnalyticsService _analytics;

	private DateTime _onStartingTime;

	public ApplicationWebHostService(IServiceProvider services, IHostApplicationLifetime lifetime, ILogger<ApplicationWebHostService> logger)
	{
		_services = services;
		_lifetime = lifetime;
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		OnStarting();
		_lifetime.ApplicationStarted.Register(OnStarted);
		_lifetime.ApplicationStopping.Register(OnStopping);
		_lifetime.ApplicationStopped.Register(OnStopped);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	private void OnStarting()
	{
		_logger?.LogInfoWithSource("service starting", "OnStarting");
		_onStartingTime = DateTime.Now;
	}

	private void OnStarted()
	{
		_logger?.LogInfoWithSource("service started", "OnStarted");
		try
		{
			_analytics = _services.GetService<IAnalyticsService>();
			_analytics?.PurgeOldAnalytics(TimeSpan.FromDays(30.0));
			_analytics?.ServiceEvent(KioskClientEvent.ServiceStarting, _onStartingTime);
			_analytics?.ServiceEvent(KioskClientEvent.ServiceStarted);
			Task.Run(SendSavedServiceEvents);
		}
		catch (Exception exception)
		{
			_logger.LogErrorWithSource($"an unhandled exception occurred. {exception}", "OnStarted");
		}
	}

	private void OnStopping()
	{
		_logger?.LogInfoWithSource("service stopping", "OnStopping");
		try
		{
			_analytics.SaveServiceEvent(new ServiceEvent
			{
				EventType = KioskClientEvent.ServiceStopping
			});
		}
		catch (Exception exception)
		{
			_logger.LogErrorWithSource($"an unhandled exception occurred. {exception}", "OnStopping");
		}
	}

	private void OnStopped()
	{
		_logger?.LogInfoWithSource("service stopped", "OnStopped");
		try
		{
			_analytics.SaveServiceEvent(new ServiceEvent
			{
				EventType = KioskClientEvent.ServiceStopped
			});
		}
		catch (Exception exception)
		{
			_logger.LogErrorWithSource($"an unhandled exception occurred. {exception}", "OnStopped");
		}
	}

	private async Task SendSavedServiceEvents()
	{
		ServiceEvent stopping = _analytics.LoadServiceEvent(KioskClientEvent.ServiceStopping);
		if (stopping != null)
		{
			await _analytics.ServiceEvent(stopping.EventType, stopping.EventTime);
		}
		ServiceEvent stopped = _analytics.LoadServiceEvent(KioskClientEvent.ServiceStopped);
		if (stopped != null)
		{
			await _analytics.ServiceEvent(stopped.EventType, stopped.EventTime);
		}
		_analytics.PurgeServiceEvents();
	}
}
