namespace Redbox.NetCore.Middleware.Metrics;

internal class ApplicationMetricsSettings
{
	public bool DisableMetrics { get; set; }

	public string MetricsAWSRegion { get; set; }

	public int BufferSize { get; set; } = 1;
}
