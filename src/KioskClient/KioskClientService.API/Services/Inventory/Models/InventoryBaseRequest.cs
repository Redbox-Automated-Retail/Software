using System;

namespace KioskClientService.API.Services.Inventory.Models;

public abstract class InventoryBaseRequest
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }
}
