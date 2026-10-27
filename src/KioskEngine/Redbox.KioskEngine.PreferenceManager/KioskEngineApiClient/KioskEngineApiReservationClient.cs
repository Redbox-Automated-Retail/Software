using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.UpdateClientService.Reservation;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;

public class KioskEngineApiReservationClient : KioskEngineApiBaseClient
{
	private static string _apiPath = "api/reservation";

	public static async Task<APIResponse<GetReservationServiceStatusResponse>> GetReservationServiceStatus()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/getreservationservicestatus", null, HttpMethod.Get);
		return await service.SendRequestAsync<GetReservationServiceStatusResponse>(request, null, "GetReservationServiceStatus", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Reservation\\KioskEngineApiReservationClient.cs");
	}

	public static async Task<APIResponse<Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.GetReservationsResponse>> GetReservations()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/getreservations", null, HttpMethod.Get);
		return await service.SendRequestAsync<Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.GetReservationsResponse>(request, null, "GetReservations", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Reservation\\KioskEngineApiReservationClient.cs");
	}

	public static async Task<APIResponse<GetReservableTitlesResponse>> GetReservableTitles()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/getreservabletitles", null, HttpMethod.Get);
		return await service.SendRequestAsync<GetReservableTitlesResponse>(request, null, "GetReservableTitles", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Reservation\\KioskEngineApiReservationClient.cs");
	}

	public static async Task<APIResponse<Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.CreateReservationResponse>> CreateReservation(ReservationRequest reservationRequest)
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath ?? "", reservationRequest, HttpMethod.Post);
		return await service.SendRequestAsync<Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.CreateReservationResponse>(request, null, "CreateReservation", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Reservation\\KioskEngineApiReservationClient.cs");
	}

	public static async Task<APIResponse<CancelReservationResponse>> CancelReservations(string referenceNumber)
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/cancelreservation?referenceNumber=" + referenceNumber, null, HttpMethod.Post);
		return await service.SendRequestAsync<CancelReservationResponse>(request, null, "CancelReservations", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Reservation\\KioskEngineApiReservationClient.cs");
	}

	public static async Task<APIResponse<RegisterWithBrokerServiceResponse>> RegisterWithBrokerService()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/registerwithbrokerservice", null, HttpMethod.Post);
		return await service.SendRequestAsync<RegisterWithBrokerServiceResponse>(request, null, "RegisterWithBrokerService", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Reservation\\KioskEngineApiReservationClient.cs");
	}

	public static async Task<APIResponse<UnregisterFromBrokerServiceResponse>> UnregisterFromBrokerService()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/unregisterfrombrokerservice", null, HttpMethod.Post);
		return await service.SendRequestAsync<UnregisterFromBrokerServiceResponse>(request, null, "UnregisterFromBrokerService", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Reservation\\KioskEngineApiReservationClient.cs");
	}
}
