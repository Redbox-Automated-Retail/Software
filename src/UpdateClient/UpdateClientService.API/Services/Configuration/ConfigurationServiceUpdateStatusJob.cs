using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Configuration;

public class ConfigurationServiceUpdateStatusJob : IConfigurationServiceUpdateStatusJob, IInvocable
{
	private IConfigurationService _configurationService;

	private ILogger<ConfigurationServiceUpdateStatusJob> _logger;

	public ConfigurationServiceUpdateStatusJob(ILogger<ConfigurationServiceUpdateStatusJob> logger, IConfigurationService configurationService)
	{
		_configurationService = configurationService;
		_logger = logger;
	}

	public async Task Invoke()
	{
		_logger.LogInfoWithSource("Invoking ConfigurationService.UpdateConfigurationStatusIfNeeded", "Invoke", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationServiceUpdateStatusJob.cs");
		await _configurationService.UpdateConfigurationStatusIfNeeded();
	}
}
