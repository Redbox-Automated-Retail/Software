using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Abstractions;

namespace Redbox.NetCore.Middleware.Http;

public class ClientMetricsHandler : DelegatingHandler
{
	private readonly IApplicationMetrics _metrics;

	private readonly ILogger<ClientMetricsHandler> _logger;

	private const string _metricHeader = "x-redbox-metrics";

	public ClientMetricsHandler(IApplicationMetrics metrics, ILoggerFactory loggerFactory)
	{
		_metrics = metrics;
		_logger = loggerFactory.CreateLogger<ClientMetricsHandler>();
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Stopwatch requestTimer = Stopwatch.StartNew();
		HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
		requestTimer.Stop();
		string metricName = request.RequestUri.GetLeftPart(UriPartial.Authority);
		if (request.Headers.TryGetValues("x-redbox-metrics", out var values))
		{
			string text = values.FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(text))
			{
				metricName = text;
				_logger.LogInformation("Setting metric name to '" + metricName + "'");
			}
			else
			{
				_logger.LogInformation("Metrics header not found. Setting metric name to '" + metricName + "'");
			}
		}
		Task.Run(() => _metrics.CaptureUrlExecutionAsync(metricName, requestTimer.ElapsedMilliseconds, response.StatusCode));
		return response;
	}
}
