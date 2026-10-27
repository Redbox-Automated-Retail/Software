using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;

namespace KioskClientService.API.Services.Transaction;

public class ExpireReservationResponse : BaseResponse, IReplayableApiCall
{
	public bool Replayable => true;

	public ExpireReservationResponse()
		: base(200)
	{
		base.Success = true;
	}

	public ExpireReservationResponse(int statusCode)
		: base(statusCode)
	{
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
