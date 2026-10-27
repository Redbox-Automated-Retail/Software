using System.Net;

namespace KioskClientService.API.Services.Analytics;

public class HttpAnalyticsRequest : BaseAnalyticsRequest
{
	public HttpStatusCode StatusCode { get; set; }

	public int? ResponseTime { get; set; }

	public int? ResponseLength { get; set; }

	public bool Success { get; set; }
}
