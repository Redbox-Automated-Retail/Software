using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.DataUpdate;

namespace UpdateClientService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataUpdateController : Controller
{
	private readonly IDataUpdateService _dataUpdateService;

	public DataUpdateController(IDataUpdateService dataUpdateService)
	{
		_dataUpdateService = dataUpdateService;
	}

	[HttpPost("changes")]
	[ProducesResponseType(typeof(DataUpdateResponse), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(401)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> GetRecordChanges(DataUpdateRequest dataUpdateRequest)
	{
		return (await _dataUpdateService.GetRecordChanges(dataUpdateRequest)).ToObjectResult();
	}
}
