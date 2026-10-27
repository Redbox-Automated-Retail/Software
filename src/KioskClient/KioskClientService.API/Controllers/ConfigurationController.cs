using System.Threading.Tasks;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Session;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[Route("api/configuration")]
[ApiController]
public class ConfigurationController : ControllerBase
{
	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly ISessionService _sessionService;

	public ConfigurationController(IOptionsSnapshotKioskConfiguration kioskConfiguration, ISessionService sessionService)
	{
		_kioskConfiguration = kioskConfiguration;
		_sessionService = sessionService;
	}

	[HttpGet("current")]
	[ProducesResponseType(typeof(KioskConfigurationResponse), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<IKioskConfiguration>> GetCurrentKioskConfiguration()
	{
		KioskConfigurationResponse response = null;
		await Task.Run(delegate
		{
			response = new KioskConfigurationResponse
			{
				KioskConfiguration = _kioskConfiguration
			};
		});
		return response.ToObjectResult();
	}

	[HttpGet("kiosksession")]
	[ProducesResponseType(typeof(KioskConfigurationResponse), 200)]
	[ProducesResponseType(typeof(KioskConfigurationResponse), 404)]
	[ProducesResponseType(500)]
	public ActionResult<IKioskConfiguration> GetKioskSessionKioskConfiguration()
	{
		KioskConfigurationResponse currentKioskSessionConfiguration = _sessionService.GetCurrentKioskSessionConfiguration();
		return currentKioskSessionConfiguration.ToObjectResult();
	}
}
