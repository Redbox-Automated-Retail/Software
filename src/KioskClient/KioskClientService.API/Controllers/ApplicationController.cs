using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/application")]
[ApiController]
public class ApplicationController : ControllerBase
{
	private readonly ILogger<ApplicationController> _logger;

	private readonly IApplicationService _applicationService;

	public ApplicationController(ILogger<ApplicationController> logger, IApplicationService applicationService)
	{
		_logger = logger;
		_applicationService = applicationService;
	}

	[HttpPost("start")]
	public async Task<ActionResult<ApplicationResponse>> StartApplication()
	{
		await _applicationService.Start();
		return new OkObjectResult(new ApplicationResponse());
	}

	[HttpPost("end")]
	public async Task<ActionResult<ApplicationResponse>> EndApplication()
	{
		await _applicationService.End();
		return new OkObjectResult(new ApplicationResponse());
	}

	[HttpPost("viewchangedevent")]
	public async Task<ActionResult<ViewShowResponse>> ViewChangedEvent(ViewStateChangedEvent e)
	{
		await _applicationService.ViewChangedEvent(e);
		return new OkObjectResult(new ViewShowResponse());
	}
}
