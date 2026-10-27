using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class TriggerUpdateConfigStatus : ICommandIoTController
{
	private readonly IConfigurationService _configurationService;

	private readonly ILogger<TriggerUpdateConfigStatus> _logger;

	public CommandEnum CommandEnum { get; } = CommandEnum.TriggerUpdateConfigStatus;

	public int Version { get; } = 2;

	public TriggerUpdateConfigStatus(IConfigurationService configurationService, ILogger<TriggerUpdateConfigStatus> logger)
	{
		_configurationService = configurationService;
		_logger = logger;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		TriggerUpdateConfigStatusRequest triggerUpdateConfigStatusRequest = JsonConvert.DeserializeObject<TriggerUpdateConfigStatusRequest>(ioTCommand.Payload.ToJson());
		try
		{
			await _configurationService.TriggerUpdateConfigurationStatus(triggerUpdateConfigStatusRequest);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception trying to trigger UpdateConfigurationStatus", "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/TriggerUpdateConfigStatus.cs");
		}
	}
}
