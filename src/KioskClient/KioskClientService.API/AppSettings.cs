namespace KioskClientService.API;

public class AppSettings
{
	public string DataFilePath { get; set; }

	public string ServiceUrl { get; set; }

	public string UpdateManagerAddress { get; set; }

	public int UpdateManagerPort { get; set; }

	public int HttpQueuePollingInterval { get; set; }

	public int HttpQueueMaxPollingInterval { get; set; }

	public int HttpQueueDepthPollingIntervalMinutes { get; set; }

	public int HttpQueueDepthToAlert { get; set; }

	public int HttpQueueAlertRetryThreshold { get; set; }

	public string PlanogramFolder { get; set; }

	public string DefaultProxyServiceUrl { get; set; }

	public string UpdateClientServiceUrl { get; set; }

	public ProfileDataSettings ProfileDataSettings { get; set; }
}
