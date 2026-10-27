using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using App.Metrics;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;
using Serilog;

namespace Redbox.NetCore.Middleware.Metrics;

internal class ApplicationMetrics : IApplicationMetrics
{
	private const string SpecificApiDimension = "SpecificApi";

	private const string ExternalApiDimension = "ExternalApi";

	private const string Status4xx = "4xxError";

	private const string Status5xx = "5xxError";

	private readonly RegionEndpoint _regionEndpoint;

	private readonly bool _isDisabled;

	private readonly string _namespace;

	private volatile List<MetricDatum> _currentMetrics = new List<MetricDatum>();

	private readonly int _bufferSize;

	private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

	public ApplicationMetrics(IMetrics metrics, IOptions<ApplicationMetricsSettings> metricsSettings)
	{
		_isDisabled = metricsSettings.Value.DisableMetrics;
		_regionEndpoint = RegionEndpoint.GetBySystemName(metricsSettings.Value.MetricsAWSRegion);
		_bufferSize = Math.Min(metricsSettings.Value.BufferSize, 20);
		_namespace = ((metrics is IMetricsRoot) ? ((IMetricsRoot)metrics).Options.DefaultContextLabel : "default");
	}

	public async Task CaptureEndpointExecutionAsync(string controllerName, string actionName, long responseMilliseconds, HttpStatusCode statusCode)
	{
		List<Dimension> dimensions = Dimensions("SpecificApi", controllerName + "." + actionName);
		await CaptureAsync(responseMilliseconds, statusCode, dimensions);
	}

	public async Task CaptureUrlExecutionAsync(string url, long responseMilliseconds, HttpStatusCode statusCode)
	{
		List<Dimension> dimensions = Dimensions("ExternalApi", url);
		await CaptureAsync(responseMilliseconds, statusCode, dimensions);
	}

	private async Task CaptureAsync(long responseMilliseconds, HttpStatusCode statusCode, List<Dimension> dimensions, List<MetricDatum> data = null)
	{
		await _semaphore.WaitAsync(5000);
		try
		{
			if (!_isDisabled)
			{
				List<MetricDatum> data2 = Metrics(dimensions, responseMilliseconds, (int)statusCode);
				await SendAsync(data2);
			}
		}
		catch (Exception exception)
		{
			Log.Logger.Error(exception, "ApplicationMetrics -> Error while capturing metrics. Clearing current metrics.");
			_currentMetrics = new List<MetricDatum>();
		}
		finally
		{
			_semaphore.Release();
		}
	}

	private async Task SendAsync(List<MetricDatum> data)
	{
		try
		{
			if (_currentMetrics.Count > _bufferSize)
			{
				_currentMetrics = _currentMetrics.TakeLast(_currentMetrics.Count - _bufferSize).ToList();
			}
			_currentMetrics.AddRange(data);
			if (_currentMetrics.Count >= _bufferSize)
			{
				Log.Logger.Information($"ApplicationMetrics -> Buffer limit of {_bufferSize} reached. Sending first {_bufferSize} metrics of {_currentMetrics?.Count ?? 0}");
				using AmazonCloudWatchClient client = new AmazonCloudWatchClient(_regionEndpoint);
				List<MetricDatum> metricsToSend = _currentMetrics.Take(_bufferSize).ToList();
				PutMetricDataResponse putMetricDataResponse = await client.PutMetricDataAsync(new PutMetricDataRequest
				{
					MetricData = _currentMetrics.Take(_bufferSize).ToList(),
					Namespace = _namespace
				});
				Log.Logger.Debug($"ApplicationMetrics -> PutMetricDataAsync -> StatusCode: {putMetricDataResponse.HttpStatusCode}, Data Sent: {metricsToSend.ToJson()}");
			}
		}
		catch (Exception exception)
		{
			Log.Logger.Error(exception, "ApplicationMetrics -> Error while sending metrics -> " + ((_currentMetrics == null) ? "null" : _currentMetrics?.ToJson()));
		}
		finally
		{
			if (_currentMetrics.Count >= _bufferSize)
			{
				_currentMetrics = _currentMetrics.TakeLast(_currentMetrics.Count - _bufferSize).ToList();
				Log.Logger.Debug($"ApplicationMetrics -> {_bufferSize} Metrics flushed. {_currentMetrics.Count} remaining -> {_currentMetrics.ToJson()}");
			}
		}
	}

	private List<MetricDatum> Metrics(List<Dimension> dimensions, long responseTime, int statusCode)
	{
		DateTime utcNow = DateTime.UtcNow;
		List<MetricDatum> list = new List<MetricDatum>
		{
			new MetricDatum
			{
				MetricName = "ResponseTime",
				Dimensions = dimensions,
				Unit = StandardUnit.Milliseconds,
				TimestampUtc = utcNow,
				Value = responseTime
			},
			new MetricDatum
			{
				MetricName = "CallCount",
				Dimensions = dimensions,
				Unit = StandardUnit.Count,
				Value = 1.0,
				TimestampUtc = utcNow
			}
		};
		if (statusCode >= 400 && statusCode < 500)
		{
			list.Add(HttpStatusDatum("4xxError", dimensions));
		}
		if (statusCode >= 500 && statusCode < 600)
		{
			list.Add(HttpStatusDatum("5xxError", dimensions));
		}
		return list;
	}

	private static List<Dimension> Dimensions(string dimensionName, string dimensionValue)
	{
		return new List<Dimension>
		{
			new Dimension
			{
				Name = dimensionName,
				Value = dimensionValue
			}
		};
	}

	private static MetricDatum HttpStatusDatum(string statusName, List<Dimension> dimensions)
	{
		return new MetricDatum
		{
			MetricName = statusName,
			Dimensions = dimensions,
			Unit = StandardUnit.Count,
			Value = 1.0
		};
	}
}
