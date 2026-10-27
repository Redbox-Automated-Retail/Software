using System.Threading.Tasks;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Authentication;
using KioskClientService.API.Services.Kiosk;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/kiosk")]
[ApiController]
public class KioskController : ControllerBase
{
	private readonly ILogger<InstallerController> _logger;

	private readonly IAuthenticationService _authenticationService;

	private readonly IKioskService _kioskService;

	public KioskController(ILogger<InstallerController> logger, IAuthenticationService authenticationService, IKioskService kioskService)
	{
		_logger = logger;
		_authenticationService = authenticationService;
		_kioskService = kioskService;
	}

	[Route("authenticate")]
	[HttpPost]
	[ProducesResponseType(typeof(KioskModels.KioskAuthenticateResponse), 200)]
	public async Task<ActionResult<KioskModels.KioskAuthenticateResponse>> Authenticate(KioskModels.KioskAuthenticateRequest request)
	{
		_logger?.LogInfoWithSource("KioskController.Authenticate called.", "Authenticate");
		return new ObjectResult(await _authenticationService.Authenticate(request));
	}

	[Route("nearby")]
	[HttpPost]
	[ProducesResponseType(typeof(NearbyKiosksResponse), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(401)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<NearbyKiosksResponse>> NearbyKiosks(NearbyKiosksRequest request)
	{
		return (await _kioskService.GetNearbyKiosks(request)).ToObjectResult();
	}
}
