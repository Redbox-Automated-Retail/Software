using System.Collections.Generic;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Inventory.API;

namespace KioskClientService.API.Services.ProductGroup;

public class ProductGroupSortOrder : IPersistentData
{
	public int SortOrderType { get; set; }

	public List<SortOrder> SortOrders { get; set; }

	public ProductGroupSortOrder()
	{
		SortOrders = new List<SortOrder>();
	}

	public void CopyFrom(GetProductGroupSortOrder pgo)
	{
		SortOrderType = pgo.ProductGroupSortOrderType;
		SortOrders.Clear();
		int position = 1;
		pgo.ProductGroupSortOrders.ForEach(delegate(KioskClientService.API.Services.Inventory.API.ProductGroupSortOrder x)
		{
			SortOrders.Add(new SortOrder
			{
				ProductGroupId = x.ProductGroupId,
				ForecastedRents = x.ForecastedRents,
				Position = position++
			});
		});
	}
}
