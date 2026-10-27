using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Bluefin;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;

namespace DeviceService.WebApi.Bluefin;

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
		HttpRequestMessage request2 = _httpService.GenerateRequest("device/activate", requestObject, HttpMethod.Post, request.BluefinServiceUrl, new List<Header> { GetAuthHeader(request.ApiKey) });
		return JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request2, request.Timeout)).Content?.ReadAsStringAsync()));
	}

	public async Task<StandardResponse> Deactivate(IBluefinActivationRequest request, string mfgSerialNumber)
	{
		DeactivateRequest requestObject = new DeactivateRequest
		{
			KioskId = request.KioskId,
			ReaderSerialNumber = mfgSerialNumber
		};
		HttpRequestMessage request2 = _httpService.GenerateRequest("device/deactivate", requestObject, HttpMethod.Post, request.BluefinServiceUrl, new List<Header> { GetAuthHeader(request.ApiKey) });
		return JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request2, request.Timeout)).Content?.ReadAsStringAsync()));
	}

	private Header GetAuthHeader(string apiKey)
	{
		return new Header("x-api-key", apiKey);
	}
}
