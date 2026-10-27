using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Services.ProfileData;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[ApiController]
[Route("api/profiledata")]
public class ProfileDataController : Controller
{
	private readonly IProfileDataService _profileDataService;

	public ProfileDataController(IProfileDataService profileDataService)
	{
		_profileDataService = profileDataService;
	}

	[HttpPost("triggerproductdataupdate")]
	[ProducesResponseType(typeof(TriggerProductDataUpdateResponse), 200)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> TriggerProductDataUpdate()
	{
		return (await _profileDataService.TriggerProductDataUpdate()).ToObjectResult();
	}

	[HttpPost("product")]
	[ProducesResponseType(typeof(AddDataRecordResponse<Product>), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AddProducts(List<long> productIds)
	{
		return (await _profileDataService.AddProducts(productIds)).ToObjectResult();
	}

	[HttpGet("instance")]
	[ProducesResponseType(typeof(GetProfileDataInstanceResponse), 200)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> GetProfileDataInstance(bool refreshFromFiles)
	{
		return (await _profileDataService.GetProfileDataInstance(refreshFromFiles)).ToObjectResult();
	}

	[HttpGet("instance/haspendingupdate")]
	[ProducesResponseType(typeof(GetProfileDataInstancePendingUpdateResponse), 200)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> GetProfileDataInstanceHasPendingUpdate()
	{
		GetProfileDataInstancePendingUpdateResponse getProfileDataInstancePendingUpdateResponse = _profileDataService.HasPendingUpdates();
		return getProfileDataInstancePendingUpdateResponse.ToObjectResult();
	}
}
