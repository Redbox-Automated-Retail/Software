using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.KDS;
using DeviceService.ComponentModel.Responses;
using DeviceService.Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.WebApi.KDS;

public class KioskDataServiceClient : IKioskDataServiceClient
{
	private const string AUTHHEADER = "Authorization";

	private const string KIOSKIDHEADER = "x-redbox-kioskid";

	private readonly IHttpService _httpService;

	private readonly string _apiUrl;

	private readonly List<Header> _headers = new List<Header>();

	private readonly ILogger _logger;

	public KioskDataServiceClient(IHttpService httpService, ILogger<KioskDataServiceClient> logger, IApplicationSettings settings)
	{
		_httpService = httpService;
		_logger = logger;
		KioskData fileData = DataFileHelper.GetFileData<KioskData>(settings.DataFilePath, "KioskData.json", LogException);
		if (fileData != null)
		{
			_apiUrl = fileData.ApiUrl;
			_headers.Add(new Header("Authorization", "Bearer " + fileData.ApiKey));
			_headers.Add(new Header("x-redbox-kioskid", fileData.KioskId.ToString()));
		}
	}

	public void LogException(string message, Exception e)
	{
		_logger?.LogError(e, "Unhandled Exception in " + message);
	}

	public async Task<StandardResponse> PostDeviceStatus(DeviceStatus deviceStatus)
	{
		_ = 1;
		StandardResponse standardResponse;
		try
		{
			HttpRequestMessage request = _httpService.GenerateRequest("DeviceStatus", deviceStatus, HttpMethod.Post, _apiUrl + "/api/KioskData", _headers);
			standardResponse = JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request, 5000)).Content?.ReadAsStringAsync()));
		}
		catch (Exception e)
		{
			standardResponse = new StandardResponse(e);
		}
		_logger.LogInformation("PostDeviceStatus Result: " + JsonConvert.SerializeObject(standardResponse));
		return standardResponse;
	}

	public async Task<StandardResponse> PostRebootStatus(RebootStatus rebootStatus)
	{
		_ = 1;
		StandardResponse standardResponse;
		try
		{
			HttpRequestMessage request = _httpService.GenerateRequest("RebootStatus", rebootStatus, HttpMethod.Post, _apiUrl + "/api/KioskData", _headers);
			standardResponse = JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request, 5000)).Content?.ReadAsStringAsync()));
		}
		catch (Exception e)
		{
			standardResponse = new StandardResponse(e);
		}
		_logger.LogInformation("PostRebootStatus Result: " + JsonConvert.SerializeObject(standardResponse));
		return standardResponse;
	}

	public async Task<StandardResponse> PostCardStats(CardStats cardStats)
	{
		_ = 1;
		StandardResponse standardResponse;
		try
		{
			_logger.LogInformation("Preparing to send PostCardStats to Kiosk Data Service");
			HttpRequestMessage request = _httpService.GenerateRequest("CardStats", cardStats, HttpMethod.Post, _apiUrl + "/api/KioskData", _headers);
			standardResponse = JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request, 5000)).Content?.ReadAsStringAsync()));
		}
		catch (Exception e)
		{
			standardResponse = new StandardResponse(e);
		}
		_logger.LogInformation("PostCardStats Result: " + JsonConvert.SerializeObject(standardResponse));
		return standardResponse;
	}
}
