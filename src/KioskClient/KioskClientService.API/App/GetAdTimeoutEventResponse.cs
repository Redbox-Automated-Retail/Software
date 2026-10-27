namespace KioskClientService.API.App;

public class GetAdTimeoutEventResponse : BaseResponse, IMessageScrub
{
	public GetAdTimeoutEventResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return this.ToJson();
	}
}
