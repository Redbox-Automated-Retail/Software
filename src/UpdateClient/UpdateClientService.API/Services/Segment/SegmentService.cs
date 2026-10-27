using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.Segment;

public class SegmentService : ISegmentService
{
	private class KioskSegmentsData : List<KioskSegmentModel>, IPersistentData
	{
	}

	private readonly ILogger<SegmentService> _logger;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private readonly IIoTCommandClient _iotCommandClient;

	private readonly IKioskConfiguration _kioskConfiguration;

	private const string SegmentsFileName = "segments.json";

	private int _processingSegments;

	public SegmentService(ILogger<SegmentService> logger, IIoTCommandClient ioTCommandClient, IPersistentDataCacheService persistentDataCacheService, IOptionsSnapshotKioskConfiguration kioskConfiguration)
	{
		_logger = logger;
		_iotCommandClient = ioTCommandClient;
		_persistentDataCacheService = persistentDataCacheService;
		_kioskConfiguration = kioskConfiguration;
	}

	public async Task<KioskSegmentsResponse> GetKioskSegments()
	{
		KioskSegmentsResponse response = new KioskSegmentsResponse();
		try
		{
			PersistentDataWrapper<KioskSegmentsData> persistentDataWrapper = await GetKioskSegmentsFromPersistentDataCache();
			if (persistentDataWrapper?.Data != null)
			{
				response.Segments.AddRange(persistentDataWrapper.Data);
			}
			else
			{
				response.StatusCode = HttpStatusCode.NotFound;
			}
		}
		catch (Exception ex)
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, "Exception while getting kiosk segments", "GetKioskSegments", "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
		}
		return response;
	}

	private async Task<PersistentDataWrapper<KioskSegmentsData>> GetKioskSegmentsFromPersistentDataCache()
	{
		PersistentDataWrapper<KioskSegmentsData> result = new PersistentDataWrapper<KioskSegmentsData>();
		try
		{
			PersistentDataWrapper<KioskSegmentsData> persistentDataWrapper = await _persistentDataCacheService.Read<KioskSegmentsData>("segments.json", useCache: true, null, log: false);
			if (persistentDataWrapper != null)
			{
				result = persistentDataWrapper;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception getting kiosk segments", "GetKioskSegmentsFromPersistentDataCache", "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
		}
		return result;
	}

	public async Task<ApiBaseResponse> UpdateKioskSegmentsFromUpdateService()
	{
		ApiBaseResponse response = new ApiBaseResponse();
		try
		{
			if (!_kioskConfiguration.Operations.SegmentUpdateEnabled)
			{
				response.StatusCode = HttpStatusCode.ServiceUnavailable;
				return response;
			}
			IoTCommandResponse<ObjectResult> ioTCommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
			{
				Version = 1,
				RequestId = Guid.NewGuid().ToString(),
				Command = CommandEnum.GetKioskSegments,
				QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
				LogResponse = false
			}, new PerformIoTCommandParameters
			{
				IoTTopic = "$aws/rules/kioskrestcall",
				WaitForResponse = true
			});
			if (ioTCommandResponse != null && ioTCommandResponse.StatusCode == 200)
			{
				KioskSegmentsResponse kioskSegmentsResponse = JsonConvert.DeserializeObject<KioskSegmentsResponse>(ioTCommandResponse?.Payload?.Value?.ToString());
				if (kioskSegmentsResponse != null)
				{
					KioskSegmentsData kioskSegmentsData = new KioskSegmentsData();
					kioskSegmentsData.AddRange(kioskSegmentsResponse.Segments);
					if (await UpdateKioskSegmentsFile(kioskSegmentsData))
					{
						response.StatusCode = HttpStatusCode.OK;
					}
					else
					{
						response.StatusCode = HttpStatusCode.InternalServerError;
					}
				}
				else
				{
					response.StatusCode = HttpStatusCode.InternalServerError;
					_logger.LogErrorWithSource("Unable to deserialize Iot command payload as KioskSegmentsResponse", "UpdateKioskSegmentsFromUpdateService", "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
				}
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				_logger.LogErrorWithSource($"Iot command {CommandEnum.GetKioskSegments} returned statuscode {ioTCommandResponse?.StatusCode}", "UpdateKioskSegmentsFromUpdateService", "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting kiosk segments.", "UpdateKioskSegmentsFromUpdateService", "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
			response.StatusCode = HttpStatusCode.InternalServerError;
		}
		return response;
	}

	private async Task<bool> UpdateKioskSegmentsFile(KioskSegmentsData kioskSegmentsData)
	{
		bool result = false;
		if (Interlocked.CompareExchange(ref _processingSegments, 1, 0) == 1)
		{
			_logger.LogInfoWithSource("Prevented attempt to process kiosk segments.", "UpdateKioskSegmentsFile", "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
			return result;
		}
		try
		{
			result = await _persistentDataCacheService.Write(kioskSegmentsData, "segments.json");
		}
		finally
		{
			_processingSegments = 0;
		}
		return result;
	}

	public async Task<bool> UpdateKioskSegmentsIfNeeded()
	{
		bool response = false;
		try
		{
			PersistentDataWrapper<KioskSegmentsData> persistentDataWrapper = await GetKioskSegmentsFromPersistentDataCache();
			if (persistentDataWrapper == null || IsKioskSegmentDataExpired(persistentDataWrapper))
			{
				ApiBaseResponse apiBaseResponse = await UpdateKioskSegmentsFromUpdateService();
				if (apiBaseResponse != null && apiBaseResponse.StatusCode == HttpStatusCode.OK)
				{
					response = true;
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception updating kiosk segments", "UpdateKioskSegmentsIfNeeded", "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
		}
		return response;
	}

	private bool IsKioskSegmentDataExpired(PersistentDataWrapper<KioskSegmentsData> persistentDataWrapper)
	{
		return (DateTime.Now - persistentDataWrapper.Modified).TotalHours > (double)_kioskConfiguration.Operations.SegmentUpdateFrequencyHours;
	}
}
