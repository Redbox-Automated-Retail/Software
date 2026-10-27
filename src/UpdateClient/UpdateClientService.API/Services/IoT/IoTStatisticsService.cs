using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.IoT;

public class IoTStatisticsService : IIoTStatisticsService
{
	private ILogger<IoTStatisticsService> _logger;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private IoTStatistics _iotStatistics;

	private const string IoTStatisticsFileName = "IoTStatistics.json";

	public IoTStatisticsService(ILogger<IoTStatisticsService> logger, IPersistentDataCacheService persistentDataCacheService)
	{
		_logger = logger;
		_persistentDataCacheService = persistentDataCacheService;
	}

	public async Task<IoTStatisticsSummaryResponse> GetIoTStatisticsSummaryResponse()
	{
		IoTStatisticsSummaryResponse response = new IoTStatisticsSummaryResponse();
		try
		{
			IoTStatistics ioTStatistics = await GetIoTStatistics();
			if (ioTStatistics?.ConnectionAttempts?.Any() == true)
			{
				foreach (IoTConnectionAttempt connectionAttempt in ioTStatistics.ConnectionAttempts)
				{
					if (connectionAttempt.Connected.HasValue && !connectionAttempt.Disconnected.HasValue)
					{
						connectionAttempt.Disconnected = DateTime.Now;
					}
				}
				IEnumerable<IGrouping<DateTime, IoTConnectionAttempt>> enumerable = from x in ioTStatistics.ConnectionAttempts
					group x by x.StartConnectionAttempt.Value.Date;
				foreach (IGrouping<DateTime, IoTConnectionAttempt> item in enumerable)
				{
					IoTStatisticsSummary ioTStatisticsSummary = new IoTStatisticsSummary
					{
						Date = item.Key.ToShortDateString()
					};
					Dictionary<string, int> dictionary = new Dictionary<string, int>();
					ioTStatisticsSummary.ConnectionCount = item.Count((IoTConnectionAttempt x) => x.Connected.HasValue);
					ioTStatisticsSummary.ConnectionAttemptsCount = item.Sum((IoTConnectionAttempt x) => x.ConnectionAttempts);
					foreach (IoTConnectionAttempt item2 in item)
					{
						if (item2.ConnectionAttemptsDuration.HasValue)
						{
							ioTStatisticsSummary.TotalConnectionAttemptsDuration += item2.ConnectionAttemptsDuration.Value;
						}
						if (!(item2.ConnectionExceptions?.Any() ?? false))
						{
							continue;
						}
						foreach (KeyValuePair<string, int> connectionException in item2.ConnectionExceptions)
						{
							int value = 0;
							dictionary.TryGetValue(connectionException.Key, out value);
							value += connectionException.Value;
							dictionary[connectionException.Key] = value;
						}
					}
					if (dictionary.Count > 0)
					{
						ioTStatisticsSummary.ConnectionExceptions = new List<ConnectionException>();
						foreach (KeyValuePair<string, int> item3 in dictionary)
						{
							ioTStatisticsSummary.ConnectionExceptions.Add(new ConnectionException
							{
								ExceptionMessage = item3.Key,
								Count = item3.Value
							});
						}
					}
					response.ioTStatisticsSummaries.Add(ioTStatisticsSummary);
				}
			}
		}
		catch (Exception ex)
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, "Exception getting IotStatisticsSummaryResponse", "GetIoTStatisticsSummaryResponse", "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
		}
		return response;
	}

	public async Task<bool> RecordConnectionAttempt()
	{
		bool result;
		try
		{
			IoTStatistics ioTStatistics = await GetIoTStatistics();
			ioTStatistics.Cleanup();
			ioTStatistics.StartConnectionAttempt();
			result = await SaveIoTStatistics(ioTStatistics);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"Failed to record connection attempt: {arg}", "RecordConnectionAttempt", "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
			result = false;
		}
		return result;
	}

	public async Task<bool> RecordConnectionSuccess()
	{
		bool result;
		try
		{
			IoTStatistics ioTStatistics = await GetIoTStatistics();
			ioTStatistics.Connected();
			result = await SaveIoTStatistics(ioTStatistics);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"Failed to record connection success: {arg}", "RecordConnectionSuccess", "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
			result = false;
		}
		return result;
	}

	public async Task<bool> RecordDisconnection()
	{
		bool result;
		try
		{
			IoTStatistics ioTStatistics = await GetIoTStatistics();
			ioTStatistics.Disconnected();
			result = await SaveIoTStatistics(ioTStatistics);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"Failed to record connection disconnect: {arg}", "RecordDisconnection", "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
			result = false;
		}
		return result;
	}

	public async Task<bool> RecordConnectionException(string exceptionMessage)
	{
		bool result;
		try
		{
			IoTStatistics ioTStatistics = await GetIoTStatistics();
			ioTStatistics.AddException(exceptionMessage);
			result = await SaveIoTStatistics(ioTStatistics);
		}
		catch (Exception arg)
		{
			_logger.LogErrorWithSource($"Failed to record connection disconnect: {arg}", "RecordConnectionException", "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
			result = false;
		}
		return result;
	}

	private async Task<IoTStatistics> GetIoTStatistics()
	{
		if (_iotStatistics == null)
		{
			PersistentDataWrapper<IoTStatistics> persistentDataWrapper = await _persistentDataCacheService.Read<IoTStatistics>("IoTStatistics.json", useCache: false, null, log: false);
			if (persistentDataWrapper?.Data != null)
			{
				_iotStatistics = persistentDataWrapper.Data;
			}
			else
			{
				_iotStatistics = new IoTStatistics();
			}
		}
		return _iotStatistics;
	}

	private async Task<bool> SaveIoTStatistics(IoTStatistics ioTStatistics)
	{
		return await _persistentDataCacheService.Write(ioTStatistics, "IoTStatistics.json");
	}
}
