using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Services.Planograms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlanogramController : ControllerBase
{
	private readonly IPlanogramService _planogramService;

	private readonly ILogger<PlanogramController> _logger;

	public PlanogramController(IPlanogramService planogramService, ILogger<PlanogramController> logger)
	{
		_planogramService = planogramService;
		_logger = logger;
	}

	[HttpGet]
	[ProducesResponseType(typeof(List<Planogram>), 200)]
	[ProducesResponseType(typeof(List<Planogram>), 404)]
	public ActionResult<List<Planogram>> GetActivePlanograms()
	{
		_logger?.LogInfoWithSource("GetActivePlanograms called.", "GetActivePlanograms");
		List<Planogram> activePlanograms = _planogramService.GetActivePlanograms();
		if (activePlanograms == null || activePlanograms.Count == 0)
		{
			return new NotFoundObjectResult(null);
		}
		return new OkObjectResult(activePlanograms);
	}

	[Route("selected")]
	[HttpGet]
	[ProducesResponseType(typeof(Planogram), 200)]
	[ProducesResponseType(typeof(Planogram), 404)]
	public async Task<ActionResult<Planogram>> GetSelectedPlanogram()
	{
		_logger?.LogInfoWithSource("GetSelected Planogram called", "GetSelectedPlanogram");
		Planogram planogram = await _planogramService.GetSelectedPlanogram();
		if (planogram == null)
		{
			return new NotFoundObjectResult(null);
		}
		return new OkObjectResult(planogram);
	}

	[Route("selected")]
	[HttpPost]
	[ProducesResponseType(typeof(SavePlanogramInfoResponse), 200)]
	[ProducesResponseType(typeof(SavePlanogramInfoResponse), 400)]
	[ProducesResponseType(typeof(SavePlanogramInfoResponse), 500)]
	public async Task<ActionResult<SavePlanogramInfoResponse>> SetSelectedPlanogram(string planogramName)
	{
		_logger?.LogInfoWithSource("SetSelected Planogram called", "SetSelectedPlanogram");
		return (await _planogramService.SaveSelectedPlanogram(planogramName)).ToObjectResult();
	}

	[Route("sync")]
	[HttpPost]
	[ProducesResponseType(typeof(SyncPlanogramsResponse), 200)]
	[ProducesResponseType(typeof(SyncPlanogramsResponse), 500)]
	public async Task<ActionResult<SyncPlanogramsResponse>> SyncPlanograms(bool forceOverwrite = false)
	{
		_logger?.LogInfoWithSource("Sync Planograms called", "SyncPlanograms");
		return (await _planogramService.SyncPlanograms(forceOverwrite)).ToObjectResult();
	}

	[Route("syncifneeded")]
	[HttpPost]
	[ProducesResponseType(typeof(SyncPlanogramsResponse), 200)]
	public async Task<ActionResult<SyncPlanogramsResponse>> SyncIfNeeded()
	{
		_logger?.LogInfoWithSource("Sync if needed called", "SyncIfNeeded");
		await _planogramService.RefreshIfNeeded();
		return Ok();
	}
}
