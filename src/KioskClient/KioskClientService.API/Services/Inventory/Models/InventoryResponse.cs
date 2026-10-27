using System;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Inventory.Models;

public class InventoryResponse : BaseResponse
{
	public Guid MessageId { get; set; }

	public bool Replayable => true;

	public InventoryResponse()
		: base(200)
	{
	}

	public override object Scrub()
	{
		return new { base.StatusCode, base.Errors }.ToJson();
	}
}
