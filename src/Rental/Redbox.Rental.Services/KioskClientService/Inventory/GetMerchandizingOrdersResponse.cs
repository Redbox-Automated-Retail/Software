using System.Collections.Generic;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Inventory;

namespace Redbox.Rental.Services.KioskClientService.Inventory;

public class GetMerchandizingOrdersResponse : BaseResponse, IGetMerchandizingOrdersResponse, IBaseResponse
{
	public List<MerchandizingOrder> MerchandizingOrders { get; set; }
}
