using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Analytics.Vistar.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/analytics")]
[ApiController]
public class AnalyticsController : ControllerBase
{
	private readonly ILogger<AnalyticsController> _logger;

	private readonly IAnalyticsService _analyticsService;

	public AnalyticsController(ILogger<AnalyticsController> logger, IAnalyticsService analyticsService)
	{
		_logger = logger;
		_analyticsService = analyticsService;
	}

	[HttpPost]
	[Route("getadtimeoutevent")]
	public async Task<ActionResult<GetAdTimeoutEvent>> GetAdTimeoutEvent(GetAdTimeoutEvent timeoutEvent)
	{
		_logger?.LogInfoWithSource(timeoutEvent.ToJson(), "GetAdTimeoutEvent");
		await _analyticsService.GetAdTimeoutEvent(timeoutEvent);
		return new OkObjectResult(new GetAdTimeoutEventResponse());
	}
}
