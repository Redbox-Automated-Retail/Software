using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StateFileController : ControllerBase
{
	private readonly IStateFileService _stateFileService;

	public StateFileController(IStateFileService stateFileService)
	{
		_stateFileService = stateFileService;
	}

	[HttpGet]
	[ProducesResponseType(typeof(StateFilesResponse), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult> GetAll()
	{
		return (await _stateFileService.GetAll()).ToObjectResult();
	}

	[HttpGet("{fileSetId}")]
	[ProducesResponseType(typeof(StateFilesResponse), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult> Getl([FromRoute] long fileSetId)
	{
		return (await _stateFileService.Get(fileSetId)).ToObjectResult();
	}

	[HttpGet("inprogress")]
	[ProducesResponseType(typeof(StateFilesResponse), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult> GetAllInProgress()
	{
		return (await _stateFileService.GetAllInProgress()).ToObjectResult();
	}

	[HttpPost]
	[ProducesResponseType(typeof(StateFileResponse), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult> Save([FromBody] StateFile stateFile)
	{
		return (await _stateFileService.Save(stateFile)).ToObjectResult();
	}
}
