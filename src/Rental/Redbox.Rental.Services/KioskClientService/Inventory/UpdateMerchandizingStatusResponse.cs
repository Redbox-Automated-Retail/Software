using System.Collections.Generic;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Inventory;

namespace Redbox.Rental.Services.KioskClientService.Inventory;

public class UpdateMerchandizingStatusResponse : BaseResponse, IUpdateMerchandizingStatusResponse, IBaseResponse
{
	public List<MerchStatusUpdate> FailedBarcodes { get; set; }

	public string Error { get; set; }
}
