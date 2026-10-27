using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Campaigns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/campaign")]
[ApiController]
public class CampaignController : ControllerBase
{
	private readonly ICampaignService _campaignService;

	private readonly ILogger<CampaignController> _logger;

	public CampaignController(ILogger<CampaignController> logger, ICampaignService cs)
	{
		_campaignService = cs;
		_logger = logger;
	}

	[HttpGet]
	[Route("startscreen")]
	[ProducesResponseType(typeof(GetStartScreenResponse), 200)]
	[ProducesResponseType(typeof(ApiBaseResponse), 400)]
	[ProducesResponseType(typeof(ApiBaseResponse), 404)]
	[ProducesResponseType(typeof(ApiBaseResponse), 500)]
	public async Task<ActionResult<GetStartScreenResponse>> GetStartScreen()
	{
		return (await _campaignService.GetStartScreen()).ToObjectResult();
	}

	[HttpGet]
	[Route("incart")]
	[ProducesResponseType(typeof(GetInCartResponse), 200)]
	[ProducesResponseType(typeof(ApiBaseResponse), 404)]
	[ProducesResponseType(typeof(ApiBaseResponse), 500)]
	public async Task<ActionResult<GetInCartResponse>> GetInCart()
	{
		return (await _campaignService.GetInCart()).ToObjectResult();
	}

	[HttpPost("refresh")]
	[ProducesResponseType(typeof(ApiBaseResponse), 200)]
	public async Task<ActionResult<ApiBaseResponse>> Refresh()
	{
		return (await _campaignService.Refresh()).ToObjectResult();
	}

	[HttpPost("refreshifneeded")]
	[ProducesResponseType(typeof(ApiBaseResponse), 200)]
	public async Task<ActionResult<ApiBaseResponse>> RefreshAllIfNeeded()
	{
		return (await _campaignService.RefreshIfNeeded()).ToObjectResult();
	}
}
