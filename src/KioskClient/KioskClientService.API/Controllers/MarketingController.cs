using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.HttpQueue;
using KioskClientService.API.Services.Marketing;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/marketing")]
[ApiController]
public class MarketingController : ControllerBase
{
	private readonly ILogger<MarketingController> _logger;

	private readonly IProxyApi _proxyApi;

	private readonly IMarketingService _marketingService;

	private readonly IHttpQueueService _httpQueueService;

	public MarketingController(ILogger<MarketingController> logger, IProxyApi proxyApi, IHttpQueueService httpQueueService, IMarketingService marketingService)
	{
		_logger = logger;
		_proxyApi = proxyApi;
		_httpQueueService = httpQueueService;
		_marketingService = marketingService;
	}

	[Route("emailconfirmation")]
	[HttpPost]
	[ProducesResponseType(typeof(EmailConfirmationResponse), 200)]
	public async Task<ActionResult<EmailConfirmationResponse>> EmailConfirmation(EmailConfirmationPayload payload)
	{
		_logger?.LogInfoWithSource("MarketingController.EmailConfirmation called.", "EmailConfirmation");
		return (await _marketingService.EmailConfirmation(payload)).ToObjectResult();
	}

	[Route("kioskoptin")]
	[HttpPost]
	[ProducesResponseType(typeof(KioskOptInResponse), 200)]
	public async Task<ActionResult<KioskOptInResponse>> KioskOptIn(KioskOptInPayload payload)
	{
		_logger?.LogInfoWithSource("MarketingController.KioskOptIn called.", "KioskOptIn");
		APIResponse<KioskOptInResponse> aPIResponse = await _proxyApi.KioskOptIn(payload);
		aPIResponse.CheckAndReplay(payload.MessageId, base.ControllerContext.HttpContext.Request, _httpQueueService);
		KioskOptInResponse kioskOptInResponse = (aPIResponse.IsSuccessStatusCode ? aPIResponse.Response : ((aPIResponse.Response != null) ? aPIResponse.Response : new KioskOptInResponse
		{
			MessageId = payload.MessageId,
			StatusCode = (int)aPIResponse.StatusCode
		}));
		return kioskOptInResponse.ToObjectResult();
	}
}
