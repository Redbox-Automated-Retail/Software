using System;
using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;

namespace KioskClientService.API.Models;

public class KioskOptInResponse : BaseResponse, IReplayableApiCall
{
	public Guid MessageId { get; set; }

	public bool Replayable => !base.Success;

	public KioskOptInResponse()
		: base(200)
	{
	}
}
