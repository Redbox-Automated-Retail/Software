using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;
using KioskClientService.API.Services.Loyalty;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/loyalty")]
[ApiController]
public class LoyaltyController : ControllerBase
{
	private readonly ILogger<LoyaltyController> _logger;

	private readonly ILoyaltyService _service;

	private readonly IHttpQueueService _queue;

	public LoyaltyController(ILogger<LoyaltyController> logger, ILoyaltyService service, IHttpQueueService httpQueueService)
	{
		_logger = logger;
		_service = service;
		_queue = httpQueueService;
	}

	[Route("estimateaccrual")]
	[HttpPost]
	[ProducesResponseType(typeof(ClientLoyaltyResponse), 200)]
	[ProducesResponseType(typeof(BadRequestObjectResult), 400)]
	[ProducesResponseType(typeof(ClientLoyaltyResponse), 500)]
	public async Task<ActionResult<ClientLoyaltyResponse>> EstimateAccrual(CartPayload payload)
	{
		ClientLoyaltyResponse result = new ClientLoyaltyResponse(payload.MessageId, payload.KioskId);
		_logger.LogInfoWithSource("EstimateAccrual called.", "EstimateAccrual");
		if (base.ModelState.IsValid)
		{
			APIResponse<ServerLoyaltyResponse> aPIResponse = await _service.EstimateAccrualAsync(payload);
			result.StatusCode = aPIResponse.StatusCode;
			if (aPIResponse.IsSuccessStatusCode)
			{
				if (aPIResponse.Response == null)
				{
					result.SetError("(EA000)", "Estimate accrual returned success with no content");
				}
				else
				{
					result.FromServerResponse(aPIResponse.Response);
				}
			}
			else if (aPIResponse.Response == null || aPIResponse.Response.Error == null)
			{
				result.SetError("(EA003)", $"Estimate call returned status code: {aPIResponse.StatusCode}, error: {aPIResponse.Response?.Error}");
			}
			else
			{
				result.SetError(aPIResponse.Response.Error switch
				{
					"Account not found" => "(EA001)", 
					"Loyalty error" => "(EA002)", 
					_ => "(EA003)", 
				}, aPIResponse.Response.Error);
			}
			return result.ToObjectResult();
		}
		return BadRequest(base.ModelState);
	}

	[Route("estimateredemption")]
	[HttpPost]
	[ProducesResponseType(typeof(ClientLoyaltyResponse), 200)]
	[ProducesResponseType(typeof(BadRequestObjectResult), 400)]
	[ProducesResponseType(typeof(ClientLoyaltyResponse), 500)]
	public async Task<ActionResult<ClientLoyaltyResponse>> EstimateRedemption(CartPayload payload)
	{
		ClientLoyaltyResponse result = new ClientLoyaltyResponse(payload.MessageId, payload.KioskId);
		_logger.LogInfoWithSource("EstimateRedemption called.", "EstimateRedemption");
		if (base.ModelState.IsValid)
		{
			APIResponse<ServerLoyaltyResponse> aPIResponse = await _service.EstimateRedemptionAsync(payload);
			result.StatusCode = aPIResponse.StatusCode;
			if (aPIResponse.IsSuccessStatusCode)
			{
				if (aPIResponse.Response == null)
				{
					result.SetError("(ER000)", "Estimate redemption returned success with no content");
				}
				else
				{
					result.FromServerResponse(aPIResponse.Response);
				}
			}
			else if (aPIResponse.Response == null || aPIResponse.Response.Error == null)
			{
				result.SetError("(ER003)", $"Estimate call returned status code: {aPIResponse.StatusCode}, error: {aPIResponse.Response?.Error}");
			}
			else
			{
				result.SetError(aPIResponse.Response.Error switch
				{
					"Account not found" => "(ER001)", 
					"Loyalty error" => "(ER002)", 
					_ => "(ER003)", 
				}, aPIResponse.Response.Error);
			}
			return result.ToObjectResult();
		}
		return BadRequest(base.ModelState);
	}

	[Route("updateaccount")]
	[HttpPost]
	[ProducesResponseType(typeof(UpdateAccountResponse), 200)]
	[ProducesResponseType(typeof(BadRequestObjectResult), 400)]
	[ProducesResponseType(typeof(UpdateAccountResponse), 500)]
	public async Task<ActionResult<UpdateAccountResponse>> UpdateAccount(UpdateAccountPayload payload)
	{
		if (base.ModelState.IsValid)
		{
			UpdateAccountResponse updateAccountResponse = await _service.UpdateAccountAsync(payload);
			if (payload.Replay)
			{
				updateAccountResponse.MessageId = payload.MessageId;
				updateAccountResponse.CheckAndReplay(_logger, payload.MessageId, base.ControllerContext.HttpContext.Request, _queue);
			}
			return updateAccountResponse.ToObjectResult();
		}
		return BadRequest(base.ModelState);
	}
}
