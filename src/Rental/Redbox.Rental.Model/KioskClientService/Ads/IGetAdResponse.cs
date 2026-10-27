namespace Redbox.Rental.Model.KioskClientService.Ads;

public interface IGetAdResponse : IBaseResponse
{
	string AdvertisementId { get; set; }

	int Width { get; set; }

	int Height { get; set; }

	string MimeType { get; set; }

	string AssetPath { get; set; }
}
