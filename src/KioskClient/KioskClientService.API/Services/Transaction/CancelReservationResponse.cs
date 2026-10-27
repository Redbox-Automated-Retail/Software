using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;

namespace KioskClientService.API.Services.Transaction;

public class CancelReservationResponse : BaseResponse, IReplayableApiCall
{
	public bool Replayable => true;

	public CancelReservationResponse()
		: base(200)
	{
		base.Success = true;
	}

	public CancelReservationResponse(int statusCode)
		: base(statusCode)
	{
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
