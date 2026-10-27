using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Services.HttpQueue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HttpQueueController : ControllerBase
{
	private readonly ILogger<HttpQueueController> _logger;

	private readonly IHttpQueueService _httpQueueInterface;

	public HttpQueueController(ILogger<HttpQueueController> logger, IHttpQueueService httpQueueInterface)
	{
		_logger = logger;
		_httpQueueInterface = httpQueueInterface;
	}

	[Route("List")]
	[HttpGet]
	public async Task<ActionResult<IEnumerable<HttpQueueServiceRequest>>> List()
	{
		_logger.LogInformation("HttpQueueController.List called");
		return new OkObjectResult(await Task.Run(() => _httpQueueInterface.List()));
	}

	[Route("Enqueue")]
	[HttpPost]
	public async Task<ActionResult<bool>> Enqueue(HttpQueueServiceRequest request)
	{
		_logger.LogInformation("HttpQueueController.Enqueue called");
		return new OkObjectResult(await Task.Run(() => _httpQueueInterface.Enqueue(request)));
	}

	[Route("delete")]
	[HttpDelete]
	public async Task<ActionResult<bool>> Delete(Guid id)
	{
		_logger.LogInformation($"HttpQueueController.Delete called for queue item: '{id}'");
		HttpQueueServiceRequest request = new HttpQueueServiceRequest
		{
			Id = id
		};
		return new OkObjectResult(await Task.Run(() => HttpQueueInternals.Finalize(request)));
	}
}
