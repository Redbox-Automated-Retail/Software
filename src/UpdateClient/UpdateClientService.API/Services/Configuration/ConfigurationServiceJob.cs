using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Configuration;

public class ConfigurationServiceJob : IConfigurationServiceJob, IInvocable
{
	private IConfigurationService _configurationService;

	private ILogger<ConfigurationServiceJob> _logger;

	public ConfigurationServiceJob(ILogger<ConfigurationServiceJob> logger, IConfigurationService configurationService)
	{
		_configurationService = configurationService;
		_logger = logger;
	}

	public async Task Invoke()
	{
		_logger.LogInfoWithSource("Invoking ConfigurationService.GetKioskConfigurationSettingChanges", "Invoke", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationServiceJob.cs");
		await _configurationService.GetKioskConfigurationSettingChanges(null);
	}
}
