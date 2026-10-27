using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpdateClientService.API.Services;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
	private ILogger<StatusController> _logger;

	private IStatusService _status;

	public StatusController(ILogger<StatusController> logger, IStatusService status)
	{
		_logger = logger;
		_status = status;
	}

	[HttpGet("versions")]
	[ProducesResponseType(200, Type = typeof(FileVersionDataResponse))]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public IActionResult FileVersions()
	{
		FileVersionDataResponse fileVersions = _status.GetFileVersions();
		return fileVersions.ToObjectResult();
	}

	[HttpPost("versions")]
	[ProducesResponseType(200, Type = typeof(FileVersionDataResponse))]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public IActionResult FileVersions(IEnumerable<string> filePaths)
	{
		FileVersionDataResponse fileVersions = _status.GetFileVersions(filePaths);
		return fileVersions.ToObjectResult();
	}
}
