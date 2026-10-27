using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands.Controller;

namespace UpdateClientService.API.Services.IoT.Commands;

public class IoTCommandService : IIoTCommandService
{
	private readonly List<ICommandIoTController> _commandList;

	private readonly ILogger<IoTCommandService> _logger;

	private readonly IActiveResponseService _activeResponseService;

	public IoTCommandService(IActiveResponseService activeResponseService, List<ICommandIoTController> commandList, ILogger<IoTCommandService> logger)
	{
		_activeResponseService = activeResponseService;
		_commandList = commandList;
		_logger = logger;
	}

	public async Task Execute(byte[] message, string topic)
	{
		await IoTCommandCallbackMethodAsync(message, topic);
	}

	private async Task IoTCommandCallbackMethodAsync(byte[] mqttMessage, string mqttTopic)
	{
		_ = 1;
		try
		{
			string text = Encoding.UTF8.GetString(mqttMessage);
			IoTCommandModel ioTCommandModel = JsonConvert.DeserializeObject<IoTCommandModel>(text);
			if (ioTCommandModel == null)
			{
				_logger.LogErrorWithSource("Error occured in callback from topic " + mqttTopic + ", message failed deserialization: " + text, "IoTCommandCallbackMethodAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
				return;
			}
			string text2 = (ioTCommandModel.LogPayload ? (", message: " + text) : $", command: {ioTCommandModel.Command}, MessageType: {ioTCommandModel.MessageType}, RequestId: {ioTCommandModel.RequestId}, Payload length: {(ioTCommandModel.Payload?.ToString()?.Length).GetValueOrDefault()}");
			_logger.LogInfoWithSource("message received topic: " + mqttTopic + text2, "IoTCommandCallbackMethodAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
			if (ioTCommandModel.MessageType == MessageTypeEnum.Request)
			{
				await ExecuteRequest(ioTCommandModel);
			}
			else if (ioTCommandModel.MessageType == MessageTypeEnum.Response)
			{
				await ExecuteResponse(ioTCommandModel);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error occured in callback from topic " + mqttTopic + ", message was: " + Encoding.UTF8.GetString(mqttMessage), "IoTCommandCallbackMethodAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
		}
	}

	private async Task ExecuteRequest(IoTCommandModel iotCommandModel)
	{
		ICommandIoTController requestedCommand = _commandList?.FirstOrDefault((ICommandIoTController x) => x.CommandEnum == iotCommandModel.Command && x.Version == iotCommandModel.Version);
		if (requestedCommand == null)
		{
			_logger.LogErrorWithSource($"IoT Command: {iotCommandModel.Command}, Version: {iotCommandModel.Version} could not found in registered command list: {_commandList?.ToJson()}.", "ExecuteRequest", "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
			return;
		}
		_logger.LogInfoWithSource($"Processing command: {requestedCommand.CommandEnum} for RequestId: {iotCommandModel.RequestId}", "ExecuteRequest", "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
		await requestedCommand.Execute(iotCommandModel);
		_logger.LogInfoWithSource($"Finished processing command: {requestedCommand.CommandEnum} for RequestId: {iotCommandModel.RequestId}. Returning. . .", "ExecuteRequest", "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
	}

	private async Task ExecuteResponse(IoTCommandModel iotCommandModel)
	{
		Action<IoTCommandModel> responseListenerAction = _activeResponseService.GetResponseListenerAction(iotCommandModel.RequestId);
		if (responseListenerAction == null)
		{
			_logger.LogInfoWithSource("ActiveResponseListener could not be found for RequestId: " + iotCommandModel.RequestId + ".  Ignoring response.", "ExecuteResponse", "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
			return;
		}
		_logger.LogInfoWithSource($"Processing Response Type: {iotCommandModel.Command} for RequestId: {iotCommandModel.RequestId}", "ExecuteResponse", "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
		responseListenerAction?.Invoke(iotCommandModel);
		_activeResponseService.RemoveResponseListener(iotCommandModel.RequestId);
	}
}
