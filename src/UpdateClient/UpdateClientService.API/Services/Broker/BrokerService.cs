using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Controllers.Models;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.Broker;

public class BrokerService : IBrokerService
{
	private readonly IIoTCommandClient _ioTCommandClient;

	private readonly ILogger<BrokerService> _logger;

	private readonly IPingStatisticsService _pingStatisticsService;

	public BrokerService(ILogger<BrokerService> logger, IIoTCommandClient ioTCommandClient, IPingStatisticsService pingStatisticsService)
	{
		_logger = logger;
		_ioTCommandClient = ioTCommandClient;
		_pingStatisticsService = pingStatisticsService;
	}

	public async Task<IActionResult> Ping(string appName, string messageId, PingRequest pingRequest)
	{
		_ = 1;
		try
		{
			PerformIoTCommandParameters parameters = new PerformIoTCommandParameters
			{
				IoTTopic = "$aws/rules/kioskping",
				WaitForResponse = false
			};
			IoTCommandResponse<ObjectResult> ioTCommandResponse = await _ioTCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
			{
				Version = 1,
				RequestId = messageId,
				AppName = appName,
				Command = CommandEnum.KioskPing,
				Payload = pingRequest,
				QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce
			}, parameters);
			await _pingStatisticsService.RecordPingStatistic(ioTCommandResponse != null && ioTCommandResponse.StatusCode == 200);
			return ProcessIoTCommandResponse(ioTCommandResponse, parameters);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception executing Ping", "Ping", "/sln/src/UpdateClientService.API/Services/Broker/BrokerService.cs");
			return new StatusCodeResult(500);
		}
	}

	public async Task<IActionResult> Register(string appName, string messageId, RegisterRequest request)
	{
		try
		{
			PerformIoTCommandParameters parameters = new PerformIoTCommandParameters
			{
				IoTTopic = "$aws/rules/kioskrestcall",
				WaitForResponse = true
			};
			return ProcessIoTCommandResponse(await _ioTCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
			{
				Version = 1,
				RequestId = messageId,
				AppName = appName,
				Command = CommandEnum.BrokerRegister,
				Payload = request,
				QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce
			}, parameters), parameters);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception executing Register", "Register", "/sln/src/UpdateClientService.API/Services/Broker/BrokerService.cs");
			return new StatusCodeResult(500);
		}
	}

	public async Task<IActionResult> Unregister(string appName, string messageId, UnRegisterRequest unregisterRequest)
	{
		try
		{
			PerformIoTCommandParameters parameters = new PerformIoTCommandParameters
			{
				IoTTopic = "$aws/rules/kioskrestcall",
				WaitForResponse = true
			};
			return ProcessIoTCommandResponse(await _ioTCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
			{
				Version = 1,
				RequestId = messageId,
				AppName = appName,
				Command = CommandEnum.BrokerUnRegister,
				Payload = unregisterRequest,
				QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce
			}, parameters), parameters);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception executing Unregister", "Unregister", "/sln/src/UpdateClientService.API/Services/Broker/BrokerService.cs");
			return new StatusCodeResult(500);
		}
	}

	private StatusCodeResult ProcessIoTCommandResponse<T>(IoTCommandResponse<T> response, PerformIoTCommandParameters parameters = null) where T : ObjectResult
	{
		if (parameters == null || parameters.WaitForResponse)
		{
			return ToStatusCodeResult(response.Payload);
		}
		ObjectResult objectResult = new ObjectResult(null);
		objectResult.StatusCode = response?.StatusCode ?? 500;
		return ToStatusCodeResult(objectResult);
	}

	private StatusCodeResult ToStatusCodeResult(object objectResult)
	{
		return new StatusCodeResult((objectResult as ObjectResult)?.StatusCode ?? 500);
	}
}
