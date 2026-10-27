using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.Kernel;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class KernelController
{
	private readonly IKernelService _kernel;

	public KernelController(IKernelService kernel)
	{
		_kernel = kernel;
	}

	[HttpPost("shutdown")]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(StatusCodeResult), 500)]
	public ActionResult PerformShutdown(ShutdownType shutdownType)
	{
		if (!_kernel.PerformShutdown(shutdownType))
		{
			return new StatusCodeResult(500);
		}
		return new OkResult();
	}
}
