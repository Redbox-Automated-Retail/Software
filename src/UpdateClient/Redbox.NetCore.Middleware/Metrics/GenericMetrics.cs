using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;

namespace Redbox.NetCore.Middleware.Metrics;

public class GenericMetrics : ICommonMetrics
{
	private readonly IMetrics _metrics;

	private readonly ILogger<GenericMetrics> _logger;

	public const string MetricTypesKeyName = "MetricType";

	public const string ValueKeyName = "Value";

	public const string NameKeyName = "Name";

	public GenericMetrics(IMetrics metrics, ILogger<GenericMetrics> logger)
	{
		_metrics = metrics;
		_logger = logger;
	}

	public async Task CaptureMetrics(Dictionary<string, string> metricTagCollection, long elapsedMilliseconds)
	{
		await Task.Run(delegate
		{
			if (_metrics == null)
			{
				_logger.LogDebugWithSource("No implementation of IMetrics found. Skipping capture.", "CaptureMetrics", "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
				return;
			}
			try
			{
				(bool, MetricType) metricTypeFromMetricTags = GetMetricTypeFromMetricTags(metricTagCollection);
				if (metricTypeFromMetricTags.Item1)
				{
					switch (metricTypeFromMetricTags.Item2)
					{
					case MetricType.Gauge:
						CaptureGaugeMetric(metricTagCollection);
						break;
					case MetricType.Counter:
						CaptureCounterMetric(metricTagCollection);
						break;
					case MetricType.Histogram:
						CaptureHistogramMetric(metricTagCollection, elapsedMilliseconds);
						break;
					default:
						_logger.LogErrorWithSource(null, $"Unsupported MetricType {metricTypeFromMetricTags.Item2}", "CaptureMetrics", "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
						break;
					}
				}
				else
				{
					_logger.LogErrorWithSource(null, $"metricTagCollection missing tag {metricTypeFromMetricTags.Item2}", "CaptureMetrics", "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Unhandled exception while capturing metrics", "CaptureMetrics", "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
			}
		});
	}

	private (bool Found, MetricType MetricType) GetMetricTypeFromMetricTags(Dictionary<string, string> metricTagCollection)
	{
		bool item = false;
		MetricType item2 = MetricType.Gauge;
		if (metricTagCollection != null && metricTagCollection.TryGetValue("MetricType", out var value))
		{
			metricTagCollection.Remove("MetricType");
			if (Enum.TryParse<MetricType>(value, out var result))
			{
				item2 = result;
				item = true;
			}
		}
		return (Found: item, MetricType: item2);
	}

	private void CaptureGaugeMetric(Dictionary<string, string> metricTagCollection)
	{
		double num = 0.0;
		if (metricTagCollection.TryGetValue("Value", out var value))
		{
			if (double.TryParse(value, out var result))
			{
				num = result;
			}
			metricTagCollection.Remove("Value");
		}
		MetricTags metricTags = new MetricTags(metricTagCollection.Keys.ToArray(), metricTagCollection.Values.ToArray());
		_logger.LogInfoWithSource($"{metricTags.ToJson()} value: {num}", "CaptureGaugeMetric", "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
		_metrics.Measure.Gauge.SetValue(MetricsRegistry.GenericGauge, metricTags, num);
	}

	private void CaptureCounterMetric(Dictionary<string, string> metricTagCollection)
	{
		MetricTags metricTags = new MetricTags(metricTagCollection.Keys.ToArray(), metricTagCollection.Values.ToArray());
		_logger.LogInfoWithSource("Incremented counter on " + metricTags.ToJson(), "CaptureCounterMetric", "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
		_metrics.Measure.Counter.Increment(MetricsRegistry.GenericCounter, metricTags);
	}

	private void CaptureHistogramMetric(Dictionary<string, string> metricTagCollection, long elapsedMilliseconds)
	{
		MetricTags metricTags = new MetricTags(metricTagCollection.Keys.ToArray(), metricTagCollection.Values.ToArray());
		_logger.LogInfoWithSource($"{metricTags.ToJson()} took {elapsedMilliseconds} ms", "CaptureHistogramMetric", "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
		_metrics.Measure.Histogram.Update(MetricsRegistry.GenericHistogram, metricTags, elapsedMilliseconds);
	}
}
