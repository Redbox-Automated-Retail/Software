using System.Collections.Generic;
using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory;

public class UpdateMerchandizingStatusResponse : BaseResponse, IMessageScrub, IReplayableApiCall
{
	public List<MerchandStatusUpdate> FailedBarcodes { get; set; }

	public string Error { get; set; }

	public bool Replayable => true;

	public UpdateMerchandizingStatusResponse()
		: base(200)
	{
	}

	public UpdateMerchandizingStatusResponse(int statusCode)
		: base(statusCode)
	{
	}

	public override object Scrub()
	{
		return JsonConvert.SerializeObject(this);
	}
}
