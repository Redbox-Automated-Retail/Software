using System.Collections.Generic;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.API;

public class MerchandStatusUpdateRequest : List<MerchStatusUpdate>, IMessageScrub
{
	public MerchandStatusUpdateRequest()
	{
	}

	public MerchandStatusUpdateRequest(IEnumerable<MerchStatusUpdate> list)
	{
		if (list != null)
		{
			AddRange(list);
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
