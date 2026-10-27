namespace Redbox.Rental.Model.Promotion;

public class OfferProduct
{
	public long ProductId { get; set; }

	public byte? NumberOfNights { get; set; }

	public decimal? DiscountedPrice { get; set; }

	public decimal? TotalDiscountAmount { get; set; }
}
