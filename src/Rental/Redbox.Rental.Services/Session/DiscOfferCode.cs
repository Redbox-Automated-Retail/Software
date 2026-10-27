using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class DiscOfferCode : IDiscOfferCode
{
	public string Barcode { get; set; }

	public string OfferCode { get; set; }
}
