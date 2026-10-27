using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Services.Controllers;

public class AddProductToCartParameters
{
	public ITitleProduct TitleProduct { get; set; }

	public bool IsPurchase { get; set; }

	public IPricingRecord PricingRecord { get; set; }

	public ItemSourceType ItemSourceType { get; set; }

	public int? RedboxPlusBenefitId { get; set; }

	public bool IsRedboxPlusFreeOneNightRental => RedboxPlusBenefitId == 2;
}
