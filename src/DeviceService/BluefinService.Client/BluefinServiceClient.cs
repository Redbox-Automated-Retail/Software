using System;
using System.Net.Http;
using System.Threading.Tasks;
using BluefinService.Client.Contract;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using HttpHelper;
using Newtonsoft.Json;

namespace BluefinService.Client;

public class BluefinServiceClient : IBluefinServiceClient
{
	private const string AUTHHEADER = "x-api-key";

	private readonly IHttpService _httpService;

	public BluefinServiceClient(IHttpService httpService)
	{
		_httpService = httpService;
	}

	public async Task<StandardResponse> Activate(IBluefinActivationRequest request, string mfgSerialNumber, string injectedSerialNumber, DateTime InstallDate)
	{
		ActivateRequest requestObject = new ActivateRequest
		{
			KioskId = request.KioskId,
			ReaderSerialNumber = mfgSerialNumber,
			InjectedSerialNumber = injectedSerialNumber,
			LocalDateTime = InstallDate
		};
		HttpRequestMessage request2 = _httpService.GenerateRequest("device/activate", requestObject, HttpMethod.Post, "x-api-key", request.ApiKey, request.BluefinServiceUrl);
		return JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request2, request.Timeout)).Content?.ReadAsStringAsync()));
	}

	public async Task<StandardResponse> Deactivate(IBluefinActivationRequest request, string mfgSerialNumber)
	{
		DeactivateRequest requestObject = new DeactivateRequest
		{
			KioskId = request.KioskId,
			ReaderSerialNumber = mfgSerialNumber
		};
		HttpRequestMessage request2 = _httpService.GenerateRequest("device/deactivate", requestObject, HttpMethod.Post, "x-api-key", request.ApiKey, request.BluefinServiceUrl);
		return JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request2, request.Timeout)).Content?.ReadAsStringAsync()));
	}
}
