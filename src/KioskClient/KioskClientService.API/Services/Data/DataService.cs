using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.HttpQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Data;

internal class DataService : IDataService
{
	private readonly ILogger<IDataService> _logger;

	private readonly IDataClient _client;

	private readonly IHttpQueueService _queue;

	public DataService(ILogger<IDataService> logger, IDataClient client, IHttpQueueService queue)
	{
		_logger = logger;
		_client = client;
		_queue = queue;
	}

	public async Task<DataModels.KioskDataResponse> SendAuditEvent(HttpRequest httpRequest, DataModels.AuditEventRequest request)
	{
		return await Send("AuditEvent", httpRequest, request, _client.SendAuditEvent);
	}

	public async Task<DataModels.KioskDataResponse> SendKioskAlert(HttpRequest httpRequest, DataModels.KioskAlertRequest request)
	{
		return await Send("KioskAlert", httpRequest, request, _client.SendKioskAlert);
	}

	public async Task<DataModels.KioskDataResponse> SendKioskEvents(HttpRequest httpRequest, DataModels.KioskEventsRequest request)
	{
		return await Send("KioskEvents", httpRequest, request, _client.SendKioskEvents);
	}

	public async Task<DataModels.KioskDataResponse> SendKioskFunctionCheck(HttpRequest httpRequest, DataModels.KioskFunctionCheckRequest request)
	{
		return await Send("KioskFunctionCheck", httpRequest, request, _client.SendKioskFunctionCheck);
	}

	public async Task<DataModels.KioskDataResponse> SendProperties(HttpRequest httpRequest, DataModels.KioskPropertiesRequest request)
	{
		return await Send("KioskProperties", httpRequest, request, _client.SendProperties);
	}

	public async Task<DataModels.KioskDataResponse> SendStatistics(HttpRequest httpRequest, DataModels.KioskStatisticsRequest request)
	{
		return await Send("KioskStatistics", httpRequest, request, _client.SendStatistics);
	}

	public async Task<DataModels.KioskDataResponse> SendVersions(HttpRequest httpRequest, DataModels.KioskVersionRequest request)
	{
		return await Send("KioskVersions", httpRequest, request, _client.SendVersions);
	}

	private async Task<DataModels.KioskDataResponse> Send(string name, HttpRequest httpRequest, DataModels.KioskBaseRequest request, DataClientMethod method)
	{
		APIResponse<DataModels.KioskDataResponse> aPIResponse = await method(request);
		if (request != null)
		{
			aPIResponse.CheckAndReplay(request.MessageId, httpRequest, _queue);
		}
		return aPIResponse.Response;
	}
}
