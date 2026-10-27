using System;
using System.Net.Http;
using System.Threading.Tasks;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using HttpHelper;
using Newtonsoft.Json;
using Serilog;

namespace Status.Client;

public class StatusClient : IStatusClient
{
	private const string AUTHHEADER = "Authorization";

	private readonly IHttpService _httpService;

	private readonly ILogger _logger;

	public StatusClient(IHttpService httpService, ILogger logger)
	{
		_httpService = httpService;
		_logger = logger;
	}

	public async Task<StandardResponse> PostDeviceStatus(DeviceStatusRequest request)
	{
		_ = 1;
		StandardResponse standardResponse;
		try
		{
			HttpRequestMessage request2 = _httpService.GenerateRequest("DeviceStatus", request.Status, HttpMethod.Post, "Authorization", "Bearer " + request.ApiKey, request.ApiUrl + "/api/KioskData");
			standardResponse = JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request2, 5000)).Content?.ReadAsStringAsync()));
		}
		catch (Exception e)
		{
			standardResponse = new StandardResponse(e);
		}
		_logger.Information($"PostDeviceStatus Result: {JsonConvert.SerializeObject(standardResponse)}");
		return standardResponse;
	}

	public async Task<StandardResponse> PostRebootStatus(RebootStatusRequest request)
	{
		_ = 1;
		StandardResponse standardResponse;
		try
		{
			HttpRequestMessage request2 = _httpService.GenerateRequest("RebootStatus", request.Status, HttpMethod.Post, "Authorization", "Bearer " + request.ApiKey, request.ApiUrl + "/api/KioskData");
			standardResponse = JsonConvert.DeserializeObject<StandardResponse>(await ((await _httpService.SendRequest(request2, 5000)).Content?.ReadAsStringAsync()));
		}
		catch (Exception e)
		{
			standardResponse = new StandardResponse(e);
		}
		_logger.Information($"PostRebootStatus Result: {JsonConvert.SerializeObject(standardResponse)}");
		return standardResponse;
	}
}
