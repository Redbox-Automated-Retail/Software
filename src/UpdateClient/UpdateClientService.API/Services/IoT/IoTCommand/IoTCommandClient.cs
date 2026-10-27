using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands;

namespace UpdateClientService.API.Services.IoT.IoTCommand;

public class IoTCommandClient : IIoTCommandClient
{
	private readonly IMqttProxy _mqttRepo;

	private readonly IStoreService _store;

	private readonly AppSettings _appSettings;

	private readonly ILogger<IoTCommandClient> _logger;

	private readonly IActiveResponseService _activeResponseService;

	private readonly TimeSpan _requestTimeoutDefault = TimeSpan.FromSeconds(20.0);

	public IoTCommandClient(IMqttProxy mqttRepo, IStoreService store, IOptions<AppSettings> appSettings, ILogger<IoTCommandClient> logger, IActiveResponseService activeResponseService)
	{
		_mqttRepo = mqttRepo;
		_store = store;
		_appSettings = appSettings.Value;
		_logger = logger;
		_activeResponseService = activeResponseService;
	}

	public async Task<string> PerformIoTCommandWithStringResult(IoTCommandModel request, PerformIoTCommandParameters parameters)
	{
		return (await PerformIoTCommand(request, parameters))?.Payload?.ToString();
	}

	public async Task<IoTCommandResponse<T>> PerformIoTCommand<T>(IoTCommandModel request, PerformIoTCommandParameters parameters = null) where T : ObjectResult
	{
		T resultPayload = null;
		if (parameters == null)
		{
			parameters = new PerformIoTCommandParameters();
		}
		IoTCommandModel ioTCommandModel = await PerformIoTCommand(request, parameters);
		if (parameters.WaitForResponse)
		{
			try
			{
				resultPayload = JsonConvert.DeserializeObject<T>(ioTCommandModel?.Payload?.ToString());
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Couldn't deserialize the response from UpdateService for request id " + request?.RequestId, "PerformIoTCommand", "/sln/src/UpdateClientService.API/Services/IoT/IoTCommand/IoTCommandClient.cs");
			}
			return new IoTCommandResponse<T>
			{
				StatusCode = (resultPayload?.StatusCode ?? 500),
				Payload = resultPayload
			};
		}
		bool.TryParse(ioTCommandModel?.Payload?.ToString(), out var result);
		int statusCode = (result ? 200 : 500);
		return new IoTCommandResponse<T>
		{
			StatusCode = statusCode
		};
	}

	private async Task<IoTCommandModel> PerformIoTCommand(IoTCommandModel request, PerformIoTCommandParameters parameters)
	{
		if (string.IsNullOrWhiteSpace(request.SourceId))
		{
			request.SourceId = _store.KioskId.ToString();
		}
		AdjustIoTTopicIfNeeded(request, parameters);
		if (parameters?.WaitForResponse ?? false)
		{
			return await PublishIoTCommandAndWaitForResponse(request, parameters);
		}
		return await PublishIoTCommandAndReturn(request, parameters);
	}

	private async Task<IoTCommandModel> PublishIoTCommandAndReturn(IoTCommandModel request, PerformIoTCommandParameters parameters)
	{
		bool flag = await PublishIoTCommandAsync(request, parameters);
		return new IoTCommandModel
		{
			RequestId = request.RequestId,
			MessageType = MessageTypeEnum.Response,
			Command = request.Command,
			Version = request.Version,
			Payload = flag.ToString(),
			SourceId = _store.KioskId.ToString()
		};
	}

	private async Task<IoTCommandModel> PublishIoTCommandAndWaitForResponse(IoTCommandModel request, PerformIoTCommandParameters parameters)
	{
		IoTCommandModel response = null;
		AsyncManualResetEvent mre = new AsyncManualResetEvent(set: false);
		try
		{
			_activeResponseService.AddResponseListener(request.RequestId, delegate(IoTCommandModel responseIotCommandModel)
			{
				response = responseIotCommandModel;
				mre.Set();
				_logger.LogInfoWithSource($"Received response from update service for request id {request?.RequestId}, IotCommand: {request?.Command},  IoT Topic: {parameters?.IoTTopic}.", "PublishIoTCommandAndWaitForResponse", "/sln/src/UpdateClientService.API/Services/IoT/IoTCommand/IoTCommandClient.cs");
			});
			TimeSpan iotCommandTimeout = parameters.RequestTimeout ?? _requestTimeoutDefault;
			if (await PublishIoTCommandAsync(request, parameters))
			{
				await mre.WaitAsyncAndDisposeOnFinish(iotCommandTimeout, request);
			}
			if (response == null)
			{
				response = new IoTCommandModel
				{
					RequestId = request.RequestId,
					MessageType = MessageTypeEnum.Response,
					Command = request.Command,
					Version = request.Version,
					Payload = new StatusCodeResult(504).ToJson(),
					SourceId = _store.KioskId.ToString()
				};
			}
		}
		finally
		{
			_activeResponseService.RemoveResponseListener(request.RequestId);
		}
		return response;
	}

	private async Task<bool> PublishIoTCommandAsync(IoTCommandModel request, PerformIoTCommandParameters parameters)
	{
		return await _mqttRepo.PublishIoTCommandAsync(parameters?.IoTTopic, request);
	}

	private void AdjustIoTTopicIfNeeded(IoTCommandModel request, PerformIoTCommandParameters parameters)
	{
		if (string.IsNullOrEmpty(parameters?.IoTTopic))
		{
			string instanceString = GetInstanceString(request?.ReturnServerId);
			parameters.IoTTopic = "redbox/updateservice-instance/" + instanceString + "/request";
			_logger.LogInfoWithSource("Using IoT Topic: " + parameters.IoTTopic + " for request id: " + request.RequestId, "AdjustIoTTopicIfNeeded", "/sln/src/UpdateClientService.API/Services/IoT/IoTCommand/IoTCommandClient.cs");
		}
	}

	private string GetInstanceString(string stringInstanceNumberOverride = null)
	{
		return stringInstanceNumberOverride;
	}
}
