using System.Windows.Media.Imaging;

namespace Redbox.Rental.Model.KioskClientService.Utilities;

public interface IKioskClientServiceQRCodeUtilities
{
	IGetQRCodeResponse GetQRCode(string valueToEncode);

	BitmapImage GetQRCodeImage(string url, bool returnDefaultOnError);
}
