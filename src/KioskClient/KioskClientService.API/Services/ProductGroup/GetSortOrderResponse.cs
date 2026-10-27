using KioskClientService.API.App;
using KioskClientService.API.Services.Inventory.API;

namespace KioskClientService.API.Services.ProductGroup;

public class GetSortOrderResponse : BaseResponse, IMessageScrub
{
	public ProductGroupSortOrder ProductGroupSortOrder { get; set; }

	public GetSortOrderResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return new
		{
			Type = ProductGroupSortOrder.SortOrderType,
			Count = ProductGroupSortOrder.SortOrders.Count
		};
	}

	public void CopyFrom(GetProductGroupSortOrder pgo)
	{
		ProductGroupSortOrder.SortOrderType = pgo.ProductGroupSortOrderType;
		ProductGroupSortOrder.SortOrders.Clear();
		int position = 1;
		pgo.ProductGroupSortOrders.ForEach(delegate(KioskClientService.API.Services.Inventory.API.ProductGroupSortOrder x)
		{
			ProductGroupSortOrder.SortOrders.Add(new SortOrder
			{
				ProductGroupId = x.ProductGroupId,
				ForecastedRents = x.ForecastedRents,
				Position = position++
			});
		});
	}
}
