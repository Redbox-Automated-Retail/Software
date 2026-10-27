using System;
using System.Threading.Tasks;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Session;

namespace Redbox.Rental.Services.KioskClientService.Session;

public class KioskClientServiceSession : BaseKioskClientService, IKioskClientServiceSession
{
	private class CreateSessionRequest
	{
		public Guid KioskSessionId { get; set; }
	}

	private const string _apiPath = "api/session";

	public async Task<bool> Start(SessionType sessionType)
	{
		WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(string.Format("{0}/{1}/start?sessionType={2}", base.KioskClientServiceBaseUrl, "api/session", sessionType)), null, "POST").ConfigureAwait(continueOnCapturedContext: false);
		LogResponse("KioskClientServiceSession.Start", webResult);
		return webResult?.Response?.Success == true;
	}

	public async Task<bool> End()
	{
		WebResult<BaseResponse> webResult = await ExecuteAsyncWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/session/end"), null, "POST").ConfigureAwait(continueOnCapturedContext: false);
		LogResponse("KioskClientServiceSession.End", webResult);
		return webResult?.Response?.Success == true;
	}

	public bool Create(Guid kioskSessionId)
	{
		CreateSessionRequest requestPayload = new CreateSessionRequest
		{
			KioskSessionId = kioskSessionId
		};
		WebResult<BaseResponse> webResult = ExecuteWebRequest<BaseResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/session"), requestPayload, "POST");
		LogResponse("KioskClientServiceSession.Create", webResult);
		return webResult?.Response?.Success == true;
	}
}
