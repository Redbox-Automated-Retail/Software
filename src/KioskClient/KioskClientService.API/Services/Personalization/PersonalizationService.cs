using KioskClientService.API.App;
using KioskClientService.API.Services.TestData;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Personalization;

public class PersonalizationService : IPersonalizationService
{
	private readonly ILogger<IPersonalizationService> _logger;

	private readonly IStoreService _storeService;

	private readonly ITestDataService _testDataService;

	private readonly IHttpService _httpService;

	public PersonalizationService(ILogger<IPersonalizationService> logger, ITestDataService testDataService, IStoreService storeService, IHttpService httpService)
	{
		_logger = logger;
		_testDataService = testDataService;
		_storeService = storeService;
		_httpService = httpService;
	}
}
