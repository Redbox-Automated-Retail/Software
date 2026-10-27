using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.Models;

public class InventoryItem
{
	[JsonProperty(Required = Required.DisallowNull)]
	public string Barcode { get; set; }

	public short? Deck { get; set; }

	public short? Slot { get; set; }

	[JsonProperty(Required = Required.DisallowNull)]
	public string Status { get; set; }
}
