using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.IoT.DownloadFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class GetDownloadFileJobStatusV2 : ICommandIoTController
{
	private readonly IDownloadFilesService _downloadFilesService;

	private readonly IStoreService _store;

	private readonly ILogger<GetDownloadFileJobStatusV2> _logger;

	private readonly IMqttProxy _mqtt;

	public CommandEnum CommandEnum => CommandEnum.GetDownloadFileJobStatus;

	public int Version => 2;

	public GetDownloadFileJobStatusV2(IDownloadFilesService downloadFilesService, IStoreService store, ILogger<GetDownloadFileJobStatusV2> logger, IMqttProxy mqtt)
	{
		_downloadFilesService = downloadFilesService;
		_store = store;
		_logger = logger;
		_mqtt = mqtt;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		MqttResponse<List<DownloadData>> result = new MqttResponse<List<DownloadData>>();
		try
		{
			string bitsJobId = ioTCommand.Payload?.ToString();
			MqttResponse<List<DownloadData>> mqttResponse = result;
			mqttResponse.Data = await _downloadFilesService.GetDownloadFileJobStatus(bitsJobId);
		}
		catch (Exception ex)
		{
			result.Error = ex.GetFullMessage();
			_logger.LogErrorWithSource(ex, "An unhandled exception occurred while getting download file job status for request " + ioTCommand.RequestId, "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/GetDownloadFileJobStatusV2.cs");
		}
		try
		{
			_logger.LogInfoWithSource("Publishing response to requestId " + ioTCommand.RequestId + " -> " + result.ToJson(), "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/GetDownloadFileJobStatusV2.cs");
			await _mqtt.PublishIoTCommandAsync("redbox/updateservice-instance/" + ioTCommand.SourceId + "/request", new IoTCommandModel
			{
				RequestId = ioTCommand.RequestId,
				Command = CommandEnum,
				Version = Version,
				MessageType = MessageTypeEnum.Response,
				SourceId = _store.KioskId.ToString(),
				Payload = result.ToJson()
			});
		}
		catch (Exception ex2)
		{
			_logger.LogErrorWithSource(ex2, "An unhandled exception occurred while publishing the IoT response", "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/GetDownloadFileJobStatusV2.cs");
		}
	}
}
