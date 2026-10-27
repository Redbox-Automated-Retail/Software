using System.Threading.Tasks;

namespace KioskClientService.API.Services.TestData;

public interface ITestDataHelper
{
	Task<bool> GetUseTestData();

	Task<T> GetTestDataValue<T>(string propertyName, T defaultValue);
}
