using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;

namespace KioskClientService.API.Services.Transaction;

public class ReconcileResponse : BaseResponse, IReplayableApiCall
{
	public bool Replayable => true;

	public string CustomerProfileNumber { get; set; }

	public ReconcileResponse()
		: base(200)
	{
		base.Success = true;
	}

	public ReconcileResponse(int statusCode)
		: base(statusCode)
	{
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
