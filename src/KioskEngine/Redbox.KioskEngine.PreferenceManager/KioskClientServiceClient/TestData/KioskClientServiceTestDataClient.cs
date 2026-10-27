using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;

public class KioskClientServiceTestDataClient : KioskClientServiceBaseClient
{
	private static string _controllerPrefix = "api/testdata";

	public static async Task<APIResponse<ReadTestDataResponse>> ReadPropertyValue(string category, string propertyName)
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskClientServiceBaseClient.KioskClientServiceBaseUrl, _controllerPrefix + "/readpropertyvalue?category=" + category + "&propertyName=" + propertyName, null, HttpMethod.Get);
		return await service.SendRequestAsync<ReadTestDataResponse>(request, null, "ReadPropertyValue", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskClientServiceClient\\TestData\\KioskClientServiceTestDataClient.cs");
	}

	public static async Task<APIResponse<WriteTestDataResponse>> WritePropertyValue(string category, string propertyName, string propertyValue)
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskClientServiceBaseClient.KioskClientServiceBaseUrl, _controllerPrefix + "/writepropertyvalue?category=" + category + "&propertyName=" + propertyName + "&propertyValue=" + propertyValue, null, HttpMethod.Post);
		return await service.SendRequestAsync<WriteTestDataResponse>(request, null, "WritePropertyValue", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskClientServiceClient\\TestData\\KioskClientServiceTestDataClient.cs");
	}
}
