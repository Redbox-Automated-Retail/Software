using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.IoT.Certificate;

public class IoTCertificateServiceApiClient : IIoTCertificateServiceApiClient
{
	private readonly IHttpService _httpService;

	private readonly IStoreService _store;

	private readonly AppSettings _appSettings;

	private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	public IoTCertificateServiceApiClient(IHttpService httpService, IStoreService store, IOptions<AppSettings> appSettings)
	{
		_httpService = httpService;
		_store = store;
		_appSettings = appSettings.Value;
		_httpService.Timeout = 90000;
	}

	public async Task<IsCertificateValidResponse> IsCertificateValid(string kioskId, string thingType, string password, string certificateId)
	{
		IsCertificateValidResponse response = new IsCertificateValidResponse
		{
			IsValid = null
		};
		var requestObject = new
		{
			type = thingType,
			name = kioskId,
			thingGroupName = _store.Market,
			certificateId = certificateId
		};
		APIResponse<bool> aPIResponse = await _httpService.SendRequestAsync<bool>(_appSettings.IoTCertificateServiceUrl, "api/thing-types/certificate-valid", requestObject, HttpMethod.Post, new List<Header>
		{
			new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey),
			new Header("password", password)
		}, isForm: false, null, "IsCertificateValid", "/sln/src/UpdateClientService.API/Services/IoT/Certificate/IoTCertificateServiceApiClient.cs");
		if (aPIResponse.IsSuccessStatusCode)
		{
			response.IsValid = aPIResponse.Response;
		}
		else
		{
			response.IsValid = null;
		}
		return response;
	}

	public async Task<IoTCertificateServiceResponse> GetCertificates(string kioskId, string thingType, string password)
	{
		await _lock.WaitAsync();
		var requestObject = new
		{
			type = thingType,
			name = kioskId,
			thingGroupName = _store.Market
		};
		APIResponse<IoTCertificateServiceResponse> aPIResponse = await _httpService.SendRequestAsync<IoTCertificateServiceResponse>(_appSettings.IoTCertificateServiceUrl, "api/thing-types/get-certificates", requestObject, HttpMethod.Post, new List<Header>
		{
			new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey),
			new Header("password", password)
		}, isForm: false, null, "GetCertificates", "/sln/src/UpdateClientService.API/Services/IoT/Certificate/IoTCertificateServiceApiClient.cs", null, logRequest: true, logResponse: false);
		_lock.Release();
		return aPIResponse?.Response;
	}

	public async Task<IoTCertificateServiceResponse> GenerateNewCertificates(string kioskId, string thingType, string password)
	{
		_ = 1;
		APIResponse<IoTCertificateServiceResponse> aPIResponse;
		try
		{
			await _lock.WaitAsync();
			var requestObject = new
			{
				type = thingType,
				name = kioskId,
				thingGroupName = _store.Market
			};
			aPIResponse = await _httpService.SendRequestAsync<IoTCertificateServiceResponse>(_appSettings.IoTCertificateServiceUrl, "api/thing-types/certificates", requestObject, HttpMethod.Post, new List<Header>
			{
				new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey),
				new Header("password", password)
			}, isForm: false, null, "GenerateNewCertificates", "/sln/src/UpdateClientService.API/Services/IoT/Certificate/IoTCertificateServiceApiClient.cs", null, logRequest: true, logResponse: false);
		}
		finally
		{
			_lock.Release();
		}
		return aPIResponse?.Response;
	}
}
