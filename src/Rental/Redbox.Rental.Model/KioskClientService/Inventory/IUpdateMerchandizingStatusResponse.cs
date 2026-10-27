using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Inventory;

public interface IUpdateMerchandizingStatusResponse : IBaseResponse
{
	List<MerchStatusUpdate> FailedBarcodes { get; set; }

	string Error { get; set; }
}
