using System.Collections.Generic;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.API;

public class MerchandStatusUpdateResponse : IMessageScrub
{
	public List<MerchandStatusUpdate> Data { get; set; }

	public string error { get; set; }

	public string hostName { get; set; }

	public object Scrub()
	{
		return JsonConvert.SerializeObject(this);
	}
}
