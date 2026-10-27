using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Inventory;

public class GetMerchandizingOrdersResponse : BaseResponse, IMessageScrub
{
	public List<MerchandizingOrder> MerchandizingOrders { get; set; }

	public GetMerchandizingOrdersResponse()
		: base(200)
	{
	}

	public GetMerchandizingOrdersResponse(int statusCode)
		: base(statusCode)
	{
	}

	public override object Scrub()
	{
		List<MerchandizingOrder> merchandizingOrders = MerchandizingOrders;
		if (merchandizingOrders == null || merchandizingOrders.Count != 1)
		{
			return $"{MerchandizingOrders?.Count ?? 0} orders found";
		}
		return this.ToJson();
	}
}
