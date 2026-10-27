namespace Redbox.Rental.Model.Session;

public interface IDiscOfferCode
{
	string Barcode { get; set; }

	string OfferCode { get; set; }
}
