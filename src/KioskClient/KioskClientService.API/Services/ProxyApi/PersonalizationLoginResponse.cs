using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class PersonalizationLoginResponse : BaseResponse, IMessageScrub
{
	public LoginResponse LoginResponse { get; set; }

	public PersonalizationLoginResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
