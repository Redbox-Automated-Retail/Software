using KioskClientService.API.App;

namespace KioskClientService.API.Services.Utilities;

public class GetQRCodeResponse : BaseResponse, IMessageScrub
{
	public string QRCodeBitmapBase64String { get; set; }

	public GetQRCodeResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		if (!string.IsNullOrEmpty(QRCodeBitmapBase64String))
		{
			return new
			{
				Success = base.Success,
				HostName = base.HostName,
				StatusCode = base.StatusCode,
				ResponseTime = base.ResponseTime,
				QRCodeBitmapBase64String = $"QR Code data with length of {QRCodeBitmapBase64String?.Length ?? 0}"
			}.ToJsonIndented();
		}
		return this.ToJsonIndented();
	}
}
