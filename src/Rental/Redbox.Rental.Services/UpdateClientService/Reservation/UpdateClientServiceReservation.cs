using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.UpdateClientService.Reservation;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.UpdateClientService.Reservation;

public class UpdateClientServiceReservation : BaseUpdateClientService, IUpdateClientServiceReservation
{
	private const string _apiPath = "api/Broker";

	public async Task<IRegisterWithBrokerServiceResponse> RegisterWithBrokerService(IRegisterWithBrokerServiceRequest registerWithBrokerServiceRequest)
	{
		LogHelper.Instance.Log("UpdateClientServiceReservation.RegisterWithBrokerService");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		IHttpService service2 = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service2.GenerateRequest(base.UpdateClientServiceBaseUrl, "api/Broker/Register", registerWithBrokerServiceRequest, HttpMethod.Post, new List<Header>
		{
			new Header("app-name", service["ProductName"]),
			new Header("x-redbox-activityid", registerWithBrokerServiceRequest?.MessageId.ToString())
		});
		APIResponse<HttpResponseMessage> aPIResponse = await service2.SendRequestAsync<HttpResponseMessage>(request, 20000, "RegisterWithBrokerService", "c:\\build\\Redbox.Rental.Services\\UpdateClientService\\Reservation\\UpdateClientServiceReservation.cs", registerWithBrokerServiceRequest);
		return new RegisterWithBrokerServiceResponse
		{
			StatusCode = (int)aPIResponse.StatusCode,
			Success = (aPIResponse?.IsSuccessStatusCode ?? false)
		};
	}

	public async Task<IUnregisterFromBrokerServiceResponse> UnregisterFromBrokerService(IUnregisterFromBrokerServiceRequest unregisterFromBrokerServiceRequest)
	{
		LogHelper.Instance.Log("UpdateClientServiceReservation.UnregisterFromBrokerService");
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(base.UpdateClientServiceBaseUrl, "api/Broker/UnRegister", unregisterFromBrokerServiceRequest, HttpMethod.Post, new List<Header>
		{
			new Header("app-name", unregisterFromBrokerServiceRequest?.AppName),
			new Header("x-redbox-activityid", unregisterFromBrokerServiceRequest?.MessageId.ToString())
		});
		APIResponse<HttpResponseMessage> aPIResponse = await service.SendRequestAsync<HttpResponseMessage>(request, null, "UnregisterFromBrokerService", "c:\\build\\Redbox.Rental.Services\\UpdateClientService\\Reservation\\UpdateClientServiceReservation.cs", unregisterFromBrokerServiceRequest);
		return new UnregisterFromBrokerServiceResponse
		{
			StatusCode = (int)aPIResponse.StatusCode,
			Success = (aPIResponse?.IsSuccessStatusCode ?? false)
		};
	}
}
