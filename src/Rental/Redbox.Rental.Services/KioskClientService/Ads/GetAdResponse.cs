using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Ads;

namespace Redbox.Rental.Services.KioskClientService.Ads;

public class GetAdResponse : BaseResponse, IGetAdResponse, IBaseResponse
{
	public string AdvertisementId { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public string MimeType { get; set; }

	public string AssetPath { get; set; }
}
