using System;
using Redbox.Rental.Model.KioskClientService.TestData;

namespace Redbox.Rental.Services.KioskClientService.TestData;

public class KioskClientServiceTestData : BaseKioskClientService, IKioskClientServiceTestData
{
	private const string _controllerPrefix = "api/testdata";

	public IIsTestDataEnabledResult GetIsTestDataEnabled()
	{
		WebResult<IsTestDataEnabledResponse> webResult = ExecuteWebRequest<IsTestDataEnabledResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/testdata/istestdataenabled"), null, "GET", 500);
		LogResponse("KioskClientServiceTestData", webResult);
		return IsTestDataEnabledResult.CreateFromIsTestDataEnabledResponse(webResult.Response);
	}

	public IReadTestDataValueResult ReadPropertyValue(string category, string propertyName)
	{
		WebResult<ReadTestDataResponse> webResult = ExecuteWebRequest<ReadTestDataResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/testdata/readpropertyvalue?category=" + category + "&propertyName=" + propertyName), null, "GET", 500);
		LogResponse("KioskClientServiceTestData", webResult);
		return ReadTestDataValueResult.CreateFromReadTestDataResponse(webResult?.Response);
	}

	public IWriteTestDataValueResult WritePropertyValue(string category, string propertyName, string propertyValue)
	{
		WebResult<WriteTestDataResponse> webResult = ExecuteWebRequest<WriteTestDataResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/testdata/writepropertyvalue?category=" + category + "&propertyName=" + propertyName + "&propertyValue=" + propertyValue), null, "POST", 500);
		LogResponse("KioskClientServiceTestData", webResult);
		return WriteTestDataValueResult.CreateFromWriteTestDataResponse(webResult?.Response);
	}
}
