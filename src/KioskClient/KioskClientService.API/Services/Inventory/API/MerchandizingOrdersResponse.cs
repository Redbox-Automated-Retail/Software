using System.Collections.Generic;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.API;

public class MerchandizingOrdersResponse : List<MerchandizingOrder>, IMessageScrub
{
	public MerchandizingOrdersResponse()
	{
	}

	public MerchandizingOrdersResponse(List<MerchandizingOrder> orders)
	{
		if (orders != null)
		{
			AddRange(orders);
		}
	}

	public object Scrub()
	{
		if (base.Count != 1)
		{
			return $"{base.Count} orders found";
		}
		return JsonConvert.SerializeObject(this);
	}
}
