using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class GetIoTStatisticsSummary : ICommandIoTController
{
	private readonly ILogger<GetIoTStatisticsSummary> _logger;

	private readonly IStoreService _store;

	private readonly IMqttProxy _mqttProxy;

	private readonly IIoTStatisticsService _ioTStatisticsService;

	public CommandEnum CommandEnum => CommandEnum.GetIotStatisticsSummary;

	public int Version => 2;

	public GetIoTStatisticsSummary(ILogger<GetIoTStatisticsSummary> logger, IIoTStatisticsService ioTStatisticsService, IStoreService store, IMqttProxy mqttProxy)
	{
		_logger = logger;
		_ioTStatisticsService = ioTStatisticsService;
		_store = store;
		_mqttProxy = mqttProxy;
	}

	public async Task Execute(IoTCommandModel ioTCommandRequest)
	{
		_ = 1;
		try
		{
			_logger.LogInfoWithSource("Getting IoTStatisticsSummary", "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/GetIoTStatisticsSummary.cs");
			IoTStatisticsSummaryResponse ioTStatisticsSummaryResponse = await _ioTStatisticsService.GetIoTStatisticsSummaryResponse();
			MqttResponse<List<IoTStatisticsSummary>> mqttResponse = new MqttResponse<List<IoTStatisticsSummary>>();
			if (ioTStatisticsSummaryResponse != null && ioTStatisticsSummaryResponse.StatusCode == HttpStatusCode.OK && ioTStatisticsSummaryResponse.ioTStatisticsSummaries != null)
			{
				mqttResponse.Data = ioTStatisticsSummaryResponse.ioTStatisticsSummaries;
			}
			else
			{
				mqttResponse.Error = "Error getting IoTStatisticsSummary";
			}
			string payload = mqttResponse.ToJson();
			await _mqttProxy.PublishIoTCommandAsync("redbox/updateservice-instance/" + ioTCommandRequest.SourceId + "/request", new IoTCommandModel
			{
				RequestId = ioTCommandRequest.RequestId,
				Command = CommandEnum,
				MessageType = MessageTypeEnum.Response,
				Version = Version,
				SourceId = _store?.KioskId.ToString(),
				Payload = payload,
				LogRequest = true
			});
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception getting IoT statistics summary", "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/GetIoTStatisticsSummary.cs");
		}
	}
}
