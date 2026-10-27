using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskClientService.Utilities;

namespace Redbox.Rental.Services.KioskClientService.Utilities;

public class KioskClientServiceQRCodeUtilities : BaseKioskClientService, IKioskClientServiceQRCodeUtilities
{
	private const string _apiPath = "api/utilities";

	private const int _defaultTimeout = 500;

	public IGetQRCodeResponse GetQRCode(string valueToEncode)
	{
		WebResult<GetQRCodeResponse> webResult = ExecuteWebRequest<GetQRCodeResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/utilities/getqrcode?valueToEncode=" + valueToEncode), null, "GET", 500);
		LogResponse("KioskClientServiceQRCodeUtilities.GetQRCode for value: " + valueToEncode, webResult);
		return webResult?.Response;
	}

	public BitmapImage GetQRCodeImage(string url, bool returnDefaultOnError)
	{
		BitmapImage bitmapImage = null;
		IGetQRCodeResponse qRCode = GetQRCode(url);
		if (qRCode != null && qRCode.Success && !string.IsNullOrEmpty(qRCode?.QRCodeBitmapBase64String))
		{
			try
			{
				using MemoryStream stream = new MemoryStream(Convert.FromBase64String(qRCode.QRCodeBitmapBase64String));
				Image image = Image.FromStream(stream);
				if (image != null)
				{
					bitmapImage = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.ToBitmapImage(image);
				}
			}
			catch (Exception arg)
			{
				LogHelper.Instance.Log($"GetQRCodeImage: Unable to convert byte array to BitmapImage.  Exception:{arg}");
			}
		}
		if ((bitmapImage == null) & returnDefaultOnError)
		{
			bitmapImage = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "qr-code-default-redbox.png")));
		}
		return bitmapImage;
	}
}
