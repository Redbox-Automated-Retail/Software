using System.Threading.Tasks;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Installer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/installer")]
[ApiController]
public class InstallerController : ControllerBase
{
	private readonly ILogger<InstallerController> _logger;

	private readonly IInstallerService _installerService;

	public InstallerController(ILogger<InstallerController> logger, IInstallerService installerService)
	{
		_logger = logger;
		_installerService = installerService;
	}

	[Route("getopsmarket")]
	[HttpGet]
	[ProducesResponseType(typeof(InstallerModels.OpsMarketResponse), 200)]
	public async Task<ActionResult<InstallerModels.OpsMarketResponse>> GetOpsMarket(int kioskid)
	{
		_logger?.LogInfoWithSource("InstallerController.GetOpsMarket called.", "GetOpsMarket");
		return new ObjectResult(await _installerService.GetOpsMarket(kioskid));
	}

	[Route("getpendingbanners")]
	[HttpGet]
	[ProducesResponseType(typeof(InstallerModels.PendingBannersResponse), 200)]
	public async Task<ActionResult<InstallerModels.PendingBannersResponse>> GetPendingBanners(int stateid)
	{
		_logger?.LogInfoWithSource("InstallerController.GetPendingBanners called.", "GetPendingBanners");
		return new ObjectResult(await _installerService.GetPendingBanners(stateid));
	}

	[Route("getpendingkiosks")]
	[HttpGet]
	[ProducesResponseType(typeof(InstallerModels.PendingKiosksResponse), 200)]
	public async Task<ActionResult<InstallerModels.PendingKiosksResponse>> GetPendingKiosks(short stateid, int bannerId)
	{
		_logger?.LogInfoWithSource("InstallerController.GetPendingKiosks called.", "GetPendingKiosks");
		return new ObjectResult(await _installerService.GetPendingKiosks(stateid, bannerId));
	}

	[Route("getpendingstates")]
	[HttpGet]
	[ProducesResponseType(typeof(InstallerModels.PendingStatesResponse), 200)]
	public async Task<ActionResult<InstallerModels.PendingStatesResponse>> GetPendingStates()
	{
		_logger?.LogInfoWithSource("InstallerController.GetPendingStates called.", "GetPendingStates");
		return new ObjectResult(await _installerService.GetPendingStates());
	}
}
