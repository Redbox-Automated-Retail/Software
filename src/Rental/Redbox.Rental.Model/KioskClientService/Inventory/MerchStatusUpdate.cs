using System;

namespace Redbox.Rental.Model.KioskClientService.Inventory;

public class MerchStatusUpdate
{
	public DateTime? ThinDateUTC { get; set; }

	public long? ThinReasonCode { get; set; }

	public long? MerchandStatus { get; set; }

	public string Barcode { get; set; }

	public DateTime UpdateDateTimeUTC { get; set; }
}
