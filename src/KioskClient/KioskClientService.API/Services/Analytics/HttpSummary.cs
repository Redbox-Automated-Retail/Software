namespace KioskClientService.API.Services.Analytics;

public class HttpSummary : Summary
{
	public double? MedianResponseTime { get; set; }

	public int? MaxResponseTime { get; set; }

	public double? SuccessRate { get; set; }
}
