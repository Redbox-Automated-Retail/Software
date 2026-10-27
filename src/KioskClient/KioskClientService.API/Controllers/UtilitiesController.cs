using System.Threading.Tasks;
using KioskClientService.API.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/utilities")]
[ApiController]
public class UtilitiesController
{
	private readonly ILogger<UtilitiesController> _logger;

	private readonly IQRCodeUtilitiesService _qrCodeUtilitiesService;

	public UtilitiesController(ILogger<UtilitiesController> logger, IQRCodeUtilitiesService qrCodeUtilitiesService)
	{
		_logger = logger;
		_qrCodeUtilitiesService = qrCodeUtilitiesService;
	}

	[HttpGet]
	[Route("getqrcode")]
	public async Task<ActionResult<GetQRCodeResponse>> GetQRCodeAsync(string valueToEncode)
	{
		_logger?.LogInformation("UtilitiesController.GetQRCodeAsync: " + valueToEncode);
		return new OkObjectResult(await (_qrCodeUtilitiesService?.GetQRCodeResponseAsync(valueToEncode)));
	}
}
