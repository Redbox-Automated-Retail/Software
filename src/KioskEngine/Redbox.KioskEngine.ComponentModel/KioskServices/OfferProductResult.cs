namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public class OfferProductResult
{
	public long ProductId { get; set; }

	public byte? NumberOfNights { get; set; }

	public decimal? DiscountedPrice { get; set; }

	public decimal? TotalDiscountAmount { get; set; }
}
