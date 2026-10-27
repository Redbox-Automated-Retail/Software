using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class TriggerGetConfigChanges : ICommandIoTController
{
	private readonly IConfigurationService _configurationService;

	private readonly ILogger<TriggerGetConfigChanges> _logger;

	public CommandEnum CommandEnum { get; } = CommandEnum.TriggerGetConfigChanges;

	public int Version { get; } = 2;

	public TriggerGetConfigChanges(IConfigurationService configurationService, ILogger<TriggerGetConfigChanges> logger)
	{
		_configurationService = configurationService;
		_logger = logger;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		TriggerGetConfigChangesRequest triggerGetConfigChangesRequest = JsonConvert.DeserializeObject<TriggerGetConfigChangesRequest>(ioTCommand.Payload.ToJson());
		try
		{
			await _configurationService.TriggerGetKioskConfigurationSettingChanges(triggerGetConfigChangesRequest);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception trying to trigger GetKioskConfigurationSettingChanges", "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/TriggerGetConfigChanges.cs");
		}
	}
}
