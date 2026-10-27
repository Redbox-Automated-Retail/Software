using System;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.App;
using Microsoft.Extensions.Options;
using Serilog;

namespace KioskClientService.API.Services.HttpQueue;

internal class HttpQueueDepthService : HttpQueueAlertingService
{
	private readonly TimeSpan _pollingInMinutes;

	private readonly int _depthToAlert;

	public HttpQueueDepthService(IOptions<AppSettings> options, IStoreService store, IHttpService httpService)
		: base(options, store, httpService)
	{
		_depthToAlert = options.Value.HttpQueueDepthToAlert;
		_pollingInMinutes = new TimeSpan(0, options.Value.HttpQueueDepthPollingIntervalMinutes, 0);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				int num = HttpQueueInternals.Depth();
				if (num > _depthToAlert)
				{
					Log.Information($"HttpQueueDepthService.ExecuteAsync() - queue depth is over {_depthToAlert}, sending alert");
					await SendAlert("HttpQueueDepth", $"KCS http queue depth for kiosk: {_kioskId} is over threshold of {_depthToAlert}");
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception, "HttpQueueDepthService.ExecuteAsync() - exception occured!");
			}
			await Task.Delay(_pollingInMinutes, stoppingToken);
		}
	}
}
