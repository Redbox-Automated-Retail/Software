using System.Collections.Generic;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.API;

public class GetProductGroupSortOrder : IMessageScrub
{
	public int ProductGroupSortOrderType { get; set; }

	public List<ProductGroupSortOrder> ProductGroupSortOrders { get; set; }

	public object Scrub()
	{
		return JsonConvert.SerializeObject(new { ProductGroupSortOrderType, ProductGroupSortOrders.Count });
	}
}
