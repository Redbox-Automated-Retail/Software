using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Products;

public class ProductGroupSortOrder
{
	public int SortOrderType { get; set; }

	public List<SortOrder> SortOrders { get; set; }

	public ProductGroupSortOrder()
	{
		SortOrders = new List<SortOrder>();
	}
}
