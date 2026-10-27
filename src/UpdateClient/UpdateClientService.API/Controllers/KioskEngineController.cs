using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.KioskEngine;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class KioskEngineController : ControllerBase
{
	private readonly IKioskEngineService _kioskEngineService;

	public KioskEngineController(IKioskEngineService kioskEngineService)
	{
		_kioskEngineService = kioskEngineService;
	}

	[HttpPost("shutdown")]
	[ProducesResponseType(typeof(PerformShutdownResponse), 200)]
	[ProducesResponseType(typeof(PerformShutdownResponse), 500)]
	public async Task<ActionResult<PerformShutdownResponse>> PerformShutdown(int timeoutMs, int attempts)
	{
		return (await _kioskEngineService.PerformShutdown(timeoutMs, attempts)).ToObjectResult();
	}

	[HttpGet("isrunning")]
	[ProducesResponseType(typeof(KioskEngineStatus), 200)]
	[ProducesResponseType(typeof(KioskEngineStatus), 500)]
	[ProducesResponseType(typeof(KioskEngineStatus), 503)]
	public async Task<ActionResult<KioskEngineStatus>> IsKioskEngineRunning()
	{
		return (await _kioskEngineService.GetStatus()).ToObjectResult();
	}
}
