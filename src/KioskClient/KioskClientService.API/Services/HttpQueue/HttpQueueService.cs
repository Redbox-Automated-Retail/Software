using System;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.App;
using Microsoft.Extensions.Options;
using Serilog;

namespace KioskClientService.API.Services.HttpQueue;

internal class HttpQueueService : HttpQueueAlertingService
{
	private readonly int _minPollingInterval;

	private readonly int _maxPollingInterval;

	private int _pollingInterval;

	private readonly IHttpService _httpService;

	private readonly int _retryThreshold;

	public HttpQueueService(IOptions<AppSettings> options, IStoreService store, IHttpService httpService)
		: base(options, store, httpService)
	{
		_pollingInterval = (_minPollingInterval = options.Value.HttpQueuePollingInterval);
		_maxPollingInterval = options.Value.HttpQueueMaxPollingInterval;
		_retryThreshold = options.Value.HttpQueueAlertRetryThreshold;
		_httpService = httpService;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				if (HttpQueueInternals.Dequeue(out var request))
				{
					if (request.Request != null)
					{
						await AlertRequestAtReplayThreshold(request);
						_httpService.Timeout = _pollingInterval - 1000;
						APIResponse<ApplicationResponse> aPIResponse = await _httpService.SendRequest<ApplicationResponse>(request.Request.ToHttpRequestMessage());
						if (aPIResponse.IsSuccessStatusCode)
						{
							_pollingInterval = Math.Max(_minPollingInterval, _pollingInterval / 2);
						}
						else
						{
							_pollingInterval = Math.Min(_maxPollingInterval, _pollingInterval * 2);
							Log.Information($"HttpQueueService.ExecuteAsync() - request {request.Id} failed with status code: {aPIResponse.StatusCode}, falling back polling interval to {_pollingInterval} seconds");
						}
					}
				}
				else
				{
					_pollingInterval = Math.Max(_minPollingInterval, _pollingInterval / 2);
				}
				request = null;
			}
			catch (Exception exception)
			{
				Log.Error(exception, "HttpQueueService.ExecuteAsync() - exception occured!");
			}
			await Task.Delay(_pollingInterval, stoppingToken);
		}
	}

	private async Task AlertRequestAtReplayThreshold(HttpQueueServiceRequest request)
	{
		Log.Information($"HttpQueueService.AlertRequestAtReplayThreshold() - request.Retries: {request.Retries}");
		if (request.Retries > 0 && request.Retries % _retryThreshold == 0)
		{
			string text = $"Request {request.Id} has been retried {request.Retries} times on kiosk {_kioskId}";
			Log.Information("HttpQueueService.AlertRequestAtReplayThreshold() - " + text + ", sending alert");
			await SendAlert("HttpQueueRetryThreshold", text);
		}
	}
}
