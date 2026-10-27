using Redbox.Rental.Model.KioskClientService.TestData;

namespace Redbox.Rental.Services.KioskClientService.TestData;

public class IsTestDataEnabledResult : IIsTestDataEnabledResult
{
	public bool Success { get; set; }

	public bool IsTestDataEnabled { get; set; }

	public static IIsTestDataEnabledResult CreateFromIsTestDataEnabledResponse(IsTestDataEnabledResponse isTestDataEnabledResponse)
	{
		IsTestDataEnabledResult isTestDataEnabledResult = new IsTestDataEnabledResult();
		if (isTestDataEnabledResponse != null)
		{
			isTestDataEnabledResult.Success = isTestDataEnabledResponse.Success;
			isTestDataEnabledResult.IsTestDataEnabled = isTestDataEnabledResponse.IsTestDataEnabled;
		}
		return isTestDataEnabledResult;
	}
}
