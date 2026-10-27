namespace Redbox.Rental.Model.Ads;

public interface IVistarAd : IAd
{
	string AdvertisementId { get; set; }

	int Width { get; set; }

	int Height { get; set; }
}
