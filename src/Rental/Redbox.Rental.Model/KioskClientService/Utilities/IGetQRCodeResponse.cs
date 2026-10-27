namespace Redbox.Rental.Model.KioskClientService.Utilities;

public interface IGetQRCodeResponse : IBaseResponse
{
	string QRCodeBitmapBase64String { get; set; }
}
