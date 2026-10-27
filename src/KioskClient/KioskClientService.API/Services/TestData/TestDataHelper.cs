using System.Threading.Tasks;

namespace KioskClientService.API.Services.TestData;

public class TestDataHelper : ITestDataHelper
{
	private readonly ITestDataService _testDataService;

	private readonly string _category;

	private readonly string _liveCallPropertyName;

	public TestDataHelper(ITestDataService testDataService, string category, string liveCallPropertyName)
	{
		_testDataService = testDataService;
		_category = category;
		_liveCallPropertyName = liveCallPropertyName;
	}

	public async Task<bool> GetUseTestData()
	{
		bool result = false;
		IsTestDataEnabledResponse isTestDataEnabledResponse = await _testDataService.GetIsTestDataEnabledAsync();
		if (isTestDataEnabledResponse != null && isTestDataEnabledResponse.IsTestDataEnabled)
		{
			ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync(_category, _liveCallPropertyName));
			if (readTestDataResponse != null && readTestDataResponse.Success && readTestDataResponse.GetPropertyValue<bool?>() == false)
			{
				result = true;
			}
		}
		return result;
	}

	public async Task<T> GetTestDataValue<T>(string propertyName, T defaultValue)
	{
		ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync(_category, propertyName));
		if (string.IsNullOrEmpty(readTestDataResponse.PropertyValue))
		{
			return defaultValue;
		}
		return readTestDataResponse.GetPropertyValue<T>();
	}
}
