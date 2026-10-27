using KioskClientService.API.App;

namespace KioskClientService.API.Services.Dual;

public class RefreshResponse : ApiBaseResponse
{
	public override object Scrub()
	{
		return this.ToJson();
	}
}
