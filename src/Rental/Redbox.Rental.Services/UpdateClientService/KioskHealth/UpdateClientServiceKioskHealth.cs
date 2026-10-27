using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.UpdateClientService.KioskHealth;

public class UpdateClientServiceKioskHealth : BaseUpdateClientService, IUpdateClientServiceKioskHealth
{
	private const string _apiPath = "api/Broker";

	public async Task<PingResponse> SendPing(IPingRequest pingRequest)
	{
		LogHelper.Instance.Log("UpdateClientServiceKioskHealth.SendPing");
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(base.UpdateClientServiceBaseUrl, "api/Broker/Ping", pingRequest, HttpMethod.Post, new List<Header>
		{
			new Header("app-name", pingRequest?.AppName),
			new Header("x-redbox-activityid", pingRequest?.MessageId.ToString())
		});
		APIResponse<HttpResponseMessage> aPIResponse = await service.SendRequestAsync<HttpResponseMessage>(request, null, "SendPing", "c:\\build\\Redbox.Rental.Services\\UpdateClientService\\KioskHealth\\UpdateClientServiceKioskHealth.cs", pingRequest);
		PingResponse pingResponse = new PingResponse
		{
			StatusCode = (int)aPIResponse.StatusCode,
			Success = aPIResponse.IsSuccessStatusCode,
			HostName = aPIResponse.HostName
		};
		LogHelper.Instance.Log("UpdateClientServiceKioskHealth.SendPing response=" + JsonConvert.SerializeObject(pingResponse));
		return pingResponse;
	}
}
