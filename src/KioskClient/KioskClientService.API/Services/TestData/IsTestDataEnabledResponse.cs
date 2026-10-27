using KioskClientService.API.App;

namespace KioskClientService.API.Services.TestData;

public class IsTestDataEnabledResponse : BaseResponse
{
	public bool IsTestDataEnabled { get; set; }

	public IsTestDataEnabledResponse()
		: base(200)
	{
		base.Success = true;
	}
}
