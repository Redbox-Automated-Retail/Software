using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class PersonalizationKioskClientLoginResponse : BaseResponse, IMessageScrub
{
	public KioskClientLoginResponse LoginResponse { get; set; }

	public PersonalizationKioskClientLoginResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
