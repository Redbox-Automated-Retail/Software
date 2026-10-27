using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Inventory;

public interface IGetMerchandizingOrdersResponse : IBaseResponse
{
	List<MerchandizingOrder> MerchandizingOrders { get; }
}
