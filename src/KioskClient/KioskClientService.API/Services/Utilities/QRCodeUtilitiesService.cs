using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace KioskClientService.API.Services.Utilities;

public class QRCodeUtilitiesService : IQRCodeUtilitiesService
{
	public class Constants
	{
		public class Errors
		{
			public const string MissingValueCode = "100";

			public const string MissingValueMessage = "QR Code value must not be null or empty.";

			public const string ExceptionGeneratingQRCodeCode = "200";

			public const string ExceptionGeneratingQRCodeMessage = "Exception generating QR Code: ";

			public const string ErrorGeneratingQRCodeCode = "300";

			public const string ErrorGeneratingQRCodeMessage = "Error generating QR Code.  : ";
		}
	}

	private readonly ILogger<IQRCodeUtilitiesService> _logger;

	public QRCodeUtilitiesService(ILogger<IQRCodeUtilitiesService> logger)
	{
		_logger = logger;
	}

	public async Task<GetQRCodeResponse> GetQRCodeResponseAsync(string valueToEncode)
	{
		_logger.LogInformation("QRCodeUtilitiesService.GetQRCodeResponseAsync valueToEncode: " + valueToEncode);
		GetQRCodeResponse result = new GetQRCodeResponse();
		await Task.Run(delegate
		{
			try
			{
				if (!string.IsNullOrEmpty(valueToEncode))
				{
					QRCodeData data = new QRCodeGenerator()?.CreateQrCode(valueToEncode, QRCodeGenerator.ECCLevel.Q);
					Bitmap bitmap = new QRCode(data)?.GetGraphic(20);
					if (bitmap != null)
					{
						using MemoryStream memoryStream = new MemoryStream();
						bitmap.Save(memoryStream, ImageFormat.Png);
						memoryStream.Position = 0L;
						result.QRCodeBitmapBase64String = WebUtility.UrlEncode(Convert.ToBase64String(memoryStream.ToArray()));
					}
					if (result.QRCodeBitmapBase64String == null || result.QRCodeBitmapBase64String.Length == 0)
					{
						result.Success = false;
						result.Errors.Add(new Error
						{
							Code = "300",
							Message = "Error generating QR Code.  : "
						});
					}
				}
				else
				{
					result.Success = false;
					result.Errors.Add(new Error
					{
						Code = "100",
						Message = "QR Code value must not be null or empty."
					});
				}
			}
			catch (Exception ex)
			{
				result.Success = false;
				result.Errors.Add(new Error
				{
					Code = "200",
					Message = "Exception generating QR Code:  " + ex.Message
				});
			}
		});
		return result;
	}
}
