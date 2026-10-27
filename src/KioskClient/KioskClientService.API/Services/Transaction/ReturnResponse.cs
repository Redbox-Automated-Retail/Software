using System;
using System.Linq;
using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;

namespace KioskClientService.API.Services.Transaction;

public class ReturnResponse : BaseResponse, IReplayableApiCall
{
	public Guid MessageId { get; set; }

	public bool Replayable => !base.Errors.Any((Error e) => ReturnErrorCodes.DoNotQueueErrorCodes.Contains(e.Code));

	public ReturnResponse()
		: base(200)
	{
	}

	public ReturnResponse(int statusCode)
		: base(statusCode)
	{
	}
}
