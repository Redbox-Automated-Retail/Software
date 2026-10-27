using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.ProxyApi;

namespace UpdateClientService.API.Services.Broker;

public class PingStatisticsService : IPingStatisticsService
{
	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private readonly ILogger<PingStatisticsService> _logger;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly IProxyApi _proxyApi;

	private readonly IStoreService _storeService;

	private PingStatistics _pingStatistics;

	private const string PingStatisticsFileName = "PingStatistics.json";

	private static DateTime _initialReportFailedPingAttempt = DateTime.Now;

	private const int _initialDelayMinutes = 30;

	private const int _reportFailedPingAttemptWaitingPeriodMinutes = 60;

	public PingStatisticsService(ILogger<PingStatisticsService> logger, IPersistentDataCacheService persistentDataCacheService, IOptionsKioskConfiguration kioskConfiguration, IProxyApi proxyApi, IStoreService storeService)
	{
		_logger = logger;
		_persistentDataCacheService = persistentDataCacheService;
		_kioskConfiguration = kioskConfiguration;
		_proxyApi = proxyApi;
		_storeService = storeService;
	}

	public async Task<bool> RecordPingStatistic(bool pingSuccess)
	{
		bool result;
		try
		{
			PingStatistics pingStatistics = await GetPingStatistics();
			pingStatistics.Cleanup();
			PingStatistic pingStatistic = pingStatistics.GetLatest();
			DateTime now = DateTime.Now;
			if (pingStatistic == null || pingStatistic.PingSuccess != pingSuccess)
			{
				pingStatistic = new PingStatistic
				{
					StartInterval = now,
					StartIntervalUTC = now.ToUniversalTime(),
					PingSuccess = pingSuccess
				};
				pingStatistics.Add(pingStatistic);
			}
			pingStatistic.EndInterval = now;
			pingStatistic.EndIntervalUTC = now.ToUniversalTime();
			pingStatistic.PingCount++;
			result = await SavePingStatistics(pingStatistics);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"Failed to record ping attempt: {arg}", "RecordPingStatistic", "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
			result = false;
		}
		return result;
	}

	public async Task<PingStatisticsResponse> GetPingStatisticsResponse()
	{
		PingStatisticsResponse response = new PingStatisticsResponse();
		try
		{
			PingStatisticsResponse pingStatisticsResponse = response;
			pingStatisticsResponse.PingStatistics = await GetPingStatistics();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception getting Ping Statistics Response", "GetPingStatisticsResponse", "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
			response.StatusCode = HttpStatusCode.InternalServerError;
		}
		return response;
	}

	public async Task<LastSuccessfulPingResponse> GetLastSuccessfulPing()
	{
		LastSuccessfulPingResponse response = new LastSuccessfulPingResponse();
		try
		{
			response.LastSuccessfulPingUTC = (await GetPingStatistics())?.GetLastSuccessful()?.EndIntervalUTC;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception getting last successful ping", "GetLastSuccessfulPing", "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
			response.StatusCode = HttpStatusCode.InternalServerError;
		}
		return response;
	}

	private async Task<PingStatistics> GetPingStatistics()
	{
		if (_pingStatistics == null)
		{
			PersistentDataWrapper<PingStatistics> persistentDataWrapper = await _persistentDataCacheService.Read<PingStatistics>("PingStatistics.json", useCache: false, null, log: false);
			if (persistentDataWrapper?.Data != null)
			{
				_pingStatistics = persistentDataWrapper.Data;
			}
			else
			{
				_pingStatistics = new PingStatistics();
			}
		}
		return _pingStatistics;
	}

	private async Task<bool> SavePingStatistics(PingStatistics pingStatistics)
	{
		return await _persistentDataCacheService.Write(pingStatistics, "PingStatistics.json");
	}

	public async Task<bool> ReportFailedPing()
	{
		bool result = true;
		try
		{
			if (DateTime.Now > _initialReportFailedPingAttempt.AddMinutes(30.0))
			{
				PingStatistics pingStatistics = await GetPingStatistics();
				PingStatistic latest = pingStatistics.GetLatest();
				DateTime? dateTime = pingStatistics?.GetLastSuccessful()?.EndIntervalUTC;
				if (dateTime.HasValue && dateTime.Value.AddMinutes(60.0) < DateTime.UtcNow && latest != null && !latest.PingSuccess)
				{
					if (_kioskConfiguration.KioskHealth.ReportFailedPingsEnabled)
					{
						APIResponse<ApiBaseResponse> aPIResponse = await _proxyApi.RecordKioskPingFailure(new KioskPingFailure
						{
							KioskId = _storeService.KioskId,
							LastSuccessfulPingUTC = dateTime.Value,
							LastUpdateUTC = DateTime.UtcNow
						});
						result = aPIResponse != null && aPIResponse.StatusCode == HttpStatusCode.OK;
					}
					else
					{
						_logger.LogInfoWithSource("Skipping reporting of failed ping because config setting ReportFailedPingsEnabled is turned off", "ReportFailedPing", "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception reporting failed ping.", "ReportFailedPing", "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
			result = false;
		}
		return result;
	}
}
