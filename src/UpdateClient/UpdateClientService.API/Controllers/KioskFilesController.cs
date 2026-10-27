using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;

namespace UpdateClientService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KioskFilesController
{
	private readonly IKioskFilesService _kioskFilesService;

	public KioskFilesController(IKioskFilesService kioskFilesService)
	{
		_kioskFilesService = kioskFilesService;
	}

	[HttpGet("peek")]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> PeekKioskFiles([FromQuery] KioskFilePeekRequest request)
	{
		MqttResponse<Dictionary<string, string>> mqttResponse = await _kioskFilesService.PeekRequestedFilesAsync(request);
		if (!string.IsNullOrWhiteSpace(mqttResponse?.Error))
		{
			return new ObjectResult(mqttResponse.Error)
			{
				StatusCode = 500
			};
		}
		return new ObjectResult(mqttResponse.Data)
		{
			StatusCode = 200
		};
	}

	[HttpGet]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> UploadKioskFiles([FromQuery] KioskUploadFileRequest request)
	{
		MqttResponse<string> mqttResponse = await _kioskFilesService.UploadFilesAsync(request);
		if (!string.IsNullOrWhiteSpace(mqttResponse?.Error))
		{
			return new ObjectResult(mqttResponse.Error)
			{
				StatusCode = 500
			};
		}
		return new ObjectResult(mqttResponse.Data)
		{
			StatusCode = 200
		};
	}
}
