using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/session")]
[ApiController]
public class SessionController : ControllerBase
{
	private readonly ILogger _logger;

	private readonly ISessionService _sessionService;

	public SessionController(ILogger<AdsController> logger, ISessionService sessionService)
	{
		_logger = logger;
		_sessionService = sessionService;
	}

	[ProducesResponseType(typeof(SessionResponse), 200)]
	[HttpPost]
	public ActionResult<SessionResponse> CreateKioskSession(CreateKioskSessionRequest createKioskSessionRequest)
	{
		_sessionService?.CreateKioskSession(createKioskSessionRequest.KioskSessionId);
		return new OkObjectResult(new SessionResponse());
	}

	[HttpPost("start")]
	public ActionResult<SessionResponse> StartUserInteraction(SessionType sessionType)
	{
		_sessionService?.StartUserInteraction(sessionType);
		return new OkObjectResult(new SessionResponse());
	}

	[HttpPost("end")]
	public ActionResult<SessionResponse> EndSession()
	{
		_sessionService?.End();
		return new OkObjectResult(new SessionResponse());
	}
}
