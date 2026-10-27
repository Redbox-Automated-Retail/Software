using System;
using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class UpdateAccountResponse : ApiBaseResponse
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string MobilePhoneNumber { get; set; }

	public string TempPassword { get; set; }

	public void CopyFromApiResponse(APIResponse<UpdateAccountResponse> source)
	{
		if (source == null)
		{
			base.StatusCode = HttpStatusCode.InternalServerError;
			base.Error = base.StatusCode.ToString();
			return;
		}
		base.StatusCode = source.StatusCode;
		if (source.Response == null)
		{
			base.Error = base.StatusCode.ToString();
			return;
		}
		CustomerProfileNumber = source.Response.CustomerProfileNumber;
		MobilePhoneNumber = source.Response.MobilePhoneNumber;
		TempPassword = source.Response.TempPassword;
		base.Error = source.Response.Error;
	}

	public override object Scrub()
	{
		return new
		{
			StatusCode = base.StatusCode,
			Error = base.Error,
			MessageId = MessageId,
			KioskId = KioskId,
			CustomerProfileNumber = ObfuscationHelper.ObfuscateNumericData(CustomerProfileNumber, 1, 1),
			MobilePhoneNumber = ObfuscationHelper.ObfuscateNumericData(MobilePhoneNumber, 1, 1),
			TempPassword = ObfuscationHelper.ObfuscateStringData(TempPassword, 1, 1)
		}.ToJsonIndented();
	}
}
