using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.Models;

public class DiscActionRequest : InventoryBaseRequest
{
	public string UserName { get; set; }

	public string JobName { get; set; }

	public DateTime ActionDate { get; set; }

	[JsonProperty(Required = Required.DisallowNull)]
	public List<Dictionary<string, string>> Actions { get; set; } = new List<Dictionary<string, string>>();
}
