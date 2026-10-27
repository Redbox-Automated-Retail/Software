using System;
using System.Threading.Tasks;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Analytics;
using KioskClientService.API.Services.Analytics.KioskDataService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API;

public class ApplicationWebHostService : WebHostService
{
	private readonly IWebHost _host;

	private readonly ILogger<ApplicationWebHostService> _logger;

	private IAnalyticsService _analytics;

	private DateTime _onStartingTime;

	public ApplicationWebHostService(IWebHost host, ILogger<ApplicationWebHostService> logger)
		: base(host)
	{
		_host = host;
		_logger = logger;
	}

	protected override void OnStarting(string[] args)
	{
		_logger?.LogInfoWithSource("service starting", "OnStarting");
		_onStartingTime = DateTime.Now;
		base.OnStarting(args);
	}

	protected override void OnStarted()
	{
		_logger?.LogInfoWithSource("service started", "OnStarted");
		try
		{
			_analytics = _host.Services.GetService<IAnalyticsService>();
			_analytics?.PurgeOldAnalytics(TimeSpan.FromDays(30.0));
			_analytics?.ServiceEvent(KioskClientEvent.ServiceStarting, _onStartingTime);
			_analytics?.ServiceEvent(KioskClientEvent.ServiceStarted);
			Task.Run(async delegate
			{
				await SendSavedServiceEvents();
			});
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"an unhandled exception occurred. {arg}", "OnStarted");
		}
		base.OnStarted();
	}

	protected override void OnStopping()
	{
		_logger?.LogInfoWithSource("service stopping", "OnStopping");
		try
		{
			_analytics.SaveServiceEvent(new ServiceEvent
			{
				EventType = KioskClientEvent.ServiceStopping
			});
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"an unhandled exception occurred. {arg}", "OnStopping");
		}
		base.OnStopping();
	}

	protected override void OnStopped()
	{
		_logger?.LogInfoWithSource("service stopped", "OnStopped");
		try
		{
			_analytics.SaveServiceEvent(new ServiceEvent
			{
				EventType = KioskClientEvent.ServiceStopped
			});
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"an unhandled exception occurred. {arg}", "OnStopped");
		}
		base.OnStopped();
	}

	private async Task SendSavedServiceEvents()
	{
		ServiceEvent serviceEvent = _analytics.LoadServiceEvent(KioskClientEvent.ServiceStopping);
		if (serviceEvent != null)
		{
			await _analytics.ServiceEvent(serviceEvent.EventType, serviceEvent.EventTime);
		}
		ServiceEvent serviceEvent2 = _analytics.LoadServiceEvent(KioskClientEvent.ServiceStopped);
		if (serviceEvent2 != null)
		{
			await _analytics.ServiceEvent(serviceEvent2.EventType, serviceEvent2.EventTime);
		}
		_analytics.PurgeServiceEvents();
	}
}
