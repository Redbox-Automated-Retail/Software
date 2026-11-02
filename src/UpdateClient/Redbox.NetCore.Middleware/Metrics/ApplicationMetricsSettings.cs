namespace Redbox.NetCore.Middleware.Metrics;

internal class ApplicationMetricsSettings
{
	public bool DisableMetrics { get; set; }

	public string MetricsAWSRegion { get; set; } = null!; // bound from the MetricsAppSettings configuration section

	public int BufferSize { get; set; } = 1;
}
