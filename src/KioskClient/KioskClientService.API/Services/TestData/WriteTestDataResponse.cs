using KioskClientService.API.App;

namespace KioskClientService.API.Services.TestData;

public class WriteTestDataResponse : BaseResponse
{
	public WriteTestDataResponse()
		: base(200)
	{
		base.Success = true;
	}
}
