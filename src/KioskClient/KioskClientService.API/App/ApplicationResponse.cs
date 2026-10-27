namespace KioskClientService.API.App;

public class ApplicationResponse : BaseResponse, IMessageScrub
{
	public ApplicationResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
