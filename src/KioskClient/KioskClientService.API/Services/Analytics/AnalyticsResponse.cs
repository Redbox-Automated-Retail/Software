using KioskClientService.API.App;

namespace KioskClientService.API.Services.Analytics;

public class AnalyticsResponse : BaseResponse, IMessageScrub
{
	public object KinesisResponse { get; set; }

	public AnalyticsResponse()
		: base(200)
	{
	}

	public override object Scrub()
	{
		return this.ToJson();
	}
}
