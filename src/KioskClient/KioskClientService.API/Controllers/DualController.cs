using System.Threading.Tasks;
using KioskClientService.API.Services.Dual;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/dual")]
[ApiController]
public class DualController : ControllerBase
{
	private readonly ILogger<DualController> _logger;

	private readonly IDualService _service;

	public DualController(ILogger<DualController> logger, IDualService service)
	{
		_logger = logger;
		_service = service;
	}

	[Route("refresh")]
	[HttpPost]
	[ProducesResponseType(typeof(RefreshResponse), 200)]
	[ProducesResponseType(typeof(BadRequestObjectResult), 400)]
	[ProducesResponseType(typeof(RefreshResponse), 500)]
	public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshPayload payload)
	{
		_logger.LogInfoWithSource("refresh called", "Refresh");
		if (base.ModelState.IsValid)
		{
			return (await _service.Refresh(payload.DualKioskId)).ToObjectResult();
		}
		_logger.LogErrorWithSource("bad request to refresh, errors: " + string.Join(',', base.ModelState.Values), "Refresh");
		return BadRequest(base.ModelState);
	}

	[Route("updatetitles")]
	[HttpPost]
	[ProducesResponseType(typeof(UpdateTitlesResponse), 200)]
	[ProducesResponseType(typeof(UpdateTitlesResponse), 500)]
	public async Task<ActionResult<UpdateTitlesResponse>> UpdateTitles([FromBody] UpdateTitlesPayload payload)
	{
		if (base.ModelState.IsValid)
		{
			return (await _service.UpdateTitles(payload.DualKioskId, payload.TitleIds)).ToObjectResult();
		}
		_logger.LogErrorWithSource("bad request to update titles, errors: " + string.Join(',', base.ModelState.Values), "UpdateTitles");
		return BadRequest(base.ModelState);
	}

	[Route("instockinother")]
	[HttpPost]
	[ProducesResponseType(typeof(InStockInOtherResponse), 200)]
	[ProducesResponseType(typeof(BadRequestObjectResult), 400)]
	[ProducesResponseType(typeof(InStockInOtherResponse), 500)]
	public async Task<ActionResult<InStockInOtherResponse>> InStockInOther([FromBody] InStockInOtherPayload payload)
	{
		if (base.ModelState.IsValid)
		{
			return (await _service.InStockInOther(payload.TitleIds)).ToObjectResult();
		}
		_logger.LogErrorWithSource("bad request to in stock in other, errors: " + string.Join(',', base.ModelState.Values), "InStockInOther");
		return BadRequest(base.ModelState);
	}
}
