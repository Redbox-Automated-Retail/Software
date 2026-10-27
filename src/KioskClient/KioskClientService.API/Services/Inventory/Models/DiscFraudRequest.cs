using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.Models;

public class DiscFraudRequest : InventoryBaseRequest
{
	public string Barcode { get; set; }

	public DateTime TransactionDate { get; set; }

	[JsonProperty(Required = Required.DisallowNull)]
	public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}
