using System.Collections.Generic;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.Models;

public class InventorySnapshotRequest : InventoryBaseRequest
{
	[JsonProperty(Required = Required.DisallowNull)]
	public List<InventoryItem> Discs { get; set; } = new List<InventoryItem>();
}
