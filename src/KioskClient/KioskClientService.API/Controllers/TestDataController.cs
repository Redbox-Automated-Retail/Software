using System.Threading.Tasks;
using KioskClientService.API.Services.TestData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/testdata")]
[ApiController]
public class TestDataController : ControllerBase
{
	private readonly ILogger _logger;

	private readonly ITestDataService _testDataService;

	public TestDataController(ILogger<TestDataController> logger, ITestDataService testDataService)
	{
		_logger = logger;
		_testDataService = testDataService;
	}

	[HttpPost]
	[Route("writepropertyvalue")]
	public async Task<ActionResult<WriteTestDataResponse>> WritePropertyValueAsync(string category, string propertyName, string propertyValue)
	{
		_logger?.LogInformation("TestDataController.WritePropertyValue");
		return new OkObjectResult(await (_testDataService?.WriteTestDataValueAsync(category, propertyName, propertyValue)));
	}

	[HttpGet]
	[Route("readpropertyvalue")]
	public async Task<ActionResult<ReadTestDataResponse>> ReadPropertyValueAsync(string category, string propertyName)
	{
		_logger?.LogInformation("TestDataController.ReadPropertyValueAsync");
		return new OkObjectResult(await (_testDataService?.ReadTestDataValueAsync(category, propertyName)));
	}

	[HttpGet]
	[Route("istestdataenabled")]
	public async Task<ActionResult<IsTestDataEnabledResponse>> GetIsTestDataEnabledAsync()
	{
		_logger?.LogInformation("TestDataController.GetIsTestDataEnabledAsync");
		return new OkObjectResult(await (_testDataService?.GetIsTestDataEnabledAsync()));
	}
}
