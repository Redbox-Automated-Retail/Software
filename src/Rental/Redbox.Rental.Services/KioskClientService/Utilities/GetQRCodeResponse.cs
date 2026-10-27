using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Utilities;

namespace Redbox.Rental.Services.KioskClientService.Utilities;

public class GetQRCodeResponse : BaseResponse, IGetQRCodeResponse, IBaseResponse
{
	public string QRCodeBitmapBase64String { get; set; }
}
