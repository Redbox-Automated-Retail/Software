using System;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Data;

internal class DataClient : IDataClient
{
	private readonly ILogger<IDataClient> _logger;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly IHttpService _httpService;

	public DataClient(ILogger<IDataClient> logger, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IHttpService httpService)
	{
		_logger = logger;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_httpService = httpService;
	}

	public async Task<APIResponse<DataModels.KioskDataResponse>> SendAuditEvent(DataModels.KioskBaseRequest request)
	{
		return await Send(request.MessageId, request.KioskId, request, "api/kioskdata/auditevent");
	}

	public async Task<APIResponse<DataModels.KioskDataResponse>> SendKioskAlert(DataModels.KioskBaseRequest request)
	{
		return await Send(request.MessageId, request.KioskId, request, "api/kioskdata/kioskalert");
	}

	public async Task<APIResponse<DataModels.KioskDataResponse>> SendKioskEvents(DataModels.KioskBaseRequest request)
	{
		return await Send(request.MessageId, request.KioskId, request, "api/kioskdata/kioskevents");
	}

	public async Task<APIResponse<DataModels.KioskDataResponse>> SendKioskFunctionCheck(DataModels.KioskBaseRequest request)
	{
		return await Send(request.MessageId, request.KioskId, request, "api/kioskdata/kioskfunctioncheck");
	}

	public async Task<APIResponse<DataModels.KioskDataResponse>> SendProperties(DataModels.KioskBaseRequest request)
	{
		return await Send(request.MessageId, request.KioskId, request, "api/kioskdata/kioskproperties");
	}

	public async Task<APIResponse<DataModels.KioskDataResponse>> SendStatistics(DataModels.KioskBaseRequest request)
	{
		return await Send(request.MessageId, request.KioskId, request, "api/kioskdata/kioskstatistics");
	}

	public async Task<APIResponse<DataModels.KioskDataResponse>> SendVersions(DataModels.KioskBaseRequest request)
	{
		return await Send(request.MessageId, request.KioskId, request, "api/kioskdata/kioskversion");
	}

	private async Task<APIResponse<DataModels.KioskDataResponse>> Send(Guid messageId, long kioskId, DataModels.KioskBaseRequest payload, string api)
	{
		KioskCloudClientConfiguration kioskCloudClientConfiguration = new KioskCloudClientConfiguration
		{
			APIKey = _kioskConfiguration.KioskClientDefault.KioskDataServiceAPIKey,
			AuthHeader = _kioskConfiguration.KioskClientDefault.KioskDataServiceAuthHeader
		};
		return await _httpService.SendRequest<DataModels.KioskDataResponse>(_kioskConfiguration.KioskClientDefault.KioskDataServiceUrl, api, payload, HttpMethod.Post, kioskCloudClientConfiguration.GetKioskCloudHeaders(messageId, kioskId), logResponseContent: false);
	}
}
