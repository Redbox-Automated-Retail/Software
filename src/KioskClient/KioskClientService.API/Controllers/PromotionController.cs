using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Promotion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/promotion")]
[ApiController]
public class PromotionController : ControllerBase
{
	private readonly ILogger<PromotionController> _logger;

	private readonly IPromotionService _promotionService;

	public PromotionController(ILogger<PromotionController> logger, IPromotionService promotionService)
	{
		_logger = logger;
		_promotionService = promotionService;
	}

	[Route("promocodevalidation")]
	[HttpPost]
	[ProducesResponseType(typeof(PromotionCodeValidationResponse), 200)]
	[ProducesResponseType(typeof(PromotionCodeValidationResponse), 400)]
	[ProducesResponseType(typeof(PromotionCodeValidationResponse), 422)]
	[ProducesResponseType(typeof(PromotionCodeValidationResponse), 500)]
	public async Task<ActionResult<PromotionCodeValidationResponse>> PromoCodeValidation(PromotionCodeValidationPayload payload)
	{
		_logger.LogInfoWithSource("PromoCodeValidation called.", "PromoCodeValidation");
		if (base.ModelState.IsValid)
		{
			APIResponse<PromotionCodeValidationResponse> aPIResponse = await _promotionService.ValidatePromotionCode(payload);
			PromotionCodeValidationResponse value;
			if (aPIResponse == null || aPIResponse.Response == null)
			{
				_logger.LogErrorWithSource("Null response returned from api call!", "PromoCodeValidation");
				value = new PromotionCodeValidationResponse
				{
					Error = "(U001)"
				};
			}
			else
			{
				value = aPIResponse.Response;
			}
			return new ObjectResult(value)
			{
				StatusCode = (int)aPIResponse.StatusCode
			};
		}
		return BadRequest(base.ModelState);
	}
}
